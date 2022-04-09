using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.HANDOverclocked;
using HandPlugin.Components;
using HandPlugin.Components.DroneProjectile;
using HandPlugin.Hooks;
using HandPlugin.Hooks.Arena;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using HandPlugin.Modules;
using UnityEngine.Networking;
using System.IO;

namespace HandPlugin
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.HAND_Overclocked", "HAN-D OVERCLOCKED BETA", "0.2.6")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(NetworkingAPI), nameof(RecalculateStatsAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency("com.DestroyedClone.AncientScepter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.ClassicItems", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Kingpinush.KingKombatArena", BepInDependency.DependencyFlags.SoftDependency)]
    class HAND_OVERCLOCKED : BaseUnityPlugin
    {
        public static GameObject HANDBody = null;
        public static GameObject HANDMonsterMaster = null;
        public static Color HANDColor = new Color(0.556862745f, 0.682352941f, 0.690196078f);
        public static Color OverclockColor = new Color(1.0f, 0.45f, 0f);

        String HANDBodyName = "";

        public CharacterBody body;
        public SkillLocator skillLocator;

        public static Shader hotpoo = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/hgstandard");

        public static bool arenaNerf = true;
        public static bool arenaPluginLoaded = false;
        public static bool arenaActive = false;

        public static bool changeSortOrder = false;

        public static SkillDef scepterDef;

        private void CreateSurvivorDef() {
            GameObject HANDDisplay = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/handbody").GetComponent<ModelLocator>().modelTransform.gameObject.InstantiateClone("HANDOverclockedDisplay", false);
            HANDDisplay.transform.localScale *= 0.9f;
            HANDDisplay.AddComponent<MenuAnimComponent>();
            SurvivorDef item = ScriptableObject.CreateInstance<SurvivorDef>();
            item.bodyPrefab = HANDBody;
            item.displayPrefab = HANDDisplay;
            item.descriptionToken = "HAND_OVERCLOCKED_DESC";
            item.outroFlavorToken = "HAND_OVERCLOCKED_OUTRO_FLAVOR";
            item.desiredSortPosition = changeSortOrder ? 5.4f : 100f;
            HANDContent.survivorDefs.Add(item);
        }

        public void ReadConfig()
        {
            changeSortOrder = base.Config.Bind<bool>(new ConfigDefinition("00 - General", "Change Sort Order"), false, new ConfigDescription("Sorts HAN-D among the vanilla survivors based on unlock condition.")).Value; ;
        }

        public void Awake()
        {
            ReadConfig();
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Kingpinush.KingKombatArena") && arenaNerf)
            {
                arenaPluginLoaded = true;
            }
            Debug.Log("\n\nSTATUS UPDATE:\n\nMACHINE ID:\t\tHAN-D\nLOCATION:\t\tAPPROACHING PETRICHOR V\nCURRENT OBJECTIVE:\tFIND AND ACTIVATE THE TELEPORTER\n\nPROVIDENCE IS DEAD.\nBLOOD IS FUEL.\nSPEED IS WAR.\n");
            CreateHAND();
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(RoR2.ContentManagement.ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new HANDContent());
        }

        private void AddHooks()
        {
            new DisableFade();
            new OnCharacterDeath();
            new FixBearDisplay();
            if (arenaPluginLoaded)
            {
                new Stage_Start();
                new AddTimedBuff();
            }
            new FixMenuUI();
        }

        private void CreateHAND() {
            SetBody();
            CreateVFX.Init();
            SetAttributes();
            HANDBuffs.Init();
            HANDMaster.Init();
            CreateSurvivorDef();

            LanguageTokens.Init();

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.DestroyedClone.AncientScepter"))
            {
                SetupScepter();
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.ThinkInvisible.ClassicItems"))
            {
                SetupScepterClassic();
            }
            AddHooks();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void SetupScepter()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(scepterDef, "HANDOverclockedBody", SkillSlot.Secondary, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void SetupScepterClassic()
        {
            ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(scepterDef, "HANDOverclockedBody", SkillSlot.Secondary, 0);
        }

        private void LoadAssets()
        {
            if (HANDContent.assets == null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HAN_D_OVERCLOCKED.handassets"))
                {
                    HANDContent.assets = AssetBundle.LoadFromStream(stream);
                }

                using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("HAN_D_OVERCLOCKED.HAND_Overclocked_Soundbank.bnk"))
                {
                    byte[] array = new byte[manifestResourceStream2.Length];
                    manifestResourceStream2.Read(array, 0, array.Length);
                    SoundAPI.SoundBanks.Add(array);
                }
            }
        }

        private void SetBody()
        {
            if (HANDBody == null)
            {
                //PrefabAPI errors here
                HANDBody = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/handbody").InstantiateClone("HANDOverclockedBody", true);
                HANDBodyName = HANDBody.name;
                PrefabAPI.RegisterNetworkPrefab(HANDBody);

                HurtBoxGroup hbg = HANDBody.GetComponent<ModelLocator>().modelTransform.GetComponent<HurtBoxGroup>();
                hbg.mainHurtBox.isSniperTarget = true;

                LoadAssets();
                HANDBody.GetComponent<CharacterBody>().portraitIcon = HANDContent.assets.LoadAsset<Texture2D>("Portrait.png");
                HANDContent.bodyPrefabs.Add(HANDBody);
            }
        }

        private void SetAttributes()
        {
            AddSkin();
            AddComponents();
            SetCharacterBodyAttributes();
            SetCameraAttributes();
            SetupSFX();
            FixSetStateOnHurt();
            SetDeathBehavior();
            //SetRagdoll();

            void AddComponents() {
                HANDBody.AddComponent<OverclockController>();
                HANDBody.AddComponent<DroneStockController>();
                HANDBody.AddComponent<TargetingController>();
                HANDBody.AddComponent<NetworkCommands>();
                HANDBody.AddComponent<DroneFollowerController>();
                HANDBody.tag = "Player";
            }
            void SetupSFX()
            {
                SfxLocator sfx = HANDBody.GetComponent<SfxLocator>();
                sfx.landingSound = "play_char_land";
                sfx.fallDamageSound = "Play_MULT_shift_hit";
            }
            void FixSetStateOnHurt()
            {
                SetStateOnHurt ssoh = HANDBody.AddComponent<SetStateOnHurt>();
                ssoh.canBeStunned = false;
                ssoh.canBeHitStunned = false;
                ssoh.canBeFrozen = true;
                ssoh.hitThreshold = 5;

                //Ice Fix Credits: SushiDev
                foreach (EntityStateMachine esm in HANDBody.GetComponentsInChildren<EntityStateMachine>())
                {
                    if (esm.customName == "Body")
                    {
                        ssoh.targetStateMachine = esm;
                        break;
                    }
                }
            }
            void SetDeathBehavior() {
                CharacterDeathBehavior handCDB = HANDBody.GetComponent<CharacterDeathBehavior>();
                handCDB.deathState = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/WispBody").GetComponent<CharacterDeathBehavior>().deathState;
            }

            void SetCameraAttributes() {
                CameraTargetParams cameraTargetParams = HANDBody.GetComponent<CameraTargetParams>();
                cameraTargetParams.cameraParams = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/toolbotbody").GetComponent<CameraTargetParams>().cameraParams;
                cameraTargetParams.cameraParams.data.idealLocalCameraPos = new Vector3(0f, 1f, -11f);
            }

            void SetCharacterBodyAttributes() {
                body = HANDBody.GetComponent<CharacterBody>();
                body.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes | CharacterBody.BodyFlags.Mechanical;
                body.subtitleNameToken = "HAND_OVERCLOCKED_SUBTITLE";
                body._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/crosshair/simpledotcrosshair");
                body.hideCrosshair = false;
                body.bodyColor = HANDColor;
                body.baseNameToken = "HAND_OVERCLOCKED_NAME";

                //base stats
                body.baseMaxHealth = 160f;
                body.baseRegen = 2.5f;
                body.baseMaxShield = 0f;
                body.baseMoveSpeed = 7f;
                body.baseAcceleration = 30f;
                body.baseJumpPower = 15f;
                body.baseDamage = 14f;
                body.baseAttackSpeed = 1f;
                body.baseCrit = 1f;
                body.baseArmor = 0f;
                body.baseJumpCount = 1;

                //leveling stats
                body.autoCalculateLevelStats = true;
                body.PerformAutoCalculateLevelStats();

                body.spreadBloomDecayTime = 1f;
                body.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/robocratepod");
            }

            HANDBody.GetComponent<ModelLocator>().modelTransform.localScale *= 1.2f;

            HANDSkills.Init(HANDBody);
        }

        private void AddSkin()    //credits to rob
        {
            GameObject model = HANDBody.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            SkinnedMeshRenderer FixRenderInfo()
            {
                SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer; //Reflection.GetFieldValue<CharacterModel.RendererInfo>(modelController, "baseRenderInfos")
                if (mainRenderer == null)
                {
                    CharacterModel.RendererInfo[] info = characterModel.baseRendererInfos;
                    if (info != null)
                    {
                        foreach (CharacterModel.RendererInfo rendererInfo in info)
                        {
                            if (rendererInfo.renderer is SkinnedMeshRenderer)
                            {
                                mainRenderer = (SkinnedMeshRenderer)rendererInfo.renderer;
                                break;
                            }
                        }
                        if (mainRenderer != null)
                        {
                            characterModel.mainSkinnedMeshRenderer = mainRenderer;
                        }
                    }
                }
                return mainRenderer;
            }

            void CreateSkinInfo(SkinnedMeshRenderer mainRenderer)
            {
                ModelSkinController skinController = model.GetComponent<ModelSkinController>();
                if (!skinController)
                {
                    skinController = model.AddComponent<ModelSkinController>();
                }

                LoadoutAPI.SkinDefInfo defaultSkinInfo = default(LoadoutAPI.SkinDefInfo);
                defaultSkinInfo.BaseSkins = Array.Empty<SkinDef>();
                defaultSkinInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
                defaultSkinInfo.Icon = LoadoutAPI.CreateSkinIcon(new Color(0f, 156f / 255f, 188f / 255f), new Color(186f / 255f, 128f / 255f, 52f / 255f), new Color(58f / 255f, 49f / 255f, 24f / 255f), new Color(2f / 255f, 29f / 255f, 55f / 255f));

                defaultSkinInfo.MeshReplacements = new SkinDef.MeshReplacement[] {
                    new SkinDef.MeshReplacement {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                    }
                };
                defaultSkinInfo.MinionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>();
                defaultSkinInfo.Name = "DEFAULT_SKIN";
                defaultSkinInfo.NameToken = "DEFAULT_SKIN";
                defaultSkinInfo.ProjectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>();
                defaultSkinInfo.RendererInfos = characterModel.baseRendererInfos;
                defaultSkinInfo.RootObject = model;

                SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(defaultSkinInfo);

                skinController.skins = new SkinDef[1]
                {
                defaultSkin
                };
            }

            CreateSkinInfo(FixRenderInfo());
        }
    }
}
