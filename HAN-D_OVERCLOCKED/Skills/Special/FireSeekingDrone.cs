using System;
using System.Collections.Generic;
using System.Text;
using HAND_OVERCLOCKED;
using HAND_OVERCLOCKED.Components;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.HANDOverclocked
{
    public class FireSeekingDrone : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            hasFired = false;
            Transform modelTransform = base.GetModelTransform();
            targetingController = base.GetComponent<TargetingController>();
            Util.PlaySound("Play_HOC_Drone", base.gameObject);
            if (base.isAuthority && targetingController)
            {
                this.initialOrbTarget = targetingController.GetTrackingTarget();
                //handController.CmdHeal();
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
            base.OnExit();
        }

        private void FireMissile(HurtBox target, Vector3 position)
        {
            hasFired = true;
            FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
            fireProjectileInfo.position = base.inputBank.aimOrigin;

            fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(base.GetAimRay().direction);
            fireProjectileInfo.crit = base.RollCrit();
            fireProjectileInfo.damage = this.damageStat * FireSeekingDrone.damageCoefficient;
            fireProjectileInfo.damageColorIndex = DamageColorIndex.Item;
            fireProjectileInfo.owner = base.gameObject;
            fireProjectileInfo.force = FireSeekingDrone.force;
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
        public static float baseDuration;
        public static float healPercent;
        public static float force;

        private float duration;
        protected bool isCrit;
        private HurtBox initialOrbTarget = null;
        private TargetingController targetingController;
    }
}
