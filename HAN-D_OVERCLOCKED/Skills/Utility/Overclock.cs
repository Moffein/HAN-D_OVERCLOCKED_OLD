using System;
using System.Collections.Generic;
using System.Text;
using HandPlugin.Components;
using RoR2;
using UnityEngine;

namespace EntityStates.HANDOverclocked
{
    public class Overclock : BaseState
    {
        public override void OnEnter()
        {
            EffectManager.SpawnEffect(effectPrefab, new EffectData
            {
                origin = base.transform.position
            }, false);
            OverclockController hc = base.gameObject.GetComponent<OverclockController>();
            if (hc && base.isAuthority)
            {
                if (hc.ovcActive)
                {
                    if (base.characterMotor)
                    {
                        base.SmallHop(base.characterMotor, 22f);
                    }
                    hc.EndOverclock();
                }
                else
                {
                    hc.BeginOverclock();
                    if (base.isAuthority && characterBody.skillLocator.utility.stock < characterBody.skillLocator.utility.maxStock)
                    {
                        characterBody.skillLocator.utility.stock++;
                    }
                    if (base.characterMotor && !base.characterMotor.isGrounded)
                    {
                        base.SmallHop(base.characterMotor, 22f);
                    }
                }
            }
            this.outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }

        public static GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/smokescreeneffect");
    }
}