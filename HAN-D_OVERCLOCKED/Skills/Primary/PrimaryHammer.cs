using HAND_OVERCLOCKED;
using System;
using UnityEngine;
using RoR2;
using System.Collections.Generic;
using System.Text;
using HAND_OVERCLOCKED.Components;

namespace EntityStates.HANDOverclocked
{
    public class FullSwing : BaseState
    {
        private void DealDamage()
        {
            if (!this.hasSwung)
            {
                Util.PlaySound("Play_HOC_Punch", base.gameObject);
                this.hasSwung = true;

                if (base.isAuthority)
                {
                    EffectManager.SimpleMuzzleFlash(FullSwing.swingEffectPrefab, base.gameObject, "SwingCenter", true);
                    Vector3 directionFlat = base.GetAimRay().direction;
                    directionFlat.y = 0;
                    directionFlat.Normalize();

                    int hitCount = new HANDSwingAttackPrimary
                    {
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                        baseDamage = this.damageStat * FullSwing.damageCoefficient,
                        position = base.transform.position + 3f * directionFlat.normalized,
                        extents = new Vector3(4.2f, 6f, 7f),
                        orientation = Quaternion.LookRotation(directionFlat, Vector3.up),
                        procCoefficient = 1f,
                        crit = RollCrit(),
                        force = FullSwing.forceMagnitude * directionFlat,
                        airborneHorizontalForceMult = FullSwing.airbornHorizontalForceMult,
                        flyingHorizontalForceMult = FullSwing.flyingHorizontalForceMult,
                        damageType = ((!HAND_OVERCLOCKED.HAND_OVERCLOCKED.arenaActive
                        && (base.characterBody.HasBuff(HAND_OVERCLOCKED.HANDContent.OverclockBuff)
                        && (secondSwing || (firstSwing && Util.CheckRoll(30f))))) ? DamageType.Stun1s : DamageType.Generic),
                        maxForceScale = Mathf.Infinity,
                        bossGroundedForceMult = 0.5f,
                        bossAirborneForceMult = 1f,
                        stopMomentum = true
                    }.Fire();
                    if (hitCount > 0)
                    {
                        Util.PlaySound("Play_MULT_shift_hit", base.gameObject);
                        BeginHitPause();
                        OverclockController hc = base.gameObject.GetComponent<OverclockController>();
                        if (hc)
                        {
                            hc.MeleeHit(hitCount);
                            hc.ExtendOverclock(0.8f);
                        }
                    }
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_HOC_StartPunch", base.gameObject);
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
                    swingAnim = 2;
                }
                else if (this.modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("FullSwing2"))
                {
                    base.PlayCrossfade("Gesture", "FullSwing3", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                    swingAnim = 3;  //travels left
                }
                else
                {
                    base.PlayCrossfade("Gesture", "FullSwing1", "FullSwing.playbackRate", this.duration / (1f - FullSwing.returnToIdlePercentage), 0.2f);
                    firstSwing = true;
                    swingAnim = 1;
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

        public static float shorthopVelocityFromHit;

        private bool secondSwing = false;
        private bool firstSwing = false;

        private int swingAnim = 0;
    }
}
