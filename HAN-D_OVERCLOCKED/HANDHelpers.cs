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
    public struct HANDHitResult
    {
        public int hitCount;
        public bool hitBoss;
    }

    public class HANDDronePersistComponent : NetworkBehaviour
    {
        public int droneCount = 0;
    }

    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    public class HANDController : NetworkBehaviour
    {
        [Command]
        public void CmdHeal()
        {
            characterBody.healthComponent.HealFraction(EntityStates.HANDOverclocked.FireSeekingDrone.healPercent, new ProcChainMask());
        }

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
                        CmdUpdateDrones(characterBody.skillLocator.special.stock);
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
                NetworkingHelpers.ApplyBuff(characterBody, HAND_OVERCLOCKED.OverclockBuff, 1, -1);
                ovcActive = true;
                startOverclockCooldown = characterBody.skillLocator.special.rechargeStopwatch;
                ovcDuration = 0f;
                CmdStartOverclock();
            }
        }

        [Client]
        public void EndOverclock()
        {
            if (this.hasAuthority)
            {
                ovcTimer = 0f;
                ovcActive = false;
                CmdEndOverclock();
            }
        }

        [Client]
        public void ManualEndOverclock()
        {
            if (this.hasAuthority)
            {
                ovcTimer = 0f;
                ovcActive = false;
                CmdManualEndOverclock(ovcDuration/ovcDurationMax);
            }
        }

        [Command]
        private void CmdEndOverclock()
        {
            if (characterBody.HasBuff(HAND_OVERCLOCKED.OverclockBuff))
            {
                characterBody.RemoveBuff(HAND_OVERCLOCKED.OverclockBuff);
            }
            RpcEndOverclockSound();
        }

        [Command]
        private void CmdManualEndOverclock(float f)
        {
            if (characterBody.HasBuff(HAND_OVERCLOCKED.OverclockBuff))
            {
                characterBody.ClearTimedBuffs(HAND_OVERCLOCKED.OverclockBuff);
            }

            EffectManager.SpawnEffect(ovcCancelEffectPrefab, new EffectData
            {
                origin = base.transform.position,
                scale = 10f
            }, true);
            new BlastAttack
            {
                attacker = base.gameObject,
                inflictor = base.gameObject,
                teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                baseDamage = characterBody.damage * Mathf.Lerp(ovcCancelMinDamageCoefficient,ovcCancelMaxDamageCoefficient,f),
                baseForce = 0f,
                position = base.transform.position,
                radius = ovcCancelRadius,
                falloffModel = BlastAttack.FalloffModel.None,
                damageType = (DamageType.Stun1s),
                crit = characterBody.RollCrit(),
                attackerFiltering = AttackerFiltering.NeverHit
            }.Fire();

            RpcEndOverclockSound();
        }

        [ClientRpc]
        private void RpcEndOverclockSound()
        {
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
        }

        [Command]
        private void CmdStartOverclock()
        {
            RpcStartOverclockSound();
        }

        [ClientRpc]
        private void RpcStartOverclockSound()
        {
            Util.PlaySound("Play_MULT_shift_start", base.gameObject);
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

            AddDronePersist();
        }

        [Client]
        private void AddDronePersist()
        {
            dronePersist = characterBody.master.gameObject.GetComponent<HANDDronePersistComponent>();
            if (!dronePersist)
            {
                dronePersist = characterBody.master.gameObject.AddComponent<HANDDronePersistComponent>();
            }
            else
            {
                characterBody.skillLocator.special.stock = dronePersist.droneCount;
                CmdUpdateDrones(dronePersist.droneCount);
            }
            hasDronePersist = true;
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
            if (hasDronePersist)
            {
                dronePersist.droneCount = characterBody.skillLocator.special.stock;
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
                    if (ovcDuration < ovcDurationMax)
                    {
                        ovcDuration += Time.fixedDeltaTime;
                        if (ovcDuration > ovcDurationMax)
                        {
                            ovcDuration = ovcDurationMax;
                        }
                    }

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
            RpcUpdateDrones(stock);
        }

        [ClientRpc]
        public void RpcUpdateDrones(int stock)
        {
            if (!this.hasAuthority)
            {
                droneCount = stock;
                characterBody.skillLocator.special.stock = stock;
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
        private HANDDronePersistComponent dronePersist;

        private int meleeHits = 0;
        public static int meleeHitsMax = 3;

        private float ovcTimer;
        public bool ovcActive;

        private float ovcTimerMax = 4f;

        private float startOverclockCooldown;

        private bool hasDronePersist = false;

        [SyncVar]
        public int droneCount = 0;

        public float ovcDuration;
        public static float ovcDurationMax = 6f; //time it takes to build up max ovc cancel damage

        public static float ovcCancelMinDamageCoefficient = 2f;
        public static float ovcCancelMaxDamageCoefficient = 6f;
        public static float ovcCancelRadius = 10f;
        public static GameObject ovcCancelEffectPrefab = EntityStates.Commando.CommandoWeapon.CastSmokescreenNoDelay.smokescreenEffectPrefab;
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

                damageInfo.force = ModifyForce(hitPoint2.hurtBox.healthComponent.gameObject, damageInfo.force);

                NetworkingHelpers.DealDamage(damageInfo, hitPoint2.hurtBox, true, true, true);

                EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
                {
                    origin = hitPoint2.hitPosition
                }, false);
            }
            return new HANDHitResult() { hitCount = array2.Length, hitBoss = bossWasHit};
        }

        public virtual Vector3 ModifyForce(GameObject go, Vector3 force)
        {
            return force;
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
        
        public DamageType damageType;
        public DamageColorIndex damageColorIndex;
        public ProcChainMask procChainMask;
        public float procCoefficient = 1f;

        public bool useSphere = false;
        public float radius = 8f;


        private static readonly Dictionary<HealthComponent, HANDSwingAttack.HitPoint> bestHitPoints = new Dictionary<HealthComponent, HANDSwingAttack.HitPoint>();
    }

    public class HANDSwingAttackPrimary : HANDSwingAttack
    {
        public override Vector3 ModifyForce(GameObject go, Vector3 force)
        {
            if (go)
            {
                CharacterBody cb = go.GetComponent<CharacterBody>();   
                if (cb)
                {
                    if(cb.isFlying)
                    {
                        force.x *= this.flyingHorizontalForceMult;
                        force.z *= this.flyingHorizontalForceMult;
                    }
                    else if (cb.characterMotor)
                    {
                        if (!cb.characterMotor.isGrounded)    //Multiply launched enemy force
                        {
                            force.x *= this.airborneHorizontalForceMult;
                            force.z *= this.airborneHorizontalForceMult;
                        }
                        else if (cb.isChampion) //deal less knockback against bosses if they're on the ground
                        {
                            force.x *= bossGroundedForceMult;
                            force.z *= bossGroundedForceMult;
                        }
                    }
                }

                //Scale force to match mass and reset current horizontal velocity to prevent launching at high attack speeds
                Rigidbody rb = cb.rigidbody;
                if (rb)
                {
                    //Debug.Log("Mass: " + rb.mass);
                    force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);//NEEDS TO BE FIXED IN MULTIPLAYER
                    rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, 0f);//NEEDS TO BE FIXED IN MULTIPLAYER

                }
                CharacterMotor cm = cb.characterMotor;
                if (cm)//NEEDS TO BE FIXED IN MULTIPLAYER
                {
                    cm.velocity.x = 0f;
                    cm.velocity.z = 0f;
                    cm.rootMotion.x = 0f;
                    cm.rootMotion.z = 0f;
                }
            }
            return force;
        }
        public float maxForceScale = 6f;
        public float airborneHorizontalForceMult = 1f;
        public float flyingHorizontalForceMult = 1f;
        public float bossGroundedForceMult = 1f;
    }

    public class HANDSwingAttackSecondary : HANDSwingAttack
    {
        public override Vector3 ModifyForce(GameObject go, Vector3 force)
        {
            if (go)
            {
                //Use separate knockback values when dealing with airborne/grounded targets.
                CharacterBody cb = go.GetComponent<CharacterBody>();
                if (cb)
                {
                    if (cb.characterMotor && cb.characterMotor.isGrounded)
                    {
                        force += groundedLaunchForce*Vector3.up;
                    }
                    else
                    {
                        force += airborneLaunchForce * Vector3.up;
                        if (cb.characterMotor && cb.characterMotor.velocity.y > 0f)
                        {
                            cb.characterMotor.velocity.y = 0f;  //NEEDS TO BE FIXED IN MULTIPLAYER
                        }
                    }
                }

                //Scale force to match mass
                Rigidbody rb = cb.rigidbody;
                if (rb)
                {
                    //Debug.Log("Mass: " + rb.mass);
                    force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
                    if (rb.velocity.y > 0f)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);    //NEEDS TO BE FIXED IN MULTIPLAYER
                    }
                    if (rb.angularVelocity.y > 0f)
                    {
                        rb.angularVelocity = new Vector3(rb.angularVelocity.x, 0f, rb.angularVelocity.z);   //NEEDS TO BE FIXED IN MULTIPLAYER
                    }
                }
            }
            return force;
        }
        public float maxForceScale = 6f;
        public float groundedLaunchForce = 0f;
        public float airborneLaunchForce = 0f;
    }
}