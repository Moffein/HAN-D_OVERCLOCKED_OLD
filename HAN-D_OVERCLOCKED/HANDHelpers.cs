using R2API.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED
{
    public class HandDroneOrb : GenericDamageOrb
    {
        public override void Begin()
        {
            this.speed = 120f;
            base.Begin();
        }

        protected override GameObject GetOrbEffect()
        {
            return HAND_OVERCLOCKED.droneProjectileOrb;
        }
    }

    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    public class HANDController : MonoBehaviour
    {
        public void MeleeHit(int hitCount)
        {
            /*hitCount -= 1;
            float hc = hitCount * 2f;
            meleeHits += hc + 6 + characterBody.GetBuffCount(HAND_OVERCLOCKED.OverclockBuff)/2.5f;
            if (meleeHits >= meleeHitsMax)
            {
                meleeHits -= meleeHitsMax;
                if (characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
                {
                    characterBody.skillLocator.special.AddOneStock();
                }
            }*/
            meleeHits += 10;
            if (meleeHits >= meleeHitsMax)
            {
                meleeHits -= meleeHitsMax;
                if (characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
                {
                    characterBody.skillLocator.special.AddOneStock();
                }
            }
        }

        private void Awake()
        {
            this.indicator = new Indicator(base.gameObject, Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator"));
        }

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.teamComponent = base.GetComponent<TeamComponent>();

            characterBody.skillLocator.special.RemoveAllStocks();   //HAN-D special starts at 0 stocks
        }

        public HurtBox GetTrackingTarget()
        {
            return this.trackingTarget;
        }

        private void OnEnable()
        {
            this.indicator.active = true;
        }

        private void OnDisable()
        {
            this.indicator.active = false;
        }

        private void FixedUpdate()
        {
            if (characterBody.skillLocator.special.stock <= 0)
            {
                OnDisable();
            }
            else if (!this.indicator.active)
            {
                OnEnable();
            }

            this.trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
            {
                this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
                HurtBox hurtBox = this.trackingTarget;
                Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
                this.SearchForTarget(aimRay);
                this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
            this.search.filterByLoS = true;
            this.search.searchOrigin = aimRay.origin;
            this.search.searchDirection = aimRay.direction;
            this.search.sortMode = BullseyeSearch.SortMode.Angle;
            this.search.maxDistanceFilter = this.maxTrackingDistance;
            this.search.maxAngleFilter = this.maxTrackingAngle;
            this.search.RefreshCandidates();
            this.search.FilterOutGameObject(base.gameObject);
            this.trackingTarget = this.search.GetResults().FirstOrDefault<HurtBox>();
        }
        public float maxTrackingDistance = 100f;
        public float maxTrackingAngle = 360f;
        public float trackerUpdateFrequency = 10f;
        private HurtBox trackingTarget;
        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private float meleeHits = 0;
        public static float meleeHitsMax = 40;
    }

    public class HANDSwingAttack
    {
        public int Fire()
        {
            Collider[] array = Physics.OverlapBox(position, extents, orientation, LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                Collider collider = array[i];
                HurtBox component = collider.GetComponent<HurtBox>();
                if (component)
                {
                    HealthComponent healthComponent = component.healthComponent;
                    if (healthComponent && healthComponent != this.attacker && (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, this.teamIndex)))
                    {
                        HANDSwingAttack.HitPoint hitPoint = default(HANDSwingAttack.HitPoint);

                        hitPoint.hurtBox = component;
                        hitPoint.hitPosition = collider.transform.position;
                        hitPoint.hitNormal = this.position - hitPoint.hitPosition;
                        if (!HANDSwingAttack.bestHitPoints.ContainsKey(healthComponent))
                        {
                            HANDSwingAttack.bestHitPoints[healthComponent] = hitPoint;
                        }
                    }
                }
            }
            HANDSwingAttack.HitPoint[] array2 = new HANDSwingAttack.HitPoint[HANDSwingAttack.bestHitPoints.Count];
            int num2 = 0;
            foreach (KeyValuePair<HealthComponent, HANDSwingAttack.HitPoint> keyValuePair in HANDSwingAttack.bestHitPoints)
            {
                array2[num2++] = keyValuePair.Value;
            }
            HANDSwingAttack.bestHitPoints.Clear();
            foreach (HANDSwingAttack.HitPoint hitPoint2 in array2)
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.attacker = this.attacker;
                damageInfo.inflictor = this.inflictor;
                damageInfo.damage = this.baseDamage;
                damageInfo.crit = this.crit;
                damageInfo.force = this.force;
                damageInfo.procChainMask = this.procChainMask;
                damageInfo.procCoefficient = this.procCoefficient;
                damageInfo.damageType = this.damageType;
                damageInfo.damageColorIndex = this.damageColorIndex;
                damageInfo.position = hitPoint2.hitPosition;
                damageInfo.ModifyDamageInfo(hitPoint2.hurtBox.damageModifier);

                if (this.airbornVerticalForce != 0f || this.groundedVerticalForce != 0f || this.airbornHorizontalForceMult != 1f)
                {
                    CharacterBody cb = hitPoint2.hurtBox.healthComponent.gameObject.GetComponent<CharacterBody>();
                    if (cb)
                    {
                        bool airborn = false;
                        if (cb.isFlying)
                        {
                            if (this.overwriteVerticalVelocity && cb.characterMotor && cb.characterMotor.velocity.y > 0f)
                            {
                                cb.characterMotor.velocity.y = 0f;
                            }
                            damageInfo.force += this.airbornVerticalForce * Vector3.up;
                            damageInfo.force.x *= this.flyingHorizontalForceMult;
                            damageInfo.force.z *= this.flyingHorizontalForceMult;
                        }
                        else if (cb.characterMotor && !cb.characterMotor.isGrounded)
                        {
                            if (this.overwriteVerticalVelocity && cb.characterMotor && cb.characterMotor.velocity.y > 0f)
                            {
                                cb.characterMotor.velocity.y = 0f;
                            }
                            damageInfo.force += this.airbornVerticalForce * Vector3.up;
                            damageInfo.force.x *= this.airbornHorizontalForceMult;
                            damageInfo.force.z *= this.airbornHorizontalForceMult;
                            airborn = true;
                        }
                        else if (cb.characterMotor.isGrounded)
                        {
                            damageInfo.force += this.groundedVerticalForce * Vector3.up;
                        }
                    }
                }

                if (this.scaleForceMass)
                {
                    Rigidbody rb = hitPoint2.hurtBox.healthComponent.gameObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        damageInfo.force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), 6f);
                    }
                }

                if (NetworkServer.active)
                {
                    hitPoint2.hurtBox.healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, hitPoint2.hurtBox.healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, hitPoint2.hurtBox.healthComponent.gameObject);
                }
                else
                {
                    HANDNetworking.write.StartMessage(53);
                    HANDNetworking.write.Write(hitPoint2.hurtBox.healthComponent.gameObject);
                    HANDNetworking.WriteDmgInfo(HANDNetworking.write, damageInfo);
                    HANDNetworking.write.Write(hitPoint2.hurtBox.healthComponent != null);
                    HANDNetworking.write.FinishMessage();
                    ClientScene.readyConnection.SendWriter(HANDNetworking.write, QosChannelIndex.defaultReliable.intVal);
                }

                EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
                {
                    origin = hitPoint2.hitPosition
                }, false);
            }
            return array2.Length;
        }

        private struct HitPoint
        {
            public HurtBox hurtBox;
            public Vector3 hitPosition;
            public Vector3 hitNormal;
        }
        public GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/omniimpactvfx");
        public GameObject attacker;
        public GameObject inflictor;
        public TeamIndex teamIndex;
        public Vector3 position;
        public Vector3 extents;
        public Quaternion orientation;
        public float baseDamage;
        public Vector3 force;
        public bool crit;
        public bool scaleForceMass = false;
        public bool overwriteVerticalVelocity = false;
        public DamageType damageType;
        public DamageColorIndex damageColorIndex;
        public ProcChainMask procChainMask;
        public float procCoefficient = 1f;
        public float airbornHorizontalForceMult = 1f;
        public float flyingHorizontalForceMult = 1f;
        public float airbornVerticalForce = 0f;
        public float groundedVerticalForce = 1f;
        private static readonly Dictionary<HealthComponent, HANDSwingAttack.HitPoint> bestHitPoints = new Dictionary<HealthComponent, HANDSwingAttack.HitPoint>();
    }

    public static class HANDNetworking
    {
        //WriteDMGInfo Credits: Rein
        public static void WriteDmgInfo(NetworkWriter writer, DamageInfo damageInfo)
        {
            writer.Write(damageInfo.damage);
            writer.Write(damageInfo.crit);
            writer.Write(damageInfo.attacker);
            writer.Write(damageInfo.inflictor);
            writer.Write(damageInfo.position);
            writer.Write(damageInfo.force);
            writer.Write(damageInfo.procChainMask.mask);
            writer.Write(damageInfo.procCoefficient);
            writer.Write((byte)damageInfo.damageType);
            writer.Write((byte)damageInfo.damageColorIndex);
            writer.Write((byte)(damageInfo.dotIndex + 1));
        }
        public static NetworkWriter write = new NetworkWriter();
    }

    /*public static class OrbHelper
    {
        private static bool eventRegistered = false;
        private static List<Type> orbs = new List<Type>();

        public static bool AddOrb(Type t)
        {
            if (t == null || !t.IsSubclassOf(typeof(Orb)))
            {
                Debug.Log("Type is not based on Orb or is null");
                return false;
            }

            RegisterEvent();

            orbs.Add(t);

            return true;
        }

        private static void RegisterEvent()
        {
            if (eventRegistered)
            {
                return;
            }

            eventRegistered = true;

            On.RoR2.Orbs.OrbCatalog.GenerateCatalog += AddCustomOrbs;
        }

        private static void AddCustomOrbs(On.RoR2.Orbs.OrbCatalog.orig_GenerateCatalog orig)
        {
            orig();

            Type[] orbCat = typeof(OrbCatalog).GetFieldValue<Type[]>("indexToType");
            Dictionary<Type, int> typeToIndex = typeof(OrbCatalog).GetFieldValue<Dictionary<Type, int>>("typeToIndex");

            int origLength = orbCat.Length;
            int extraLength = orbs.Count;

            Array.Resize<Type>(ref orbCat, origLength + extraLength);

            int temp;

            for (int i = 0; i < extraLength; i++)
            {
                temp = i + origLength;
                orbCat[temp] = orbs[i];
                typeToIndex.Add(orbs[i], temp);
            }
        }
    }*/
}
