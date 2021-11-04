using RoR2;

namespace HAND_OVERCLOCKED.Hooks
{
    public class DisableFade
    {
        public DisableFade()
        {
            On.RoR2.CameraRigController.OnEnable += (orig, self) =>
            {
                SceneDef def = SceneCatalog.GetSceneDefForCurrentScene();
                if (def && def.baseSceneName.Equals("lobby"))
                {
                    self.enableFading = false;
                }
                orig(self);
            };
        }
    }
}
