using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using HAND_OVERCLOCKED;
using HAND_OVERCLOCKED.Components;
using RoR2.Projectile;

namespace EntityStates.HANDOverclocked
{
    public class SlamScepter : BaseState
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
                    EffectManager.SpawnEffect(SlamScepter.impactEffectPrefab, new EffectData
                    {
                        origin = base.transform.position + i * directionFlat.normalized - 1.8f * Vector3.up,
                        scale = 0.5f
                    }, true);
                }
                if (base.isAuthority)
                {
                    bool isCrit = base.RollCrit();
                    Quaternion direction = Quaternion.LookRotation(directionFlat, Vector3.up);
                    int hitCount = new HAND_OVERCLOCKED.HANDSwingAttackSecondary
                    {
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                        baseDamage = this.damageStat * Mathf.Lerp(damageCoefficientMin, damageCoefficientMax, chargePercent),
                        position = base.transform.position + hitRange / 2f * directionFlat.normalized - 0.5f * Vector3.up,
                        extents = new Vector3(4.2f, 7f, hitRange + 1f),
                        orientation = direction,
                        procCoefficient = 1f,
                        crit = isCrit,
                        force = Vector3.zero,
                        airborneLaunchForce = Mathf.Lerp(airbornVerticalForceMin, airbornVerticalForceMax, chargePercent),
                        damageType = DamageType.Stun1s,
                        groundedLaunchForce = Mathf.Lerp(forceMagnitudeMin, forceMagnitudeMax, chargePercent),
                        hitEffectPrefab = SlamScepter.hitEffectPrefab,
                        maxForceScale = Mathf.Infinity,
                        squish = true
                    }.Fire();
                    if (base.characterMotor && !base.characterMotor.isGrounded)
                    {
                        base.SmallHop(base.characterMotor, shorthopVelocityFromHit);
                    }
                    if (hitCount > 0)
                    {
                        BeginHitPause();
                        OverclockController hc = base.gameObject.GetComponent<OverclockController>();
                        if (hc)
                        {
                            hc.MeleeHit(hitCount);
                            hc.ExtendOverclock(Mathf.Lerp(0.8f, 1.6f, chargePercent));
                        }

                        FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                        fireProjectileInfo.position = base.transform.position;
                        fireProjectileInfo.rotation = direction;
                        fireProjectileInfo.crit = isCrit;
                        fireProjectileInfo.damage = 1f * this.damageStat;
                        fireProjectileInfo.owner = base.gameObject;
                        fireProjectileInfo.projectilePrefab = zapConePrefab;
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                }
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();
            //Util.PlaySound("Play_HOC_Hammer", base.gameObject);
            //Util.PlaySound("Play_UI_podImpact", base.gameObject);
            Util.PlaySound("Play_parent_attack1_slam", base.gameObject);
            Util.PlaySound("Play_UI_podImpact", base.gameObject);
            this.duration = SlamScepter.baseDuration / this.attackSpeedStat;
            this.minDuration = SlamScepter.baseMinDuration / this.attackSpeedStat;
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
                    EffectManager.SimpleMuzzleFlash(SlamScepter.swingEffectPrefab, base.gameObject, "SwingCenter", false);
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

        public static float baseDuration = 0.6f;
        public static float baseMinDuration = 0.4f;
        public static float returnToIdlePercentage = 0.443662f;
        public static float damageCoefficientMin = 6f;
        public static float damageCoefficientMax = 18f;
        public static float forceMagnitudeMin = 2000f;
        public static float forceMagnitudeMax = 2000f;
        public static float airbornVerticalForceMin = -4800f;
        public static float airbornVerticalForceMax = -6400f;
        public static float minRange = 9f;
        public static float maxRange = 22f;

        public static GameObject zapConePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/LoaderZapCone");
        public static GameObject impactEffectPrefab;
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactToolbotDashLarge");
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

        public static float shorthopVelocityFromHit = 24f;

        public float chargePercent;
    }
}
