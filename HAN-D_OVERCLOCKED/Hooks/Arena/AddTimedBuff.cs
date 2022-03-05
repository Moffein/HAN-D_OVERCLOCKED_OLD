using HandPlugin.Components;
//using NS_KingKombatArena;
using RoR2;
using System.Runtime.CompilerServices;

namespace HandPlugin.Hooks.Arena
{
    /*public class AddTimedBuff
    {
        public AddTimedBuff()
        {
            On.RoR2.CharacterBody.AddTimedBuff_BuffIndex_float += (orig, self, buffIndex, duration) =>
            {
                orig(self, buffIndex, duration);

                if (HAND_OVERCLOCKED.arenaActive && buffIndex == RoR2Content.Buffs.Immune.buffIndex
                && self.baseNameToken == "HAND_OVERCLOCKED_NAME" && IsSameDurationAsKingImmunity(duration))
                {
                    NetworkCommands nc = self.gameObject.GetComponent<NetworkCommands>();
                    if (nc)
                    {
                        nc.ResetSpecialStock();
                    }
                }
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool IsSameDurationAsKingImmunity(float duration)
        {
            return duration == KingKombatArenaMainPlugin.AccessCurrentKombatArenaInstance().GetKombatArenaRules().duelStartImmunityTime;
        }
    }*/
}
