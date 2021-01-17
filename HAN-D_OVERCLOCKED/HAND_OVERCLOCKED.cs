using BepInEx;
using EntityStates;
using EntityStates.HANDOverclocked;
using MonoMod.Cil;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Orbs;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.HAND_Overclocked", "HAN-D OVERCLOCKED BETA", "0.0.12")]
    [R2APISubmoduleDependency(nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(ResourcesAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(NetworkingAPI), nameof(EffectAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    class HAND_OVERCLOCKED : BaseUnityPlugin
    {
        public static GameObject HANDBody = null;
        public static GameObject droneProjectileOrb = null;
        GameObject HANDMonsterMaster = null;
        Texture2D HANDIcon = null;
        Color HANDColor = new Color(0.556862745f, 0.682352941f, 0.690196078f);
        Color OverclockColor = new Color(1.0f, 0.45f, 0f);

        String HANDBodyName = "";

        public static BuffIndex OverclockBuff;
        public static BuffIndex steamBuff;

        public static float overclockAtkSpd;
        public static float overclockSpd;

        const String assetPrefix = "@MoffeinHAND_OVERCLOCKED";
        const String portraitPath = assetPrefix + ":handprofile2.png";

        bool useBodyClone = true;

        public CharacterBody body;
        public SkillLocator skillLocator;

        public static AssetBundle HANDAssetBundle = null;
        public static AssetBundleResourcesProvider HANDProvider;

        public static GameObject slamEffect;

        #region HAN-D
        public static Sprite winchIcon = null;
        public static Texture2D portraitIcon = null;
        public static Sprite droneIcon = null;
        public static Sprite forcedReassemblyIcon = null;
        public static Sprite overclockBuffIcon = null;
        public static Sprite passiveIcon = null;
        public static Sprite hurtIcon = null;
        public static Sprite overclockIcon = null;
        public static Sprite explungeIcon = null;
        public static Sprite passiveDroneBuffIcon = null;
        public static Sprite unethicalReassemblyIcon = null;
        public static Sprite passive = null;
        #endregion

        /*public void Start()
        {
            //never do anything in start, i BEG YOU
            SetAttributes();
            InitSkills();
            AssignSkills();
            //MutilateBody();
        }*/

        private void CreateSurvivorDef() {
            GameObject HANDDisplay = HANDBody.GetComponent<ModelLocator>().modelTransform.gameObject;
            HANDDisplay.AddComponent<MenuAnimComponent>();


            string HANDDesc = "";
            HANDDesc += "HAN-D is a robot janitor whose powerful melee attacks are sure to leave a mess!<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > HURT has increased knockback against airborne enemies. Use FORCED_REASSEMBLY to pop enemies in the air, then HURT them to send them flying!" + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > FORCED_REASSEMBLY's self-knockback can be used to reach flying enemies." + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > OVERCLOCK lasts as long as you can keep hitting enemies." + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > Use DRONES to heal and stay in the fight." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add("KEYWORD_HANDOVERCLOCKED_SPRINGY", "<style=cKeywordName>Springy</style><style=cSub>The skill boosts you upwards when used.</style>");
            LanguageAPI.Add("HAND_OVERCLOCKED_NAME", "HAN-D");
            LanguageAPI.Add("HAND_OVERCLOCKED_OUTRO_FLAVOR", "..and so it left, unrewarded in all of its efforts.");
            String tldr = "<style=cMono>\r\n//--AUTO-TRANSCRIPTION FROM BASED DEPARTMENT OF UES SAFE TRAVELS--//</style>\r\n\r\n<i>*hits <color=#327FFF>Spinel Tonic</color>*</i>\n\nIs playing without the <color=#6955A6>Command</color> artifact the ultimate form of cuckoldry?\n\nI cannot think or comprehend of anything more cucked than playing without <color=#6955A6>Command</color>. Honestly, think about it rationally. You are shooting, running, jumping for like 60 minutes solely so you can get a fucking <color=#77FF16>Squid Polyp</color>. All that hard work you put into your run - dodging <style=cIsHealth>Stone Golem</style> lasers, getting annoyed by six thousand <style=cIsHealth>Lesser Wisps</color> spawning above your head, activating <color=#E5C962>Shrines of the Mountain</color> all for one simple result: your inventory is filled up with <color=#FFFFFF>Warbanners</color> and <color=#FFFFFF>Monster Tooth</color> necklaces which cost money.\n\nOn a god run? Great. A bunch of shitty items which add nothing to your run end up coming out of the <color=#E5C962>Chests</color> you buy. They get the benefit of your hard earned dosh that came from killing <style=cIsHealth>Lemurians</style>.\n\nAs a man who plays this game you are <style=cIsHealth>LITERALLY</style> dedicating two hours of your life to opening boxes and praying it's not another <color=#77FF16>Chronobauble</color>. It's the ultimate and final cuck. Think about it logically.\r\n<style=cMono>\r\nTranscriptions complete.\r\n</style>\r\n \r\n\r\n";
            LanguageAPI.Add("HAND_OVERCLOCKED_LORE", tldr);
            LanguageAPI.Add("HANDOCBODY_DEFAULT_SKIN_NAME", "Default");

            SurvivorDef item = new SurvivorDef
            {
                bodyPrefab = HANDBody,
                descriptionToken = HANDDesc,
                displayPrefab = HANDDisplay,
                primaryColor = HANDColor,
                unlockableName = "",
                outroFlavorToken = "HAND_OVERCLOCKED_OUTRO_FLAVOR"
            };
            SurvivorAPI.AddSurvivor(item);
        }


        public void Awake()
        {
            LogCore.logger = base.Logger;

            LogCore.LogI("\n\nSTATUS UPDATE:\n\nMACHINE ID:\t\tHAN-D\nLOCATION:\t\tAPPROACHING PETRICHOR V\nCURRENT OBJECTIVE:\tFIND AND ACTIVATE THE TELEPORTER\n\nPROVIDENCE IS DEAD.\nBLOOD IS FUEL.\nSPEED IS WAR.\n");


            CreateHAND();
        }

        private void Hook() {
            On.RoR2.CameraRigController.OnEnable += CameraRigController_OnEnable;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.CharacterModel.EnableItemDisplay += CharacterModel_EnableItemDisplay;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }
        #region Hooks
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self && self.HasBuff(OverclockBuff))
            {
                int ovcCount = self.GetBuffCount(OverclockBuff);
                self.SetPropertyValue<float>("attackSpeed", self.attackSpeed * (1 + overclockAtkSpd));
                //self.SetPropertyValue<float>("armor", self.armor + overclockArmor);
                self.SetPropertyValue<float>("moveSpeed", self.moveSpeed * (1 + overclockSpd));
            }
        }

        private void CharacterModel_EnableItemDisplay(On.RoR2.CharacterModel.orig_EnableItemDisplay orig, CharacterModel self, ItemIndex itemIndex)
        {
            if ((itemIndex != ItemIndex.Bear) || self.name != "mdlHAND")
            {
                orig(self, itemIndex);
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport report)
        {
            orig(self, report);
            if (report.victim && report.attacker && report.attackerBody && report.attackerBody.gameObject.name == (HANDBodyName + "(Clone)"))
            {
                if (Util.CheckRoll(report.victim.globalDeathEventChanceCoefficient * 100f, report.attackerBody.master ? report.attackerBody.master.luck : 0f, null))
                {
                    HANDController hc = report.attackerBody.gameObject.GetComponent<HANDController>();
                    if (hc)
                    {
                        hc.RpcAddSpecialStock();
                    }
                }

            }
        }

        private void CameraRigController_OnEnable(On.RoR2.CameraRigController.orig_OnEnable orig, CameraRigController self)
        {
            //prevents null refs when loading into TestScene as it has no scenedef
            var def = SceneCatalog.GetSceneDefForCurrentScene();
            if (def && def.baseSceneName.Equals("lobby"))
            {
                self.enableFading = false;
            }
            orig(self);
        }
        #endregion

        private void CreateHAND() {
            SetBody();
            CreateSlamEffect();
            CreateBuffs();
            Repair();
            SetAttributes();

            CreateMaster();
            CreateSurvivorDef();

            //and lastly
            Hook();
        }

        private void CreateSlamEffect()
        {
            slamEffect = Resources.Load<GameObject>("prefabs/effects/impacteffects/ParentSlamEffect").InstantiateClone("OrbitalImpactEffect", false);

            var particleParent = slamEffect.transform.Find("Particles");
            var debris = particleParent.Find("Debris, 3D");
            var debris2 = particleParent.Find("Debris");
            var sphere = particleParent.Find("Nova Sphere");

            debris.gameObject.SetActive(false);
            debris2.gameObject.SetActive(false);
            sphere.gameObject.SetActive(false);

            EffectAPI.AddEffect(slamEffect);
        }

        private void Repair() {
            RepairSwingEffect();
            AddSkin();
        }

        private void LoadAssets()
        {
            if (HANDAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HAN_D_OVERCLOCKED.hand_assets"))
                {
                    HANDAssetBundle = AssetBundle.LoadFromStream(assetStream);
                    HANDProvider = new AssetBundleResourcesProvider("@MoffeinHAND_OVERCLOCKED", HANDAssetBundle);
                    ResourcesAPI.AddProvider(HANDProvider);
                }

                LoadIcons();
            }
            void LoadIcons()
            {
                winchIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Winch.png");
                portraitIcon = HANDAssetBundle.LoadAsset<Texture2D>("Assets/Import/HAND_ICONS/Portrait.png");
                droneIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Drone.png");
                forcedReassemblyIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Forced_Reassembly.png");
                overclockBuffIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/OverclockBuff.png");
                passiveIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Passive.png");
                hurtIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Hurt.png");
                overclockIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Overclock.png");
                explungeIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Explunge.png");
                passiveDroneBuffIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/PassiveDrone.png");
                unethicalReassemblyIcon = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Unethical_Reassembly.png");
                passive = HANDAssetBundle.LoadAsset<Sprite>("Assets/Import/HAND_ICONS/Passive.png");
            }
        }

        private void SetBody()
        {
            if (HANDBody == null)
            {
                if (useBodyClone)
                {
                    HANDBody = Resources.Load<GameObject>("prefabs/characterbodies/handbody").InstantiateClone("HANDOverclockedBody", true);
                    BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
                    {
                        list.Add(HANDBody);
                    };
                }
                else
                {
                    HANDBody = Resources.Load<GameObject>("prefabs/characterbodies/handbody");
                }
                HANDBodyName = HANDBody.name;
                LoadAssets();
                HANDBody.GetComponent<CharacterBody>().portraitIcon = HANDIcon;
            }
            if (droneProjectileOrb == null)
            {
                /*if (useBodyClone)
                {
                    droneProjectileOrb = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/ArrowOrbEffect").InstantiateClone("HOCDroneOrb", false);
                }
                else
                {
                    droneProjectileOrb = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/ArrowOrbEffect");
                }*/
                droneProjectileOrb = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/ArrowOrbEffect");
            }
        }

        private void SetAttributes()
        {
            AddComponents();
            SetCharacterBodyAttributes();
            SetupSFX();
            FixSetStateOnHurt();
            SetDeathBehavior();

            void AddComponents() {
                HANDBody.AddComponent<HANDController>();
                //body = HANDBody.GetComponent<CharacterBody>();
                HANDBody.tag = "Player";
            }
            void SetupSFX()
            {
                SfxLocator sfx = HANDBody.GetComponent<SfxLocator>();
                sfx.landingSound = "play_char_land";
                sfx.fallDamageSound = "Play_MULT_shift_hit";
            }
            void FixSetStateOnHurt() {
                SetStateOnHurt ssoh = HANDBody.AddComponent<SetStateOnHurt>();
                ssoh.canBeStunned = false;
                ssoh.canBeHitStunned = false;
                ssoh.canBeFrozen = true;
                ssoh.hitThreshold = 5;

                //Ice Fix Credits: SushiDev
                int i = 0;
                EntityStateMachine[] esmr = new EntityStateMachine[2];
                foreach (EntityStateMachine esm in HANDBody.GetComponentsInChildren<EntityStateMachine>())
                {
                    switch (esm.customName)
                    {
                        case "Body":
                            ssoh.targetStateMachine = esm;
                            break;
                        default:
                            if (i < 2)
                            {
                                esmr[i] = esm;
                            }
                            i++;
                            break;
                    }
                }

            }
            void SetDeathBehavior() {
                CharacterDeathBehavior handCDB = HANDBody.GetComponent<CharacterDeathBehavior>();
                handCDB.deathState = Resources.Load<GameObject>("prefabs/characterbodies/WispBody").GetComponent<CharacterDeathBehavior>().deathState;
            }
            void SetCharacterBodyAttributes() {
                body = HANDBody.GetComponent<CharacterBody>();
                body.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
                body.subtitleNameToken = "Lean, Mean, Cleaning Machine";
                body.crosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/simpledotcrosshair");
                body.hideCrosshair = false;

                body.baseNameToken = "HAND_OVERCLOCKED_NAME";

                //base stats
                body.baseMaxHealth = 160f;
                body.baseRegen = 2.5f;
                body.baseMaxShield = 0f;
                body.baseMoveSpeed = 7f;
                body.baseAcceleration = 30f;
                body.baseJumpPower = 15f;
                body.baseDamage = 12f;
                body.baseAttackSpeed = 1f;
                body.baseCrit = 1f;
                body.baseArmor = 20f;
                body.baseJumpCount = 1;

                //leveling stats
                body.autoCalculateLevelStats = true;
                body.levelMaxHealth = body.baseMaxHealth * 0.3f;
                body.levelRegen = body.baseRegen * 0.2f;
                body.levelMaxShield = 0f;
                body.levelMoveSpeed = 0f;
                body.levelJumpPower = 0f;
                body.levelDamage = body.baseDamage * 0.2f;
                body.levelAttackSpeed = 0f;
                body.levelCrit = 0f;
                body.levelArmor = 0f;

                body.spreadBloomDecayTime = 1f;
                body.preferredPodPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/robocratepod");
            }

            HANDBody.GetComponent<CameraTargetParams>().idealLocalCameraPos = new Vector3(0f, 0f, -4.7f);

            CreateSkills();
        }

        #region Skills
        private void CreateSkills() {
            skillLocator = HANDBody.GetComponent<SkillLocator>();

            CreatePrimary();
            CreateSecondary();
            CreateUtility();
            CreateSpecial();
        }


        private void CreatePrimary() {
            FullSwing.damageCoefficient = 3.9f;
            FullSwing.baseDuration = 1f;
            FullSwing.airbornVerticalForce = 0f;
            FullSwing.forceMagnitude = 1400f;
            FullSwing.airbornHorizontalForceMult = 1.8f;
            FullSwing.flyingHorizontalForceMult = 0.5f;
            FullSwing.shorthopVelocityFromHit = 10f;
            FullSwing.returnToIdlePercentage = 0.443662f;
            FullSwing.swingEffectPrefab = Resources.Load<GameObject>("prefabs/effects/handslamtrail");

            LoadoutAPI.AddSkill(typeof(FullSwing));

            SkillDef primarySkill = SkillDef.CreateInstance<SkillDef>();
            primarySkill.activationState = new SerializableEntityStateType(typeof(FullSwing));
            primarySkill.skillNameToken = "HURT";
            primarySkill.skillName = "FullSwing";
            primarySkill.skillDescriptionToken = "<style=cIsUtility>Agile</style>. Swing your hammer in a wide arc, hurting enemies for <style=cIsDamage>" + FullSwing.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>.";
            primarySkill.noSprint = false;
            primarySkill.canceledFromSprinting = false;
            primarySkill.baseRechargeInterval = 0f;
            primarySkill.baseMaxStock = 1;
            primarySkill.rechargeStock = 1;
            primarySkill.isBullets = false;
            primarySkill.shootDelay = 0.1f;
            primarySkill.beginSkillCooldownOnSkillEnd = false;
            primarySkill.activationStateMachineName = "Weapon";
            primarySkill.interruptPriority = EntityStates.InterruptPriority.Any;
            primarySkill.isCombatSkill = true;
            primarySkill.mustKeyPress = false;
            primarySkill.icon = hurtIcon;
            primarySkill.requiredStock = 1;
            primarySkill.stockToConsume = 1;
            primarySkill.keywordTokens = new string[] { "KEYWORD_AGILE" };
            LoadoutAPI.AddSkillDef(primarySkill);

            SkillFamily primarySkillFamily = skillLocator.primary.skillFamily;

            primarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = primarySkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primarySkill.skillNameToken, false, null)

            };
        }

        private void CreateSecondary() {
            ChargeSlam2.baseMinDuration = 0.6f;
            ChargeSlam2.baseChargeDuration = 1.4f;
            Slam2.baseDuration = 0.6f;
            Slam2.damageCoefficientMin = 4f;
            Slam2.damageCoefficientMax = 12f;
            Slam2.baseMinDuration = 0.4f;
            Slam2.forceMagnitudeMin = 2000f;
            Slam2.forceMagnitudeMax = 2000f;
            Slam2.airbornVerticalForceMin = -2400f;
            Slam2.airbornVerticalForceMax = -3000f;
            Slam2.shorthopVelocityFromHit = 24f;
            Slam2.hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactToolbotDashLarge");
            Slam2.impactEffectPrefab = slamEffect;//Resources.Load<GameObject>("prefabs/effects/impacteffects/PodGroundImpact");
            Slam2.swingEffectPrefab = Resources.Load<GameObject>("prefabs/effects/handslamtrail");
            Slam2.returnToIdlePercentage = 0.443662f;
            Slam2.minRange = 9;
            Slam2.maxRange = 22;
            LoadoutAPI.AddSkill(typeof(ChargeSlam2));
            LoadoutAPI.AddSkill(typeof(Slam2));

            SkillDef secondarySkill = SkillDef.CreateInstance<SkillDef>();
            secondarySkill.activationState = new SerializableEntityStateType(typeof(ChargeSlam2));
            secondarySkill.skillNameToken = "FORCED_REASSEMBLY";
            secondarySkill.skillName = "ChargeSlam";
            secondarySkill.skillDescriptionToken = "<style=cIsUtility>Springy</style>. Charge up a powerful hammer slam for <style=cIsDamage>400%-1200% damage</style>. <style=cIsDamage>Range and knockback</style> increases with charge.";
            secondarySkill.noSprint = false;
            secondarySkill.canceledFromSprinting = false;
            secondarySkill.baseRechargeInterval = 5f;
            secondarySkill.baseMaxStock = 1;
            secondarySkill.rechargeStock = 1;
            secondarySkill.requiredStock = 1;
            secondarySkill.stockToConsume = 1;
            secondarySkill.activationStateMachineName = "Weapon";
            secondarySkill.interruptPriority = EntityStates.InterruptPriority.Skill;
            secondarySkill.isCombatSkill = true;
            secondarySkill.mustKeyPress = false;
            secondarySkill.icon = forcedReassemblyIcon;
            secondarySkill.isBullets = false;
            secondarySkill.shootDelay = 0.08f;
            secondarySkill.beginSkillCooldownOnSkillEnd = true;
            secondarySkill.keywordTokens = new string[] { "KEYWORD_STUNNING", "KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            LoadoutAPI.AddSkillDef(secondarySkill);

            SkillFamily secondarySkillFamily = skillLocator.secondary.skillFamily;

            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = secondarySkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(secondarySkill.skillNameToken, false, null)

            };
        }

        private void CreateUtility() {
            overclockAtkSpd = 0.4f;
            overclockSpd = 0.3f;
            LoadoutAPI.AddSkill(typeof(Overclock));

            SkillDef ovcSkill = SkillDef.CreateInstance<SkillDef>();
            ovcSkill.activationState = new SerializableEntityStateType(typeof(Overclock));
            ovcSkill.skillNameToken = "OVERCLOCK";
            ovcSkill.skillName = "Overclock";
            //ovcSkill.skillDescriptionToken = "<style=cIsUtility>Springy</style>. Gain a brief <style=cIsUtility>burst of speed</style> and activate <style=cIsDamage>OVERCLOCK</style> if it is available.";
            ovcSkill.skillDescriptionToken = "Gain <style=cIsUtility>+30% movement speed</style>, <style=cIsDamage>+40% attack speed</style>, and <style=cIsDamage>stun chance</style>. <style=cIsUtility>Hit enemies to increase duration</style>. Cancel OVERCLOCK to release steam and <style=cIsUtility>Spring</style> into the air, damaging enemies for <style=cIsDamage>200%-600% damage</style>.";
            ovcSkill.isCombatSkill = false;
            ovcSkill.noSprint = false;
            ovcSkill.canceledFromSprinting = false;
            ovcSkill.baseRechargeInterval = 7f;
            ovcSkill.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
            ovcSkill.mustKeyPress = true;
            ovcSkill.beginSkillCooldownOnSkillEnd = false;
            ovcSkill.baseMaxStock = 1;
            ovcSkill.fullRestockOnAssign = true;
            ovcSkill.rechargeStock = 1;
            ovcSkill.requiredStock = 1;
            ovcSkill.stockToConsume = 1;
            ovcSkill.icon = overclockIcon;
            ovcSkill.activationStateMachineName = "Hook";
            ovcSkill.isBullets = false;
            ovcSkill.shootDelay = 0f;
            ovcSkill.keywordTokens = new string[] { "KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            LoadoutAPI.AddSkillDef(ovcSkill);

            SkillFamily utilitySkillFamily = skillLocator.utility.skillFamily;

            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = ovcSkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(ovcSkill.skillNameToken, false, null)
            };

        }

        private void CreateSpecial() {
            FireSeekingDrone.damageCoefficient = 2.7f;
            FireSeekingDrone.projectilePrefab = Resources.Load<GameObject>("prefabs/projectiles/EngiHarpoon");
            FireSeekingDrone.baseDuration = 0.25f;
            FireSeekingDrone.healPercent = 0.1f;
            FireSeekingDrone.effectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");

            SkillDef droneSkill = SkillDef.CreateInstance<SkillDef>();
            droneSkill.activationState = new SerializableEntityStateType(typeof(FireSeekingDrone));
            droneSkill.skillNameToken = "DRONES";
            droneSkill.skillName = "Drones";
            //droneSkill.skillDescriptionToken = "Increase <style=cIsUtility>move speed</style> and <style=cIsDamage>attack speed by " + Overclock.attackSpeedBonus.ToString("P0").Replace(" ", "") + "</style>.";
            //droneSkill.skillDescriptionToken += " All attacks <style=cIsHealing>give a temporary barrier on hit.</style> <style=cIsUtility>Increase duration by attacking enemies</style>.";
            droneSkill.skillDescriptionToken = "Expel a helper drone, <style=cIsHealing>healing yourself for 10% HP</style> and dealing <style=cIsDamage>" + FireSeekingDrone.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>. ";
            droneSkill.skillDescriptionToken += "<style=cIsUtility>Gain charges by killing enemies or hitting bosses.</style>";
            droneSkill.isCombatSkill = true;
            droneSkill.noSprint = false;
            droneSkill.canceledFromSprinting = false;
            droneSkill.baseRechargeInterval = 0f;
            droneSkill.interruptPriority = EntityStates.InterruptPriority.Any;
            droneSkill.mustKeyPress = false;
            droneSkill.beginSkillCooldownOnSkillEnd = true;
            droneSkill.baseMaxStock = 10;
            droneSkill.fullRestockOnAssign = false;
            droneSkill.rechargeStock = 0;
            droneSkill.requiredStock = 1;
            droneSkill.stockToConsume = 1;
            droneSkill.icon = droneIcon;
            droneSkill.activationStateMachineName = "Hook";
            droneSkill.isBullets = false;
            droneSkill.shootDelay = 0.1f;
            LoadoutAPI.AddSkillDef(droneSkill);

            SkillFamily specialSkillFamily = skillLocator.special.skillFamily;

            specialSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = droneSkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(droneSkill.skillNameToken, false, null)
            };
        }
        #endregion

        private void CreateBuffs()
        {
            BuffDef OverclockBuffDef = new BuffDef
            {
                buffColor = OverclockColor,
                buffIndex = BuffIndex.Count,
                canStack = false,
                eliteIndex = EliteIndex.None,
                iconPath = "Textures/BuffIcons/texBuffTeslaIcon",
                isDebuff = false,
                name = "MoffeinHANDOverclock"
            };
            BuffDef steamDef = new BuffDef
            {
                buffColor = hexToColor("7e7474"),
                buffIndex = BuffIndex.Count,
                canStack = true,
                eliteIndex = EliteIndex.None,
                iconPath = "Textures/BuffIcons/texBuffOnFireIcon",
                isDebuff = false,
                name = "MoffeinHANDSteam"
            };

            HAND_OVERCLOCKED.steamBuff = BuffAPI.Add(new CustomBuff(steamDef));
            HAND_OVERCLOCKED.OverclockBuff = BuffAPI.Add(new CustomBuff(OverclockBuffDef));
        }

        public static Color hexToColor(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }

        #region Repair
        private void AddSkin()    //credits to rob
        {
            GameObject model = HANDBody.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            SkinnedMeshRenderer FixRenderInfo()
            {
                SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer"); //Reflection.GetFieldValue<CharacterModel.RendererInfo>(modelController, "baseRenderInfos")
                if (mainRenderer == null)
                {
                    CharacterModel.RendererInfo[] info = Reflection.GetFieldValue<CharacterModel.RendererInfo[]>(characterModel, "baseRendererInfos");
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
                            characterModel.SetFieldValue<SkinnedMeshRenderer>("mainSkinnedMeshRenderer", mainRenderer);
                        }
                    }
                }
                return mainRenderer;
            }

            void CreateSkinInfo(SkinnedMeshRenderer mainRenderer)
            {
                ModelSkinController skinController = model.AddOrGetComponent<ModelSkinController>();

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
                defaultSkinInfo.UnlockableName = "";

                Material commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/BrotherGlassBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

                CharacterModel.RendererInfo[] rendererInfos = defaultSkinInfo.RendererInfos;
                CharacterModel.RendererInfo[] array = new CharacterModel.RendererInfo[rendererInfos.Length];
                rendererInfos.CopyTo(array, 0);

                array[0].defaultMaterial = commandoMat;

                LoadoutAPI.SkinDefInfo dave = default(LoadoutAPI.SkinDefInfo);
                dave.BaseSkins = Array.Empty<SkinDef>();
                dave.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
                dave.Icon = LoadoutAPI.CreateSkinIcon(new Color(0f, 156f / 255f, 188f / 255f), new Color(186f / 255f, 128f / 255f, 52f / 255f), new Color(58f / 255f, 49f / 255f, 24f / 255f), new Color(2f / 255f, 29f / 255f, 55f / 255f));

                dave.MeshReplacements = new SkinDef.MeshReplacement[] {
                    new SkinDef.MeshReplacement {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                    }
                };
                dave.MinionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>();
                dave.Name = "WOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOW";
                dave.NameToken = "Glass";
                dave.ProjectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>();
                dave.RendererInfos = array;
                dave.RootObject = model;
                dave.UnlockableName = "";


                //, ));

                SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(defaultSkinInfo);
                SkinDef daveSkin = LoadoutAPI.CreateNewSkinDef(dave);

                skinController.skins = new SkinDef[2]
                {
                defaultSkin,
                daveSkin
                };
            }

            CreateSkinInfo(FixRenderInfo());
        }
     
        //Credits to Enigma
        private void RepairSwingEffect()
        {
            GameObject HANDSwingTrail = Resources.Load<GameObject>("prefabs/effects/handslamtrail");
            Transform HANDSwingTrailTransform = HANDSwingTrail.transform.Find("SlamTrail");

            var HANDrenderer = HANDSwingTrailTransform.GetComponent<Renderer>();

            if (HANDrenderer)
            {
                HANDrenderer.material = Resources.Load<GameObject>("prefabs/effects/LemurianBiteTrail").transform.Find("SwingTrail").GetComponent<Renderer>().material;
            }
        }
        #endregion
        private void CreateMaster()
        {
            HANDMonsterMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/charactermasters/commandomonstermaster"), "HANDOverclockedMonsterMaster", true);
            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(HANDMonsterMaster);
            };

            CharacterMaster cm = HANDMonsterMaster.GetComponent<CharacterMaster>();
            cm.bodyPrefab = HANDBody;

            Component[] toDelete = HANDMonsterMaster.GetComponents<AISkillDriver>();
            foreach (AISkillDriver asd in toDelete)
            {
                Destroy(asd);
            }

            AISkillDriver special = HANDMonsterMaster.AddComponent<AISkillDriver>();
            special.skillSlot = SkillSlot.Special;
            special.requireSkillReady = true;
            special.requireEquipmentReady = false;
            special.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            special.minDistance = 0f;
            special.maxDistance = float.PositiveInfinity;
            special.selectionRequiresTargetLoS = false;
            special.activationRequiresTargetLoS = false;
            special.activationRequiresAimConfirmation = false;
            special.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            special.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            special.ignoreNodeGraph = false;
            special.driverUpdateTimerOverride = 0.1f;
            special.noRepeat = false;
            special.shouldSprint = true;
            special.shouldFireEquipment = false;
            special.shouldTapButton = false;
            special.maxUserHealthFraction = 0.6f;

            AISkillDriver utility = HANDMonsterMaster.AddComponent<AISkillDriver>();
            utility.skillSlot = SkillSlot.Utility;
            utility.requireSkillReady = true;
            utility.requireEquipmentReady = false;
            utility.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            utility.minDistance = 0f;
            utility.maxDistance = 20f;
            utility.selectionRequiresTargetLoS = false;
            utility.activationRequiresTargetLoS = false;
            utility.activationRequiresAimConfirmation = false;
            utility.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            utility.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            utility.ignoreNodeGraph = false;
            utility.driverUpdateTimerOverride = 0f;
            utility.noRepeat = true;
            utility.shouldSprint = true;
            utility.shouldFireEquipment = false;
            utility.shouldTapButton = false;

            AISkillDriver secondary = HANDMonsterMaster.AddComponent<AISkillDriver>();
            secondary.skillSlot = SkillSlot.Secondary;
            secondary.requireSkillReady = true;
            secondary.requireEquipmentReady = false;
            secondary.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            secondary.minDistance = 0f;
            secondary.maxDistance = 18f;
            secondary.selectionRequiresTargetLoS = true;
            secondary.activationRequiresTargetLoS = false;
            secondary.activationRequiresAimConfirmation = false;
            secondary.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            secondary.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            secondary.ignoreNodeGraph = false;
            secondary.driverUpdateTimerOverride = 2f;
            secondary.noRepeat = true;
            secondary.shouldSprint = true;
            secondary.shouldFireEquipment = false;
            secondary.shouldTapButton = false;

            AISkillDriver primary = HANDMonsterMaster.AddComponent<AISkillDriver>();
            primary.skillSlot = SkillSlot.Primary;
            primary.requireSkillReady = false;
            primary.requireEquipmentReady = false;
            primary.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            primary.minDistance = 0f;
            primary.maxDistance = 15f;
            primary.selectionRequiresTargetLoS = true;
            primary.activationRequiresTargetLoS = false;
            primary.activationRequiresAimConfirmation = false;
            primary.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            primary.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            primary.ignoreNodeGraph = false;
            primary.driverUpdateTimerOverride = 0.6f;
            primary.noRepeat = false;
            primary.shouldSprint = true;
            primary.shouldFireEquipment = false;
            primary.shouldTapButton = false;

            AISkillDriver chase = HANDMonsterMaster.AddComponent<AISkillDriver>();
            chase.skillSlot = SkillSlot.None;
            chase.requireSkillReady = false;
            chase.requireEquipmentReady = false;
            chase.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chase.minDistance = 0f;
            chase.maxDistance = float.PositiveInfinity;
            chase.selectionRequiresTargetLoS = false;
            chase.activationRequiresTargetLoS = false;
            chase.activationRequiresAimConfirmation = false;
            chase.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chase.aimType = AISkillDriver.AimType.AtMoveTarget;
            chase.ignoreNodeGraph = false;
            chase.driverUpdateTimerOverride = -1f;
            chase.noRepeat = false;
            chase.shouldSprint = true;
            chase.shouldFireEquipment = false;
            chase.shouldTapButton = false;

            AISkillDriver afk = HANDMonsterMaster.AddComponent<AISkillDriver>();
            afk.skillSlot = SkillSlot.None;
            afk.requireSkillReady = false;
            afk.requireEquipmentReady = false;
            afk.moveTargetType = AISkillDriver.TargetType.NearestFriendlyInSkillRange;
            afk.minDistance = 0f;
            afk.maxDistance = float.PositiveInfinity;
            afk.selectionRequiresTargetLoS = false;
            afk.activationRequiresTargetLoS = false;
            afk.activationRequiresAimConfirmation = false;
            afk.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            afk.aimType = AISkillDriver.AimType.MoveDirection;
            afk.ignoreNodeGraph = false;
            afk.driverUpdateTimerOverride = -1f;
            afk.noRepeat = false;
            afk.shouldSprint = true;
            afk.shouldFireEquipment = false;
            afk.shouldTapButton = false;
        }

        private class MenuAnimComponent : MonoBehaviour
        {
            internal void OnEnable()
            {
                if (base.gameObject && base.transform.parent && base.gameObject.transform.parent.gameObject && base.gameObject.transform.parent.gameObject.name == "CharacterPad")
                {
                    base.StartCoroutine(this.SelectSound());
                }
            }

            private IEnumerator SelectSound()
            {
                Util.PlaySound("play_drone_repair", base.gameObject);

                Animator modelAnimator = base.gameObject.GetComponent<Animator>();
                int layerIndex = modelAnimator.GetLayerIndex("Gesture");
                modelAnimator.SetFloat("FullSwing.playbackRate", 1f);
                modelAnimator.CrossFadeInFixedTime("FullSwing1", 0.2f, layerIndex);
                modelAnimator.Update(0f);
                float length = modelAnimator.GetNextAnimatorStateInfo(layerIndex).length;
                modelAnimator.SetFloat("FullSwing.playbackRate", length / 1.5f);
                yield return new WaitForSeconds(0.4f);
                Util.PlaySound("play_loader_m1_swing", base.gameObject);
                yield return new WaitForSeconds(0.4f);
                modelAnimator.CrossFadeInFixedTime("FullSwing2", 0.2f, layerIndex);
                yield return new WaitForSeconds(0.4f);
                Util.PlaySound("play_loader_m1_swing", base.gameObject);
                yield break;
            }
        }

        /*private void MutilateBody() //the most bootleg way of getting punch han-d back
        {
            Component[] snComponents = HANDBody.GetComponentsInChildren<Transform>();
            foreach (Transform t in snComponents)
            {
                //Debug.Log(t.name);
                if (t.name == "HANDHammerMesh")
                {
                    //HANDController.hammer = t;
                    break;
                }
            }
            //HANDController.hammer.localScale = Vector3.zero;
            rightGun.localScale = new Vector3(5f, 1.8f, 1.2f);
            leftGun.localScale = new Vector3(0, 0, 0);
            CharacterModel snCM = null;
            snCM = SniperBody.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>();
            CharacterModel.RendererInfo[] baseRendererInfos = snCM.baseRendererInfos;
            
        }*/
    }
}
