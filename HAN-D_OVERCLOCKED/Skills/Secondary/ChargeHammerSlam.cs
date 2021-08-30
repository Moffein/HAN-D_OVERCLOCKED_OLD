using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using HAND_OVERCLOCKED;

namespace EntityStates.HANDOverclocked
{
    public class ChargeSlam2 : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_HOC_StartHammer", base.gameObject);
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

            /*if (base.characterBody && base.characterBody.isSprinting)
            {
                base.characterBody.isSprinting = false;
            }*/

            if (base.fixedAge > this.minDuration && charge < chargeDuration)
            {
                charge += Time.deltaTime * this.attackSpeedStat;
                if (charge > chargeDuration)
                {
                    Util.PlaySound("Play_HOC_StartPunch", base.gameObject);
                    charge = chargeDuration;
                    EffectManager.SpawnEffect(chargeEffectPrefab, new EffectData
                    {
                        origin = base.transform.position
                    }, false);
                }
                chargePercent = charge / chargeDuration;
            }

            if (base.fixedAge >= this.minDuration)
            {
                if (base.isAuthority && base.inputBank && !base.inputBank.skill2.down)
                {
                    this.outer.SetNextState(new EntityStates.HANDOverclocked.Slam2() { chargePercent = chargePercent });
                    return;
                }
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

        private float shakeTimer = 0.15f;
        private float shakeStopwatch = 0f;

        public static GameObject holdChargeVfxPrefab = EntityStates.Toolbot.ChargeSpear.holdChargeVfxPrefab;
        private GameObject holdChargeVfxGameObject = null;
    }
}
