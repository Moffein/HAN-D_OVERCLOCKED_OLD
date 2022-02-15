using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HAND_OVERCLOCKED.Modules
{
    public class HANDBuffs
    {
        public static bool initialized = false;

        public static BuffDef OverclockBuff;
        public static BuffDef DroneBuff;
        public static BuffDef DroneDebuff;
        public static BuffDef ParallelComputingBuff;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            BuffDef ParallelBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            ParallelBuffDef.buffColor = HAND_OVERCLOCKED.HANDColor;
            ParallelBuffDef.canStack = true;
            ParallelBuffDef.iconSprite = RoR2Content.Buffs.Overheat.iconSprite;
            ParallelBuffDef.isDebuff = false;
            ParallelBuffDef.name = "MoffeinHANDParallelComputing";
            FixScriptableObjectName(ParallelBuffDef);
            HANDContent.buffDefs.Add(ParallelBuffDef);
            ParallelComputingBuff = ParallelBuffDef;

            BuffDef OverclockBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            OverclockBuffDef.buffColor = HAND_OVERCLOCKED.OverclockColor;
            OverclockBuffDef.canStack = false;
            OverclockBuffDef.iconSprite = RoR2Content.Buffs.TeslaField.iconSprite;
            OverclockBuffDef.isDebuff = false;
            OverclockBuffDef.name = "MoffeinHANDOverclock";
            FixScriptableObjectName(OverclockBuffDef);
            HANDContent.buffDefs.Add(OverclockBuffDef);
            OverclockBuff = OverclockBuffDef;

            BuffDef DroneBoostDef = ScriptableObject.CreateInstance<BuffDef>();
            DroneBoostDef.buffColor = HAND_OVERCLOCKED.OverclockColor;
            DroneBoostDef.canStack = false;
            DroneBoostDef.iconSprite = RoR2Content.Buffs.HiddenInvincibility.iconSprite;
            DroneBoostDef.isDebuff = false;
            DroneBoostDef.name = "MoffeinHANDDroneBoost";
            FixScriptableObjectName(DroneBoostDef);
            HANDContent.buffDefs.Add(DroneBoostDef);
            DroneBuff = DroneBoostDef;

            BuffDef DroneDebuffDef = ScriptableObject.CreateInstance<BuffDef>();
            DroneDebuffDef.buffColor = HAND_OVERCLOCKED.HANDColor;
            DroneDebuffDef.canStack = false;
            DroneDebuffDef.iconSprite = RoR2Content.Buffs.Weak.iconSprite;
            DroneDebuffDef.isDebuff = true;
            DroneDebuffDef.name = "MoffeinHANDDroneDebuff";
            FixScriptableObjectName(DroneDebuffDef);
            HANDContent.buffDefs.Add(DroneDebuffDef);
            DroneDebuff = DroneDebuffDef;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            StealBuffVisuals();
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(ParallelComputingBuff))
            {
                int pcCount = sender.GetBuffCount(ParallelComputingBuff);
                args.damageMultAdd += pcCount * 0.025f;
                args.armorAdd += pcCount;
            }
            if (sender.HasBuff(OverclockBuff))
            {
                args.attackSpeedMultAdd += 0.4f;
                args.moveSpeedMultAdd += 0.4f;
            }
            if (sender.HasBuff(DroneBuff))
            {
                args.armorAdd += 50f;
            }
            if (!HAND_OVERCLOCKED.arenaActive && sender.HasBuff(DroneDebuff))
            {
                args.moveSpeedReductionMultAdd += 0.6f;
                args.damageMultAdd -= 0.3f;
            }
        }

        private static void StealBuffVisuals()
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
                    return hasBuff || self.body.HasBuff(DroneBuff) || self.body.HasBuff(OverclockBuff);
                });
            };
        }

        private static void FixScriptableObjectName(BuffDef bd)
        {
            (bd as ScriptableObject).name = bd.name;
        }
    }
}
