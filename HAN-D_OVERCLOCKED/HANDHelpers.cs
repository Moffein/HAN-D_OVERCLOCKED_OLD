using EntityStates.Engi.EngiWeapon;
using R2API.Networking;
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
    public class HANDController : NetworkBehaviour
    {
        [Client]
        public void MeleeHit(HANDHitResult result)
        {
            if (this.hasAuthority && result.hitBoss)
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

        [Client]
        public void BeginOverclock(float duration)
        {
            if (this.hasAuthority && !characterBody.HasBuff(HAND_OVERCLOCKED.OverclockBuff))
            {
                ovcTimer = duration;
                NetworkingHelpers.ApplyBuff(characterBody, HAND_OVERCLOCKED.OverclockBuff, 1, ovcTimer);
                ovcActive = true;
                startOverclockCooldown = characterBody.skillLocator.special.rechargeStopwatch;
            }
        }

        [Client]
        public void EndOverclock()
        {
            if (this.hasAuthority)
            {
                ovcTimer = 0f;
                ovcActive = false;
                Util.PlaySound("Play_MULT_shift_end", base.gameObject);//disable this later
                CmdEndOverclock();
            }
        }

        [Command]
        private void CmdEndOverclock()
        {
            if (characterBody.HasBuff(HAND_OVERCLOCKED.OverclockBuff))
            {
                characterBody.ClearTimedBuffs(HAND_OVERCLOCKED.OverclockBuff);
            }
            //RpcEndOverclockSound();
        }

        [ClientRpc]
        private void RpcEndOverclockSound()
        {
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
        }

        private void OnDestroy()
        {
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
        }

        [Client]
        public void ExtendOverclock(float duration)
        {
            if (this.hasAuthority && ovcActive)
            {
                ovcTimer += duration;
                if (ovcTimer > ovcTimerMax)
                {
                    ovcTimer = ovcTimerMax;
                }
                if (ovcTimer > 0)
                {
                    NetworkingHelpers.ApplyBuff(characterBody, HAND_OVERCLOCKED.OverclockBuff, 1, ovcTimer);
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

            TickOverclock();
        }

        [Client]
        private void TickOverclock()
        {
            if (this.hasAuthority)
            {
                if (ovcActive)
                {
                    if (ovcTimer > 0f)
                    {
                        characterBody.skillLocator.utility.rechargeStopwatch = startOverclockCooldown;
                        if (ovcTimer > 0f)
                        {
                            ovcTimer -= Time.fixedDeltaTime;
                        }
                    }
                    else
                    {
                        if (characterBody.skillLocator.utility.stock > 0)
                        {
                            characterBody.skillLocator.utility.stock--;
                        }
                        EndOverclock();
                    }
                }
            }
        }

        [ClientRpc]
        public void RpcAddSpecialStock()
        {
            if (this.hasAuthority && characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock )
            {
                characterBody.skillLocator.special.AddOneStock();
                CmdUpdateDrones(characterBody.skillLocator.special.stock);
            }
        }

        [Command]
        public void CmdUpdateDrones(int stock)
        {
            droneCount = stock;
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
            this.indicator = new Indicator(base.gameObject, Resources.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
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
        public bool ovcActive;

        private float ovcTimerMax = 4f;

        private float startOverclockCooldown;

        [SyncVar]
        public int droneCount = 0;
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
                        damageInfo.force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
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

                NetworkingHelpers.DealDamage(damageInfo, hitPoint2.hurtBox, true, true, true);
                /*if (NetworkServer.active)
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
                }*/

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
        public float maxForceScale = 6f;
        private static readonly Dictionary<HealthComponent, HANDSwingAttack.HitPoint> bestHitPoints = new Dictionary<HealthComponent, HANDSwingAttack.HitPoint>();
    }
}
