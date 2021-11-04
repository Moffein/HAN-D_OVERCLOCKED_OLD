using HAND_OVERCLOCKED.Components;
using RoR2;

namespace HAND_OVERCLOCKED.Hooks
{
    public class OnCharacterDeath
    {
        public OnCharacterDeath()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, report) =>
            {
                orig(self, report);
                if (report.victim && report.attacker && report.attackerBody && report.attackerBody.baseNameToken == ("HAND_OVERCLOCKED_NAME"))
                {
                    if (Util.CheckRoll(report.victim.globalDeathEventChanceCoefficient * 100f, report.attackerBody.master ? report.attackerBody.master.luck : 0f, null))
                    {
                        DroneStockController hc = report.attackerBody.gameObject.GetComponent<DroneStockController>();
                        if (hc)
                        {
                            hc.RpcAddSpecialStock();
                        }
                    }
                }
            };
        }
    }
}
