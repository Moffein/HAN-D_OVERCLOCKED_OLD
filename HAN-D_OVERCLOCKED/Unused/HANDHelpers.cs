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

    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(CharacterBody))]
    public class HANDOverclockController : NetworkBehaviour, IOnDamageDealtServerReceiver
    {
        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        public void OnDamageDealtServer(DamageReport damageReport)
        {
            throw new NotImplementedException();
        }
    }

    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    public class HANDDroneController : NetworkBehaviour, IOnKilledOtherServerReceiver
    {
        public float maxTrackingDistance = 100f;
        public float maxTrackingAngle = 360f;
        public float trackerUpdateFrequency = 10f;

        private float trackerUpdateStopwatch;
        private HurtBox trackingTarget;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private TeamComponent team;
        private InputBankTest input;
        private CharacterBody body;
        private SkillLocator locator;

        [SyncVar]
        private int droneCount;

        private bool hasDronePersist;
        private HANDDronePersistComponent dronePersist;

        private Indicator indicator;



        private void Awake()
        {
            //fetch components in awake, it's the only safe way
            this.indicator = new Indicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
            this.body = base.GetComponent<CharacterBody>();
            this.input = base.GetComponent<InputBankTest>();
            this.team = base.GetComponent<TeamComponent>();
            this.locator = base.GetComponent<SkillLocator>();
        }

        private void Start() {
            //do our component stuff here
            body.skillLocator.special.RemoveAllStocks();   //HAN-D DRONES starts at 0 stocks
            body.skillLocator.special.enabled = false;

            AddDronePersist();
        }

        [Client]
        private void AddDronePersist()
        {
            if (body.master)
            {
                dronePersist = body.master.gameObject.GetComponent<HANDDronePersistComponent>();
                if (!dronePersist)
                {
                    dronePersist = body.master.gameObject.AddComponent<HANDDronePersistComponent>();
                }
                else
                {
                    body.skillLocator.special.stock = dronePersist.droneCount;
                    CmdUpdateDrones(dronePersist.droneCount);
                }
                hasDronePersist = true;
            }
        }

        [Command]
        public void CmdUpdateDrones(int stock)
        {
            //IF IT IS SYNCED
            //JUST UPDATE IT ON THE FUCKING CLIENT DUUUUUUUUUUUUUUDE
            //I DON'T GET IT
            //LITERALLY
            //DOES THE SAME THING AS RpcUpdateDrones
            //SO WHY DO YOU UPDATE IT ON SERVER
            //THEN UPDATE IT ON CLIENT
            //DO YOU NOT CARE??
            droneCount = stock;
            RpcUpdateDrones(stock);
        }

        [ClientRpc]
        public void RpcUpdateDrones(int stock)
        {
            droneCount = stock;
            locator.special.stock = stock;
        }

        public void FixedUpdate() {
            this.trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
            {
                this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
                HurtBox hurtBox = this.trackingTarget;
                Ray aimRay = new Ray(this.input.aimOrigin, this.input.aimDirection);
                this.SearchForTarget(aimRay);
                this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
            }
        }

        private void SearchForTarget(Ray aimRay)
        {
            this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.team.teamIndex);
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



        public void OnKilledOtherServer(DamageReport damageReport)
        {
            //throw new NotImplementedException();
        }
    }

    [RequireComponent(typeof(TeamComponent))]
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    public class HANDController : NetworkBehaviour, IOnDamageInflictedServerReceiver
    {
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
        #region shit
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
                LogCore.LogI("Overclock activated");
                ovcTimer = duration;
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
                LogCore.LogI("ENDOVERCLOCK");
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
                LogCore.LogI("ManualEndOverclock");
                ovcTimer = 0f;
                ovcActive = false;
                CmdManualEndOverclock(ovcDuration / ovcDurationMax);
            }
        }

        [Command]
        private void CmdEndOverclock()
        {
            LogCore.LogI("CmdEndOverclock");


            characterBody.RemoveBuff(HAND_OVERCLOCKED.OverclockBuff);

            ovcActive = false;
            RpcEndOverclockSound();
        }

        [Command]
        private void CmdManualEndOverclock(float f)
        {
            LogCore.LogI("CmdManualEndOverclock");

            characterBody.RemoveBuff(HAND_OVERCLOCKED.OverclockBuff);
            for (int i = 0; i < characterBody.GetBuffCount(HAND_OVERCLOCKED.steamBuff); i++)
            {
                characterBody.RemoveBuff(HAND_OVERCLOCKED.steamBuff);
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
                baseDamage = characterBody.damage * Mathf.Lerp(ovcCancelMinDamageCoefficient, ovcCancelMaxDamageCoefficient, f),
                baseForce = 0f,
                position = base.transform.position,
                radius = ovcCancelRadius,
                falloffModel = BlastAttack.FalloffModel.None,
                damageType = (DamageType.Stun1s),
                crit = characterBody.RollCrit(),
                attackerFiltering = AttackerFiltering.NeverHit
            }.Fire();
            ovcActive = false;

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
            characterBody.AddBuff(HAND_OVERCLOCKED.OverclockBuff);
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
        #endregion

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
            if (this.hasAuthority && characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
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
            this.indicator = new Indicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
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

        public void OnDamageInflictedServer(DamageReport damageReport)
        {
            if (ovcActive) {
                characterBody.AddBuff(HAND_OVERCLOCKED.steamBuff);//
            }
        }

    }
}