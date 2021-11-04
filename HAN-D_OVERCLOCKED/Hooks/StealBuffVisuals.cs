using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace HAND_OVERCLOCKED.Hooks
{
    public class StealBuffVisuals
    {
        public StealBuffVisuals()
        {
            IL.RoR2.CharacterModel.UpdateOverlays += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                     x => x.MatchLdsfld(typeof(RoR2Content.Buffs), "AttackSpeedOnCrit")
                    );
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasBuff, self) =>
                {
                    return hasBuff || self.body.HasBuff(HANDContent.DroneBuff) || self.body.HasBuff(HANDContent.OverclockBuff);
                });
            };
        }
    }
}
