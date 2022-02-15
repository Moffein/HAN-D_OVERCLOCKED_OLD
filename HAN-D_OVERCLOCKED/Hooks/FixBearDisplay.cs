using RoR2;

namespace HandPlugin.Hooks
{
    public class FixBearDisplay
    {
        public FixBearDisplay()
        {
            On.RoR2.CharacterModel.EnableItemDisplay += (orig, self, itemIndex) =>
            {
                if ((itemIndex != RoR2Content.Items.Bear.itemIndex) || self.name != "mdlHAND")
                {
                    orig(self, itemIndex);
                }
            };
        }
    }
}
