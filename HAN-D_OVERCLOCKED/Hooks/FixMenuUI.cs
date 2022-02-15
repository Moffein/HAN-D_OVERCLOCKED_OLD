using HAND_OVERCLOCKED.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace HAND_OVERCLOCKED.Hooks
{
    public class FixMenuUI
    {
        public FixMenuUI()
        {
            On.RoR2.NetworkUIPromptController.OnControlBegin += (orig, self) =>
            {
                orig(self);

                if (self.currentLocalParticipant.cachedBodyObject)
                {
                    OverclockController rc = self.currentLocalParticipant.cachedBodyObject.GetComponent<OverclockController>();
                    if (rc)
                    {
                        rc.menuActive = true;
                    }
                }
            };

            On.RoR2.NetworkUIPromptController.OnControlEnd += (orig, self) =>
            {
                orig(self);

                if (self.currentLocalParticipant.cachedBodyObject)
                {
                    OverclockController rc = self.currentLocalParticipant.cachedBodyObject.GetComponent<OverclockController>();
                    if (rc)
                    {
                        rc.menuActive = false;
                    }
                }
            };
        }
    }
}
