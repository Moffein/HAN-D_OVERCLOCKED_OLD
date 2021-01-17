using EntityStates.Commando.CommandoWeapon;
using EntityStates.Engi.EngiWeapon;
using HAND_OVERCLOCKED;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.Networking;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.HANDOverclocked
{
    public class FullSwing : BaseState
    {
        private void DealDamage()
        {
            if (!this.hasSwung)
            {
                Util.PlaySound("Play_loader_m1_swing", base.gameObject);
                if (base.isAuthority)
                {
                    this.hasSwung = true;
                    EffectManager.SimpleMuzzleFlash(FullSwing.swingEffectPrefab, base.gameObject, "SwingCenter", true);
                    Vector3 directionFlat = base.GetAimRay().direction;
                    directionFlat.y = 0;
                    directionFlat.Normalize();

                    HANDHitResult hitCount = new HANDSwingAttackPrimary
                    {
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                        baseDamage = this.damageStat * FullSwing.damageCoefficient,
                        position = base.transform.position + 2.5f * directionFlat.normalized,
                        extents = new Vector3(4.2f, 6f, 6f),
                        orientation = Quaternion.LookRotation(directionFlat, Vector3.up),
                        procCoefficient = 1f,
                        crit = RollCrit(),
                        force = FullSwing.forceMagnitude * directionFlat,
                        airborneHorizontalForceMult = FullSwing.airbornHorizontalForceMult,
                        flyingHorizontalForceMult = FullSwing.flyingHorizontalForceMult,
                        damageType = ((base.characterBody.HasBuff(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff) && (secondSwing || (firstSwing && Util.CheckRoll(30f)))) ? DamageType.Stun1s : DamageType.Generic),
                        maxForceScale = 20f,
                        bossGroundedForceMult = 0.5f
                    }.Fire();
                    if (hitCount.hitCount > 0)
                    {
                        Util.PlaySound("Play_MULT_shift_hit", base.gameObject);
                        BeginHitPause();
                        hc = base.gameObject.GetComponent<HANDController>();
                        if (hc)
                        {
                            hc.MeleeHit(hitCount);
                            hc.ExtendOverclock(1.1f);
                        }

                    }
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FullSwing.baseDuration / this.attackSpeedStat;
            this.modelAnimator = base.GetModelAnimator();
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    this.hammerChildTransform = component.FindChild("SwingCenter");
                }
            }
            if (this.modelAnimator)
            {
                int layerIndex = this.modelAnimator.GetLayerIndex("Gesture");
                if (this.modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("FullSwing3") || this.modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("FullSwing1"))
                {
                    base.PlayCrossfade("Gesture", "FullSwing2", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                    secondSwing = true;
                }
                else if (this.modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("FullSwing2"))
                {
                    base.PlayCrossfade("Gesture", "FullSwing3", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                }
                else
                {
                    base.PlayCrossfade("Gesture", "FullSwing1", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                    firstSwing = true;
                }
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
        }

        private void BeginHitPause()
        {
            this.enteredHitPause = true;
            this.storedVelocity = base.characterMotor.velocity;
            base.characterMotor.velocity = Vector3.zero;
            if (this.modelAnimator)
            {
                this.modelAnimator.speed = 0f;
            }
            this.hitPauseTimer = this.hitPauseDuration;
        }

        private void ExitHitPause()
        {
            this.hitPauseTimer = 0f;
            base.characterMotor.velocity = this.storedVelocity;
            this.storedVelocity = Vector3.zero;
            if (this.modelAnimator)
            {
                this.modelAnimator.speed = 1f;
            }
            if (!base.isGrounded)
            {
                base.SmallHop(base.characterMotor, FullSwing.shorthopVelocityFromHit);
            }
            this.exitedHitPause = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.modelAnimator && this.modelAnimator.GetFloat("Hammer.hitBoxActive") > 0.5f)
            {
                if (!this.hasSwung)
                {
                    this.DealDamage();
                }
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
            if (base.isAuthority && this.enteredHitPause && this.hitPauseTimer > 0f)
            {
                this.hitPauseTimer -= Time.fixedDeltaTime;
                base.characterMotor.velocity = Vector3.zero;
                if (this.hitPauseTimer <= 0f)
                {
                    this.ExitHitPause();
                }
            }
        }

        public override void OnExit()
        {
            if (!this.hasSwung && !base.inputBank.skill2.justPressed)
            {
                this.DealDamage();
                if (this.enteredHitPause)
                {
                    this.ExitHitPause();
                }
            }
            if (this.enteredHitPause && !this.exitedHitPause)
            {
                this.ExitHitPause();
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public static float baseDuration;
        public static float returnToIdlePercentage;
        public static float damageCoefficient;
        public static float forceMagnitude;
        public static float pullAngle;
        public static float pullForce;
        public static float pullDistance;
        public static float airbornVerticalForce;
        public static GameObject swingEffectPrefab;
        public static float airbornHorizontalForceMult;
        public static float flyingHorizontalForceMult;

        private Transform hammerChildTransform;
        private Animator modelAnimator;
        private float duration;
        private bool hasSwung;

        private Vector3 storedVelocity;
        private float hitPauseTimer = 0f;
        private float hitPauseDuration = 0.1f;
        private bool enteredHitPause = false;
        private bool exitedHitPause = false;

        private HANDController hc;

        public static float shorthopVelocityFromHit;

        private bool secondSwing = false;
        private bool firstSwing = false;
    }

    public class FireSeekingDrone : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            EffectManager.SpawnEffect(FireSeekingDrone.effectPrefab, new EffectData
            {
                origin = base.transform.position
            }, false);

            hasFired = false;
            Transform modelTransform = base.GetModelTransform();
            this.handController = base.GetComponent<HANDController>();
            Util.PlaySound("Play_drone_repair", base.gameObject);
            if (base.isAuthority && this.handController)
            {
                this.initialOrbTarget = this.handController.GetTrackingTarget();
                handController.CmdHeal();
            }
            this.duration = baseDuration / this.attackSpeedStat;
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(this.duration + 1f);
            }
            this.isCrit = base.RollCrit();
        }

        public override void OnExit()
        {
            if (!hasFired && base.isAuthority)
            {
                FireMissile(this.initialOrbTarget, base.inputBank.aimOrigin);
            }
            if (base.isAuthority && this.handController)
            {
                this.handController.CmdUpdateDrones(characterBody.skillLocator.special.stock);
            }
            base.OnExit();
        }

        private void FireMissile(HurtBox target, Vector3 position)
        {
            hasFired = true;
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.position = base.inputBank.aimOrigin;
            fireProjectileInfo.rotation = Quaternion.LookRotation(Vector3.up);
            fireProjectileInfo.crit = base.RollCrit();
            fireProjectileInfo.damage = this.damageStat * FireSeekingDrone.damageCoefficient;
            fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
            fireProjectileInfo.owner = base.gameObject;
            fireProjectileInfo.projectilePrefab = FireSeekingDrone.projectilePrefab;
            if (target)
            {
                fireProjectileInfo.target = target.gameObject;
            }
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!hasFired && base.isAuthority)
            {
                FireMissile(this.initialOrbTarget, base.inputBank.aimOrigin);
            }
            if (base.fixedAge > this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        private bool hasFired;


        public static float damageCoefficient;
        public static GameObject projectilePrefab;
        public static string muzzleString;
        public static GameObject muzzleflashEffectPrefab;
        public static GameObject effectPrefab;
        public static float baseDuration;
        public static float healPercent;

        private float duration;
        protected bool isCrit;
        private HurtBox initialOrbTarget = null;
        private HANDController handController;
    }

    public class Overclock : BaseState
    {
        public override void OnEnter()
        {
            HANDController hc = base.gameObject.GetComponent<HANDController>();
            if (hc)
            {
                LogCore.LogI("HIIII");
                if (hc.ovcActive && base.isAuthority)
                {
                    LogCore.LogI("ovc active, boost time");
                    if (base.characterMotor)
                    {
                        base.SmallHop(base.characterMotor, 24f);
                    }
                    hc.ManualEndOverclock();
                }
                else
                {
                    if (isAuthority)
                    {
                        LogCore.LogI("ok");
                        hc.BeginOverclock(4f);
                        if (base.isAuthority && characterBody.skillLocator.utility.stock < characterBody.skillLocator.utility.maxStock)
                        {
                            characterBody.skillLocator.utility.stock++;
                        }
                    }
                }
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > 0 && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
    }

    public class ChargeSlam2 : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.minDuration = ChargeSlam2.baseMinDuration / this.attackSpeedStat;
            this.modelAnimator = base.GetModelAnimator();
            if (this.modelAnimator)
            {
                base.PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", this.minDuration);
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(4f);
            }
            charge = 0f;
            chargePercent = 0f;
            chargeDuration = ChargeSlam2.baseChargeDuration / this.attackSpeedStat;
        }

        public override void OnExit()
        {
            Util.PlaySound("Play_MULT_m2_throw", base.gameObject);
            if (this.holdChargeVfxGameObject)
            {
                EntityState.Destroy(this.holdChargeVfxGameObject);
                this.holdChargeVfxGameObject = null;
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge > this.minDuration && charge < chargeDuration)
            {
                charge += Time.deltaTime * this.attackSpeedStat;
                /*if (!played50 && charge > chargeDuration/2f)
                {
                    played50 = true;
                    Util.PlaySound("Play_engi_M1_chargeStock", base.gameObject);
                    EffectManager.SpawnEffect(chargeEffectPrefab, new EffectData
                    {
                        origin = base.transform.position
                    }, true);
                }*/
                if (charge > chargeDuration)
                {
                    Util.PlaySound("Play_MULT_m2_aim", base.gameObject);
                    charge = chargeDuration;
                    EffectManager.SpawnEffect(chargeEffectPrefab, new EffectData
                    {
                        origin = base.transform.position
                    }, true);
                    /*if (!chargeEffect)
                    {
                        chargeEffect = true;
                        if (!this.holdChargeVfxGameObject)
                        {
                            ChildLocator component = base.GetModelTransform().GetComponent<ChildLocator>();
                            if (component)
                            {
                                this.holdChargeVfxGameObject = UnityEngine.Object.Instantiate<GameObject>(holdChargeVfxPrefab, component.FindChild("SwingCenter"));
                            }
                        }
                    }*/
                }
                chargePercent = charge / chargeDuration;
            }

            if (base.fixedAge >= this.minDuration && base.isAuthority && base.inputBank && !base.inputBank.skill2.down)
            {
                this.outer.SetNextState(new EntityStates.HANDOverclocked.Slam2() { chargePercent = chargePercent });
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public static float baseMinDuration;
        public static float baseChargeDuration;
        private float minDuration;
        private float chargeDuration;
        private float charge;
        private float chargePercent;
        private Animator modelAnimator;
        public static GameObject chargeEffectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");

        private bool played50 = false;
        public static GameObject holdChargeVfxPrefab = EntityStates.Toolbot.ChargeSpear.holdChargeVfxPrefab;
        private bool chargeEffect = false;
        private GameObject holdChargeVfxGameObject = null;
    }

    public class Slam2 : BaseState
    {
        private void DealDamage()
        {
            if (!this.hasSwung)
            {
                this.hasSwung = true;
                Vector3 directionFlat = base.GetAimRay().direction;
                directionFlat.y = 0;
                directionFlat.Normalize();

                float hitRange = Mathf.Lerp(minRange, maxRange, chargePercent);

                for (int i = 5; i <= Mathf.RoundToInt(hitRange) + 1; i += 2)
                {
                    EffectManager.SpawnEffect(Slam2.impactEffectPrefab, new EffectData
                    {
                        origin = base.transform.position + i * directionFlat.normalized - 1.8f * Vector3.up,
                        scale = 0.5f
                    }, true);
                }

                if (base.isAuthority)
                {
                    HANDHitResult hitCount = new HAND_OVERCLOCKED.HANDSwingAttackSecondary
                    {
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                        baseDamage = this.damageStat * Mathf.Lerp(damageCoefficientMin, damageCoefficientMax, chargePercent),
                        position = base.transform.position + hitRange / 2f * directionFlat.normalized - 0.5f * Vector3.up,
                        extents = new Vector3(4.2f, 7f, hitRange + 1f),
                        orientation = Quaternion.LookRotation(directionFlat, Vector3.up),
                        procCoefficient = 1f,
                        crit = RollCrit(),
                        force = Vector3.zero,
                        airborneLaunchForce = Mathf.Lerp(airbornVerticalForceMin, airbornVerticalForceMax, chargePercent),
                        damageType = DamageType.Stun1s,
                        groundedLaunchForce = Mathf.Lerp(forceMagnitudeMin, forceMagnitudeMax, chargePercent),
                        hitEffectPrefab = Slam2.hitEffectPrefab,
                        maxForceScale = 20f
                    }.Fire();
                    if (base.characterMotor && !base.characterMotor.isGrounded)
                    {
                        base.SmallHop(base.characterMotor, shorthopVelocityFromHit);
                    }
                    if (hitCount.hitCount > 0)
                    {
                        BeginHitPause();
                        hc = base.gameObject.GetComponent<HANDController>();
                        if (hc)
                        {
                            hc.MeleeHit(hitCount);
                            hc.ExtendOverclock(Mathf.Lerp(1.2f, 4f, chargePercent));
                        }
                    }
                }
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_parent_attack1_slam", base.gameObject);
            this.duration = Slam2.baseDuration / this.attackSpeedStat;
            this.minDuration = Slam2.baseMinDuration / this.attackSpeedStat;
            this.modelAnimator = base.GetModelAnimator();
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    this.hammerChildTransform = component.FindChild("SwingCenter");
                }
            }
            if (this.modelAnimator)
            {
                base.PlayAnimation("Gesture", "Slam", "Slam.playbackRate", this.duration);
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
        }

        public override void OnExit()
        {
            if (!this.hasSwung)
            {
                this.DealDamage();
                if (this.enteredHitPause)
                {
                    this.ExitHitPause();
                }
            }
            if (this.enteredHitPause && !this.exitedHitPause)
            {
                this.ExitHitPause();
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.modelAnimator && this.modelAnimator.GetFloat("Hammer.hitBoxActive") > 0.5f)
            {
                if (!this.hasSwung)
                {
                    this.DealDamage();
                    EffectManager.SimpleMuzzleFlash(Slam2.swingEffectPrefab, base.gameObject, "SwingCenter", false);
                }
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
            if (base.isAuthority && this.enteredHitPause && this.hitPauseTimer > 0f)
            {
                this.hitPauseTimer -= Time.fixedDeltaTime;
                base.characterMotor.velocity = Vector3.zero;
                if (this.hitPauseTimer <= 0f)
                {
                    this.ExitHitPause();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (hasSwung && base.fixedAge >= this.minDuration)
            {
                return InterruptPriority.Any;
            }
            return InterruptPriority.PrioritySkill;
        }

        private void BeginHitPause()
        {
            this.enteredHitPause = true;
            this.storedVelocity = base.characterMotor.velocity;
            base.characterMotor.velocity = Vector3.zero;
            if (this.modelAnimator)
            {
                this.modelAnimator.speed = 0f;
            }
            this.hitPauseTimer = this.hitPauseDuration;
        }

        private void ExitHitPause()
        {
            this.hitPauseTimer = 0f;
            base.characterMotor.velocity = this.storedVelocity;
            this.storedVelocity = Vector3.zero;
            if (this.modelAnimator)
            {
                this.modelAnimator.speed = 1f;
            }
            this.exitedHitPause = true;
        }

        public static float baseDuration;
        public static float baseMinDuration;
        public static float returnToIdlePercentage;
        public static float damageCoefficientMin;
        public static float damageCoefficientMax;
        public static float forceMagnitudeMin;
        public static float forceMagnitudeMax;
        public static float airbornVerticalForceMin;
        public static float airbornVerticalForceMax;
        public static float minRange;
        public static float maxRange;

        public static GameObject impactEffectPrefab;
        public static GameObject hitEffectPrefab;
        public static GameObject swingEffectPrefab;
        private Transform hammerChildTransform;
        private Animator modelAnimator;
        private float duration;
        private float minDuration;
        private bool hasSwung;

        private Vector3 storedVelocity;
        private float hitPauseTimer = 0f;
        private float hitPauseDuration = 0.1f;
        private bool enteredHitPause = false;
        private bool exitedHitPause = false;

        private HANDController hc;

        public static float shorthopVelocityFromHit;

        public float chargePercent;
    }
}