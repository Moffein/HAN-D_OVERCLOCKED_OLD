using EntityStates.Engi.EngiWeapon;
using HAND_OVERCLOCKED;
using RoR2;
using RoR2.Networking;
using RoR2.Orbs;
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
            if (!this.hasSwung && base.isAuthority)
            {
                Util.PlaySound("Play_loader_m1_swing", base.gameObject);

                if (base.isAuthority)
                {
                    this.hasSwung = true;
                    EffectManager.SimpleMuzzleFlash(FullSwing.swingEffectPrefab, base.gameObject, "SwingCenter", true);
                    Vector3 directionFlat = base.GetAimRay().direction;
                    directionFlat.y = 0;
                    directionFlat.Normalize();

                    int hitCount = new HANDSwingAttack
                    {
                        scaleForceMass = true,
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
                        airbornVerticalForce = FullSwing.airbornVerticalForce,
                        airbornHorizontalForceMult = FullSwing.airbornHorizontalForceMult,
                        flyingHorizontalForceMult = FullSwing.flyingHorizontalForceMult,
                        damageType = DamageType.Generic
                    }.Fire();
                    if (hitCount > 0)
                    {
                        Util.PlaySound("Play_MULT_shift_hit", base.gameObject);
                        BeginHitPause();
                        if (NetworkServer.active)
                        {
                            int ovcCount = base.characterBody.GetBuffCount(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff);
                            base.characterBody.ClearTimedBuffs(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff);
                            if (ovcCount < HAND_OVERCLOCKED.HAND_OVERCLOCKED.maxOverclock)
                            {
                                ovcCount++;
                            }
                            for (int i = 0; i < ovcCount; i++)
                            {
                                base.characterBody.AddTimedBuff(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff, HAND_OVERCLOCKED.HAND_OVERCLOCKED.overclockBaseDecay + i * HAND_OVERCLOCKED.HAND_OVERCLOCKED.overclockDecay);
                            }
                        }
                        base.gameObject.GetComponent<HANDController>().MeleeHit(hitCount);
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
                }
                else if (this.modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("FullSwing2"))
                {
                    base.PlayCrossfade("Gesture", "FullSwing3", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                }
                else
                {
                    base.PlayCrossfade("Gesture", "FullSwing1", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                }
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
            //Pull disabled because it was splatting Greater Wisps and was generally unworkable
            //PullEnemies(base.transform.position, base.GetAimRay().direction, FullSwing.pullAngle, FullSwing.pullDistance, FullSwing.pullForce, base.GetTeam());
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
            if (!base.isGrounded)
            {
                this.storedVelocity.y = Mathf.Max(this.storedVelocity.y, FullSwing.shorthopVelocityFromHit);
            }
            base.characterMotor.velocity = this.storedVelocity;
            this.storedVelocity = Vector3.zero;
            if (this.modelAnimator)
            {
                this.modelAnimator.speed = 1f;
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

        /*private void PullEnemies(Vector3 position, Vector3 direction, float coneAngle, float maxDistance, float force, TeamIndex excludedTeam)
        {
            foreach (Collider collider in Physics.OverlapSphere(position, maxDistance))
            {
                float scaledForce = force * Mathf.Log10((position - collider.transform.position).magnitude * (100f / FullSwing.pullDistance)) / 2f;
                Vector3 normalized = (position - collider.transform.position).normalized;
                if (Vector3.Dot(-normalized, direction) >= Mathf.Cos(coneAngle * 0.5f * 0.0174532924f))
                {
                    TeamComponent component = collider.GetComponent<TeamComponent>();
                    if (component)
                    {
                        TeamIndex teamIndex = component.teamIndex;
                        if (teamIndex != excludedTeam)
                        {
                            CharacterMotor component2 = collider.GetComponent<CharacterMotor>();
                            Rigidbody component3 = collider.GetComponent<Rigidbody>();
                            float mult = Mathf.Min(component3.mass / 50f, 5f);

                            if (component2)
                            {
                                if (component2.isGrounded)
                                {
                                    collider.transform.position += Vector3.up;
                                }
                                component2.disableAirControlUntilCollision = true;
                                component2.ApplyForce(mult * normalized * scaledForce, false, false);
                            }

                            if (component3)
                            {
                                component3.AddForce(mult * normalized * scaledForce, ForceMode.Impulse);
                            }
                        }
                    }
                }
            }
        }*/

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

        public static float shorthopVelocityFromHit;
    }

    public class BlastOff : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound("Play_MULT_m2_main_explode", base.gameObject);

            if (base.isAuthority)
            {
                EffectManager.SpawnEffect(BlastOff.effectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = BlastOff.radius
                }, true);
                new BlastAttack
                {
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                    baseDamage = this.damageStat * BlastOff.damageCoefficient,
                    baseForce = BlastOff.forceMagnitude,
                    position = base.transform.position,
                    radius = BlastOff.radius,
                    procCoefficient = 1f,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageType = DamageType.Stun1s,
                    crit = RollCrit(),
                    attackerFiltering = AttackerFiltering.NeverHit
                }.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterMotor && base.characterDirection)
            {
                if (base.characterMotor.isGrounded)
                {
                    base.characterMotor.rootMotion += Vector3.up;
                }
                base.characterMotor.disableAirControlUntilCollision = false;
                base.characterMotor.velocity.y = BlastOff.jumpForce;
            }

            if (base.fixedAge >= BlastOff.baseDuration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public static GameObject effectPrefab;
        public static float airbornVerticalForce;
        public static float jumpForce;
        public static float forceMagnitude;
        public static float damageCoefficient;
        public static float radius;
        public static float baseDuration;
    }

    public class ChargeSlam : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_MULT_m2_aim", base.gameObject);
            this.duration = ChargeSlam.baseDuration / this.attackSpeedStat;
            this.modelAnimator = base.GetModelAnimator();
            if (this.modelAnimator)
            {
                base.PlayAnimation("Gesture", "ChargeSlam", "ChargeSlam.playbackRate", this.duration);
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(4f);
            }
        }

        public override void OnExit()
        {
            Util.PlaySound("Play_MULT_m2_throw", base.gameObject);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority && !base.inputBank.skill2.down)
            {
                this.outer.SetNextState(new EntityStates.HANDOverclocked.Slam());
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public static float baseDuration;
        private float duration;
        private Animator modelAnimator;
    }

    public class Slam : BaseState
    {
        private void DealDamage()
        {
            if (!this.hasSwung)
            {
                this.hasSwung = true;
                Vector3 directionFlat = base.GetAimRay().direction;
                directionFlat.y = 0;
                directionFlat.Normalize();

                for (int i = 5; i <= 23; i += 2)
                {
                    EffectManager.SpawnEffect(Slam.impactEffectPrefab, new EffectData
                    {
                        origin = base.transform.position + i * directionFlat.normalized - 1.8f * Vector3.up,
                        scale = 0.5f
                    }, true);
                }

                int hitCount = new HAND_OVERCLOCKED.HANDSwingAttack
                {
                    scaleForceMass = true,
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                    baseDamage = this.damageStat * Slam.damageCoefficient,
                    position = base.transform.position + 11f * directionFlat.normalized,
                    extents = new Vector3(4.2f, 6f, 23f),
                    orientation = Quaternion.LookRotation(directionFlat, Vector3.up),
                    procCoefficient = 1f,
                    crit = RollCrit(),
                    force = Vector3.zero,
                    airbornVerticalForce = Slam.airbornVerticalForce,
                    damageType = DamageType.Stun1s,
                    overwriteVerticalVelocity = true,
                    groundedVerticalForce = Slam.forceMagnitude,
                    hitEffectPrefab = Slam.hitEffectPrefab
                }.Fire();
                if (hitCount > 0)
                {
                    BeginHitPause();
                    if (NetworkServer.active)
                    {
                        int ovcCount = base.characterBody.GetBuffCount(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff);
                        base.characterBody.ClearTimedBuffs(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff);
                        if (ovcCount < HAND_OVERCLOCKED.HAND_OVERCLOCKED.maxOverclock)
                        {
                            ovcCount++;
                        }
                        for (int i = 0; i < ovcCount; i++)
                        {
                            base.characterBody.AddTimedBuff(HAND_OVERCLOCKED.HAND_OVERCLOCKED.OverclockBuff, HAND_OVERCLOCKED.HAND_OVERCLOCKED.overclockBaseDecay + i * HAND_OVERCLOCKED.HAND_OVERCLOCKED.overclockDecay);
                        }
                    }
                    base.gameObject.GetComponent<HANDController>().MeleeHit(hitCount);
                }
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_parent_attack1_slam", base.gameObject);
            this.duration = Slam.baseDuration / this.attackSpeedStat;
            this.minDuration = Slam.baseMinDuration / this.attackSpeedStat;
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
                    EffectManager.SimpleMuzzleFlash(Slam.swingEffectPrefab, base.gameObject, "SwingCenter", false);
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
            if (!base.isGrounded)
            {
                this.storedVelocity.y = Mathf.Max(this.storedVelocity.y, Slam.shorthopVelocityFromHit);
            }
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
        public static float damageCoefficient;
        public static float forceMagnitude;
        public static float airbornVerticalForce;

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

        public static float shorthopVelocityFromHit;
    }

    public class Overheat : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(Overheat.soundString, base.gameObject);
            EffectManager.SpawnEffect(Overheat.effectPrefab, new EffectData
            {
                origin = base.transform.position,
                scale = Overheat.radius
            }, false);
            if (NetworkServer.active)
            {
                base.healthComponent.HealFraction(Overheat.healPercent, new ProcChainMask());
                new BlastAttack()
                {
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                    baseDamage = this.damageStat * Overheat.damageCoefficient,
                    baseForce = 0f,
                    position = base.transform.position,
                    radius = Overheat.radius,
                    falloffModel = BlastAttack.FalloffModel.None,
                    damageType = DamageType.IgniteOnHit,
                    attackerFiltering = AttackerFiltering.NeverHit
                }.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= Overheat.baseDuration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public static float damageCoefficient;
        public static float radius;
        public static float healPercent;
        public static GameObject effectPrefab;
        public static float baseDuration;
        public static string soundString;
    }

    public class FireSeekingDrone : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            EffectManager.SpawnEffect(FireSeekingDrone.effectPrefab, new EffectData
            {
                origin = base.transform.position
            }, true);
            base.healthComponent.HealFraction(FireSeekingDrone.healPercent, new ProcChainMask());
            hasFired = false;
            Transform modelTransform = base.GetModelTransform();
            this.handController = base.GetComponent<HANDController>();
            Util.PlaySound("Play_drone_repair", base.gameObject);
            if (this.handController && base.isAuthority)
            {
                this.initialOrbTarget = this.handController.GetTrackingTarget();
                Debug.Log(this.handController.GetTrackingTarget());
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
            if (!hasFired)
            {
                this.FireOrbDrone();
            }
            base.OnExit();
        }

        protected virtual GenericDamageOrb CreateDroneOrb()
        {
            return new HandDroneOrb();
        }

        private void FireOrbDrone()
        {
            hasFired = true;
            if (!NetworkServer.active || initialOrbTarget == null)
            {
                return;
            }
            GenericDamageOrb genericDamageOrb = this.CreateDroneOrb();
            genericDamageOrb.damageValue = base.characterBody.damage * orbDamageCoefficient;
            genericDamageOrb.isCrit = this.isCrit;
            genericDamageOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
            genericDamageOrb.attacker = base.gameObject;
            genericDamageOrb.procCoefficient = orbProcCoefficient;
            HurtBox hurtBox = this.initialOrbTarget;
            if (hurtBox)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, true);
                genericDamageOrb.origin = base.transform.position;
                genericDamageOrb.target = hurtBox;
                OrbManager.instance.AddOrb(genericDamageOrb);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!hasFired)
            {
                this.FireOrbDrone();
            }
            if (base.fixedAge > this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        private bool hasFired;

        public static float orbDamageCoefficient;
        public static float orbProcCoefficient;
        public static string muzzleString;
        public static GameObject muzzleflashEffectPrefab;
        public static GameObject effectPrefab;
        public static float baseDuration;
        public static float healPercent;

        private float duration;
        protected bool isCrit;
        private HurtBox initialOrbTarget;
        private HANDController handController;
    }
}
