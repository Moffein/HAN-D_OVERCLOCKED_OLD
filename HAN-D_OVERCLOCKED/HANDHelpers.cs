using EntityStates.Engi.EngiWeapon;
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

    public struct HANDHitResult
    {
        public int hitCount;
        public bool hitBoss;
    }

    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    public class HANDController : MonoBehaviour
    {
        public void MeleeHit(HANDHitResult result)
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
            /*meleeHits += 1;
            if (meleeHits >= meleeHitsMax)
            {
                meleeHits = 0;
                if (characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
                {
                    characterBody.skillLocator.special.AddOneStock();
                }
            }
            characterBody.skillLocator.special.rechargeStopwatch = meleeHits;*/
            if (result.hitBoss)
            {
                meleeHits++;
                if (meleeHits >= meleeHitsMax)
                {
                    meleeHits = 0;
                    if (characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
                    {
                        characterBody.skillLocator.special.AddOneStock();
                    }
                }
            }
        }

        public void BeginOverclock(float duration)
        {
            //if (!characterBody.HasBuff(HAND_OVERCLOCKED.OverclockCooldownBuff))
            //{
                if (!characterBody.HasBuff(HAND_OVERCLOCKED.OverclockBuff))
                {
                    characterBody.AddBuff(HAND_OVERCLOCKED.OverclockBuff);
                    cancelledOVC = false;
                    ovcTimer = duration;
                    ovcActive = true;
                    Util.PlaySound("Play_MULT_shift_start", base.gameObject);
                    if (characterBody.skillLocator.utility.stock < characterBody.skillLocator.utility.maxStock)
                    {
                        characterBody.skillLocator.utility.stock++;
                    }
                }
                else
                {
                    cancelledOVC = true;
                    EndOverclock();
                }
            //}
        }

        private void EndOverclock()
        {
            ovcTimer = 0f;
            ovcActive = false;
            if (characterBody.HasBuff(HAND_OVERCLOCKED.OverclockBuff))
            {
                characterBody.RemoveBuff(HAND_OVERCLOCKED.OverclockBuff);
                /*characterBody.ClearTimedBuffs(HAND_OVERCLOCKED.OverclockCooldownBuff);
                for (int i = 1; i <= 7; i++)
                {
                    characterBody.AddTimedBuff(HAND_OVERCLOCKED.OverclockCooldownBuff, i);
                }*/
            }
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
            if (!cancelledOVC)
            {
                if (characterBody.skillLocator.utility.stock > 0)
                {
                    characterBody.skillLocator.utility.stock--;
                }
            }
        }

        private void OnDestroy()
        {
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
        }

        public void ExtendOverclock(float duration)
        {
            if (ovcActive)
            {
                ovcTimer += duration;
                if (ovcTimer > ovcTimerMax)
                {
                    ovcTimer = ovcTimerMax;
                }
            }
        }

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.teamComponent = base.GetComponent<TeamComponent>();

            characterBody.skillLocator.special.RemoveAllStocks();   //HAN-D DRONES starts at 0 stocks
            characterBody.skillLocator.special.enabled = false;

            ovcTimer = 0f;
            ovcActive = false;
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

            if (ovcActive)
            {
                if (ovcTimer > 0f)
                {
                    characterBody.skillLocator.utility.rechargeStopwatch = 0f;
                    ovcTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    EndOverclock();
                }
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

        private void Awake()
        {
            this.indicator = new Indicator(base.gameObject, Resources.Load<GameObject>("Prefabs/HuntressTrackingIndicator"));
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

        private int meleeHits = 0;
        public static int meleeHitsMax = 3;

        private float ovcTimer;
        private float ovcTimerMax = 4f;
        public bool ovcActive;

        private bool cancelledOVC = false;
    }

    public class HANDSwingAttack
    {
        public HANDHitResult Fire()
        {
            bool bossWasHit = false;
            HealthComponent myHC = this.attacker.GetComponent<CharacterBody>().healthComponent;
            Collider[] array = useSphere? Physics.OverlapSphere(position, radius, LayerIndex.entityPrecise.mask) : Physics.OverlapBox(position, extents, orientation, LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                Collider collider = array[i];
                HurtBox component = collider.GetComponent<HurtBox>();
                if (component)
                {
                    HealthComponent healthComponent = component.healthComponent;
                    if (healthComponent && healthComponent != myHC && (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, this.teamIndex)))
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
                if (hitPoint2.hurtBox.healthComponent.body && hitPoint2.hurtBox.healthComponent.body.isBoss)
                {
                    bossWasHit = true;
                }

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

                if (this.airbornVerticalForce != 0f || this.groundedVerticalForce != 0f || this.airbornHorizontalForceMult != 1f || this.landEnemyVerticalForce != 0f)
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
                        else
                        {
                            damageInfo.force += this.landEnemyVerticalForce * Vector3.up;
                            if (cb.characterMotor && !cb.characterMotor.isGrounded)
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
                                damageInfo.force *= groundedForceMult;
                            }
                        }
                    }
                }

                if (this.scaleForceMass)
                {
                    Rigidbody rb = hitPoint2.hurtBox.healthComponent.gameObject.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        damageInfo.force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), 6f);
                        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                        rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, 0f);

                    }
                    CharacterMotor cm = hitPoint2.hurtBox.healthComponent.gameObject.GetComponent<CharacterMotor>();
                    if (cm)
                    {
                        cm.velocity.x = 0f;
                        cm.velocity.z = 0f;
                        cm.rootMotion.x = 0f;
                        cm.rootMotion.z = 0f;
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
                    HANDNetworking.write.Write(damageInfo);
                    HANDNetworking.write.Write(hitPoint2.hurtBox.healthComponent != null);
                    HANDNetworking.write.FinishMessage();
                    ClientScene.readyConnection.SendWriter(HANDNetworking.write, QosChannelIndex.defaultReliable.intVal);
                }

                EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
                {
                    origin = hitPoint2.hitPosition
                }, false);
            }
            return new HANDHitResult() { hitCount = array2.Length, hitBoss = bossWasHit};
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
        public float groundedVerticalForce = 0f;
        public float groundedForceMult = 1f;
        public float landEnemyVerticalForce = 0f;
        public bool useSphere = false;
        public float radius = 8f;
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
