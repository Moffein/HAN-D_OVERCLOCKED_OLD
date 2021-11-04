using R2API;
using HAND_OVERCLOCKED;

namespace HAND_OVERCLOCKED.Hooks
{
    public class GetStatCoefficients
    {
        public GetStatCoefficients()
        {
            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender.HasBuff(HANDContent.ParallelComputingBuff))
                {
                    int pcCount = sender.GetBuffCount(HANDContent.ParallelComputingBuff);
                    args.damageMultAdd += pcCount * 0.025f;
                    args.armorAdd += pcCount;
                }
                if (sender.HasBuff(HANDContent.OverclockBuff))
                {
                    args.attackSpeedMultAdd += 0.4f;
                    args.moveSpeedMultAdd += 0.4f;
                }
                if (sender.HasBuff(HANDContent.DroneBuff))
                {
                    args.attackSpeedMultAdd += 0.4f;
                    args.moveSpeedMultAdd += 0.4f;
                }
                if (!HAND_OVERCLOCKED.arenaActive && sender.HasBuff(HANDContent.DroneDebuff))
                {
                    args.moveSpeedReductionMultAdd += 0.6f;
                    args.damageMultAdd -= 0.3f;
                }
            };
        }
    }
}
