using BepInEx;
using EntityStates;
using EntityStates.HANDOverclocked;
using HAND_OVERCLOCKED.Components;
using RoR2.Projectile;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using HAND_OVERCLOCKED.Components.DroneProjectile;
using Mono.Cecil;
using R2API;
using R2API.Utils;
using R2API.Networking;
using RoR2.ContentManagement;

namespace HAND_OVERCLOCKED
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.HAND_Overclocked", "HAN-D OVERCLOCKED BETA", "0.1.0")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(LanguageAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(SoundAPI), nameof(NetworkingAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    class HAND_OVERCLOCKED : BaseUnityPlugin
    {
        public static GameObject HANDBody = null;
        GameObject HANDMonsterMaster = null;
        Color HANDColor = new Color(0.556862745f, 0.682352941f, 0.690196078f);
        Color OverclockColor = new Color(1.0f, 0.45f, 0f);

        String HANDBodyName = "";

        public CharacterBody body;
        public SkillLocator skillLocator;

        public static GameObject slamEffect;

        private readonly Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");

        private void RegisterLanguageTokens()
        {
            string HANDDesc = "";
            HANDDesc += "HAN-D is a robot janitor whose powerful melee attacks are sure to leave a mess!<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > HURT has increased knockback against airborne enemies. Use FORCED_REASSEMBLY to pop enemies in the air, then HURT them to send them flying!" + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > FORCED_REASSEMBLY's self-knockback can be used to reach flying enemies." + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > OVERCLOCK lasts as long as you can keep hitting enemies." + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > Use DRONES to heal and stay in the fight." + Environment.NewLine + Environment.NewLine;
            LanguageAPI.Add("HAND_OVERCLOCKED_DESC", HANDDesc);

            LanguageAPI.Add("HAND_OVERCLOCKED_SUBTITLE", "Lean, Mean, Cleaning Machine");

            LanguageAPI.Add("KEYWORD_HANDOVERCLOCKED_SPRINGY", "<style=cKeywordName>Springy</style><style=cSub>Spring upwards when using this skill.</style>");
            LanguageAPI.Add("KEYWORD_HANDOVERCLOCKED_DEBILITATE", "<style=cKeywordName>Debilitate</style><style=cSub>Reduce damage by <style=cIsDamage>30%</style>. Reduce movement speed by <style=cIsDamage>60%</style>.</style>");
            LanguageAPI.Add("KEYWORD_HANDOVERCLOCKED_ENERGIZE", "<style=cKeywordName>Energize</style><style=cSub>Increase movement speed and attack speed by <style=cIsDamage>40%</style>.</style>");

            LanguageAPI.Add("HAND_OVERCLOCKED_NAME", "HAN-D");
            LanguageAPI.Add("HAND_OVERCLOCKED_OUTRO_FLAVOR", "..and so it left, servos pulsing with new life.");
            LanguageAPI.Add("HAND_OVERCLOCKED_MAIN_ENDING_ESCAPE_FAILURE_FLAVOR", "..and so it vanished, unrewarded in all of its efforts.");

            String tldr = "<style=cMono>\r\n//--AUTO-TRANSCRIPTION FROM BASED DEPARTMENT OF UES SAFE TRAVELS--//</style>\r\n\r\n<i>*hits <color=#327FFF>Spinel Tonic</color>*</i>\n\nIs playing without the <color=#6955A6>Command</color> artifact the ultimate form of cuckoldry?\n\nI cannot think or comprehend of anything more cucked than playing without <color=#6955A6>Command</color>. Honestly, think about it rationally. You are shooting, running, jumping for like 60 minutes solely so you can get a fucking <color=#77FF16>Squid Polyp</color>. All that hard work you put into your run - dodging <style=cIsHealth>Stone Golem</style> lasers, getting annoyed by six thousand <style=cIsHealth>Lesser Wisps</color> spawning above your head, activating <color=#E5C962>Shrines of the Mountain</color> all for one simple result: your inventory is filled up with <color=#FFFFFF>Warbanners</color> and <color=#FFFFFF>Monster Tooth</color> necklaces which cost money.\n\nOn a god run? Great. A bunch of shitty items which add nothing to your run end up coming out of the <color=#E5C962>Chests</color> you buy. They get the benefit of your hard earned dosh that came from killing <style=cIsHealth>Lemurians</style>.\n\nAs a man who plays this game you are <style=cIsHealth>LITERALLY</style> dedicating two hours of your life to opening boxes and praying it's not another <color=#77FF16>Chronobauble</color>. It's the ultimate and final cuck. Think about it logically.\r\n<style=cMono>\r\nTranscriptions complete.\r\n</style>\r\n \r\n\r\n";
            LanguageAPI.Add("HAND_OVERCLOCKED_LORE", tldr);

            LanguageAPI.Add("HAND_OVERCLOCKED_PRIMARY_NAME", "HURT");
            LanguageAPI.Add("HAND_OVERCLOCKED_SECONDARY_NAME", "FORCED_REASSEMBLY");
            LanguageAPI.Add("HAND_OVERCLOCKED_UTILITY_NAME", "OVERCLOCK");
            LanguageAPI.Add("HAND_OVERCLOCKED_SPECIAL_NAME", "DRONE");

            LanguageAPI.Add("HAND_OVERCLOCKED_PASSIVE_NAME", "PARALLEL_COMPUTING");
            LanguageAPI.Add("HAND_OVERCLOCKED_PASSIVE_DESC", "Gain <style=cIsDamage>+2.5% damage</style> and <style=cIsHealing>+1 armor</style> for every <style=cIsUtility>drone ally on your team</style>.");
        }

        private void CreateSurvivorDef() {
            GameObject HANDDisplay = HANDBody.GetComponent<ModelLocator>().modelTransform.gameObject;
            HANDDisplay.AddComponent<MenuAnimComponent>();
            SurvivorDef item = ScriptableObject.CreateInstance<SurvivorDef>();
            item.bodyPrefab = HANDBody;
            item.displayPrefab = HANDDisplay;
            item.descriptionToken = "HAND_OVERCLOCKED_DESC";
            item.outroFlavorToken = "HAND_OVERCLOCKED_OUTRO_FLAVOR";
            item.desiredSortPosition = 100f;
            HANDContent.survivorDefs.Add(item);
        }


        public void Awake()
        {
            Debug.Log("\n\nSTATUS UPDATE:\n\nMACHINE ID:\t\tHAN-D\nLOCATION:\t\tAPPROACHING PETRICHOR V\nCURRENT OBJECTIVE:\tFIND AND ACTIVATE THE TELEPORTER\n\nPROVIDENCE IS DEAD.\nBLOOD IS FUEL.\nSPEED IS WAR.\n");
            CreateHAND();
            CreateBuffs();
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(RoR2.ContentManagement.ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new HANDContent());
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
            if (self)
            {
                float origDamage = self.damage;
                float origAtkSpd = self.attackSpeed;
                float origMoveSpd = self.moveSpeed;
                if (self.HasBuff(HANDContent.ParallelComputingBuff))
                {
                    int pcCount = self.GetBuffCount(HANDContent.ParallelComputingBuff);
                    self.damage += origDamage * pcCount * 0.025f;
                    self.armor += pcCount;
                }
                if (self.HasBuff(HANDContent.OverclockBuff))
                {
                    self.attackSpeed += origAtkSpd * 0.4f;
                    self.moveSpeed += origMoveSpd * 0.4f;
                }
                if (self.HasBuff(HANDContent.DroneBuff))
                {
                    self.attackSpeed += origAtkSpd * 0.4f;
                    self.moveSpeed += origMoveSpd * 0.4f;
                }
                if (self.HasBuff(HANDContent.DroneDebuff))
                {
                    self.moveSpeed *= 0.4f;
                    self.damage *= 0.7f;
                }
            }
        }

        private void CharacterModel_EnableItemDisplay(On.RoR2.CharacterModel.orig_EnableItemDisplay orig, CharacterModel self, ItemIndex itemIndex)
        {
            if ((itemIndex != RoR2Content.Items.Bear.itemIndex) || self.name != "mdlHAND")
            {
                orig(self, itemIndex);
            }
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport report)
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

                if (report.attacker == report.damageInfo.inflictor)
                {
                    if ((report.damageInfo.damageType & DamageType.Stun1s) > 0 && (report.damageInfo.damageType & DamageType.AOE) == 0 && report.damageInfo.procCoefficient >= 1f && report.damageInfo.damage > report.attackerBody.damage * 4f)
                    {
                        if (report.victimBody)
                        {
                            if (report.victimBody.modelLocator && report.victimBody.modelLocator.modelTransform && report.victimBody.modelLocator.modelTransform.gameObject && !report.victimBody.modelLocator.modelTransform.gameObject.GetComponent<SquashedComponent>())
                            {
                                report.victimBody.modelLocator.modelTransform.gameObject.AddComponent<SquashedComponent>().speed = 5f;
                            }
                        }
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
            Repair();
            SetAttributes();

            CreateMaster();
            CreateSurvivorDef();

            RegisterLanguageTokens();
            //and lastly
            Hook();
        }

        private void CreateSlamEffect()
        {
            slamEffect = Resources.Load<GameObject>("prefabs/effects/impacteffects/ParentSlamEffect").InstantiateClone("HANDOVerclockedSlamImpactEffect", false);

            var particleParent = slamEffect.transform.Find("Particles");
            var debris = particleParent.Find("Debris, 3D");
            var debris2 = particleParent.Find("Debris");
            var sphere = particleParent.Find("Nova Sphere");

            debris.gameObject.SetActive(false);
            debris2.gameObject.SetActive(false);
            sphere.gameObject.SetActive(false);

            ShakeEmitter se = slamEffect.AddComponent<ShakeEmitter>();
            se.shakeOnStart = true;
            se.duration = 0.65f;
            se.scaleShakeRadiusWithLocalScale = false;
            se.radius = 30f;
            se.wave = new Wave()
            {
                amplitude = 7f,
                cycleOffset = 0f,
                frequency = 6f
            };

            slamEffect.GetComponent<EffectComponent>().soundName = "";
            //Play_parent_attack1_slam

            HANDContent.effectDefs.Add(new EffectDef(slamEffect));
        }

        private void Repair() {
            RepairSwingEffect();
            AddSkin();
        }

        private void LoadAssets()
        {
            if (HANDContent.assets == null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HAN_D_OVERCLOCKED.handassets"))
                {
                    HANDContent.assets = AssetBundle.LoadFromStream(stream);
                }

                using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HAN_D_OVERCLOCKED.HAND_Overclocked_Soundbank.bnk"))
                {
                    var bytes = new byte[bankStream.Length];
                    bankStream.Read(bytes, 0, bytes.Length);
                    SoundAPI.SoundBanks.Add(bytes);
                }
            }
        }

        private void SetBody()
        {
            if (HANDBody == null)
            {
                HANDBody = Resources.Load<GameObject>("prefabs/characterbodies/handbody").InstantiateClone("HANDOverclockedBody", true);
                HANDBodyName = HANDBody.name;
                LoadAssets();
                HANDBody.GetComponent<CharacterBody>().portraitIcon = HANDContent.assets.LoadAsset<Texture2D>("Portrait.png");
                HANDContent.bodyPrefabs.Add(HANDBody);
            }
        }

        private void SetAttributes()
        {
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
                HANDBody.AddComponent<HANDNetworkSounds>();
                HANDBody.AddComponent<DroneFollowerController>();
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

            void SetCameraAttributes() {
                CameraTargetParams cameraTargetParams = HANDBody.GetComponent<CameraTargetParams>();
                cameraTargetParams.idealLocalCameraPos = new Vector3(0f, 0f, -4.7f);
                cameraTargetParams.cameraParams = Resources.Load<GameObject>("prefabs/characterbodies/toolbotbody").GetComponent<CameraTargetParams>().cameraParams;
            }

            void SetCharacterBodyAttributes() {
                body = HANDBody.GetComponent<CharacterBody>();
                body.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes | CharacterBody.BodyFlags.Mechanical;
                body.subtitleNameToken = "HAND_OVERCLOCKED_SUBTITLE";
                body.crosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/simpledotcrosshair");
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
                body.baseDamage = 12f;
                body.baseAttackSpeed = 1f;
                body.baseCrit = 1f;
                body.baseArmor = 12f;
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

            HANDBody.GetComponent<ModelLocator>().modelTransform.localScale *= 1.2f;

            CreateSkills();
        }

        #region Skills
        private void CreateSkills() {
            skillLocator = HANDBody.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = "HAND_OVERCLOCKED_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = "HAND_OVERCLOCKED_PASSIVE_DESC";
            skillLocator.passiveSkill.icon = HANDContent.assets.LoadAsset<Sprite>("Drone2.png");

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
            FullSwing.recoilAmplitude = 6f;

            HANDContent.entityStates.Add(typeof(FullSwing));

            SkillDef primarySkill = SkillDef.CreateInstance<SkillDef>();
            primarySkill.activationState = new SerializableEntityStateType(typeof(FullSwing));
            primarySkill.skillNameToken = "HAND_OVERCLOCKED_PRIMARY_NAME";
            primarySkill.skillName = "FullSwing";
            primarySkill.skillDescriptionToken = "Swing your hammer in a wide arc, hurting enemies for <style=cIsDamage>" + FullSwing.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>.";
            primarySkill.cancelSprintingOnActivation = true;
            primarySkill.canceledFromSprinting = false;
            primarySkill.baseRechargeInterval = 0f;
            primarySkill.baseMaxStock = 1;
            primarySkill.rechargeStock = 1;
            primarySkill.beginSkillCooldownOnSkillEnd = false;
            primarySkill.activationStateMachineName = "Weapon";
            primarySkill.interruptPriority = EntityStates.InterruptPriority.Any;
            primarySkill.isCombatSkill = true;
            primarySkill.mustKeyPress = false;
            primarySkill.icon = HANDContent.assets.LoadAsset<Sprite>("Hurt.png");
            primarySkill.requiredStock = 1;
            primarySkill.stockToConsume = 1;
            primarySkill.keywordTokens = new string[] {};
            HANDContent.skillDefs.Add(primarySkill);

            SkillFamily primarySkillFamily = skillLocator.primary.skillFamily;

            primarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = primarySkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(primarySkill.skillNameToken, false, null)

            };
            HANDContent.skillFamilies.Add(primarySkillFamily);
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
            Slam2.airbornVerticalForceMax = -3200f;
            Slam2.shorthopVelocityFromHit = 24f;
            Slam2.hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactToolbotDashLarge");
            Slam2.impactEffectPrefab = slamEffect;//Resources.Load<GameObject>("prefabs/effects/impacteffects/PodGroundImpact");
            Slam2.swingEffectPrefab = Resources.Load<GameObject>("prefabs/effects/handslamtrail");
            Slam2.returnToIdlePercentage = 0.443662f;
            Slam2.minRange = 9;
            Slam2.maxRange = 22;
            HANDContent.entityStates.Add(typeof(ChargeSlam2));
            HANDContent.entityStates.Add(typeof(Slam2));

            SkillDef secondarySkill = SkillDef.CreateInstance<SkillDef>();
            secondarySkill.activationState = new SerializableEntityStateType(typeof(ChargeSlam2));
            secondarySkill.skillNameToken = "HAND_OVERCLOCKED_SECONDARY_NAME";
            secondarySkill.skillName = "ChargeSlam";
            secondarySkill.skillDescriptionToken = "<style=cIsUtility>Springy</style>. Charge up a powerful hammer slam for <style=cIsDamage>400%-1200% damage</style>. <style=cIsDamage>Range and knockback</style> increases with charge.";
            secondarySkill.cancelSprintingOnActivation = true;
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
            secondarySkill.icon = HANDContent.assets.LoadAsset<Sprite>("Forced_Reassembly2.png");
            secondarySkill.beginSkillCooldownOnSkillEnd = true;
            secondarySkill.keywordTokens = new string[] { "KEYWORD_STUNNING", "KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            HANDContent.skillDefs.Add(secondarySkill);

            SkillFamily secondarySkillFamily = skillLocator.secondary.skillFamily;

            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = secondarySkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(secondarySkill.skillNameToken, false, null)

            };
            HANDContent.skillFamilies.Add(secondarySkillFamily);
        }

        private void CreateUtility()
        {
            HANDContent.entityStates.Add(typeof(Overclock));

            EntityStateMachine stateMachine = HANDBody.AddComponent<EntityStateMachine>();
            stateMachine.customName = "Overclock";
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));

            SkillDef ovcSkill = SkillDef.CreateInstance<SkillDef>();
            ovcSkill.activationState = new SerializableEntityStateType(typeof(Overclock));
            ovcSkill.skillNameToken = "HAND_OVERCLOCKED_UTILITY_NAME";
            ovcSkill.skillName = "Overclock";
            ovcSkill.skillDescriptionToken = "<style=cIsUtility>Springy</style>. Increase <style=cIsUtility>movement speed</style> and <style=cIsDamage>attack speed</style> by <style=cIsDamage>40%</style>, and gain <style=cIsDamage>50% stun chance</style>. <style=cIsUtility>Hit enemies to increase duration</style>.";
            ovcSkill.isCombatSkill = false;
            ovcSkill.cancelSprintingOnActivation = false;
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
            ovcSkill.icon = HANDContent.assets.LoadAsset<Sprite>("Overclock.png");
            ovcSkill.activationStateMachineName = "Overclock";
            ovcSkill.keywordTokens = new string[] { "KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            HANDContent.skillDefs.Add(ovcSkill);

            SkillFamily utilitySkillFamily = skillLocator.utility.skillFamily;

            utilitySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = ovcSkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(ovcSkill.skillNameToken, false, null)
            };
            HANDContent.skillFamilies.Add(utilitySkillFamily);

            OverclockController.texGauge = HANDContent.assets.LoadAsset<Texture2D>("gauge_bar_hd3.png");
            OverclockController.texGaugeArrow = HANDContent.assets.LoadAsset<Texture2D>("gauge_bar_arrow_hd.png");
            OverclockController.overclockCancelIcon = HANDContent.assets.LoadAsset<Sprite>("Overclock_Cancel.png");
            OverclockController.overclockIcon = HANDContent.assets.LoadAsset<Sprite>("Overclock.png");
            OverclockController.ovcDef = ovcSkill;
        }

        private void CreateSpecial() {
            FireSeekingDrone.damageCoefficient = 2.7f;
            FireSeekingDrone.force = 250f;
            FireSeekingDrone.projectilePrefab = CreateDroneProjectile();
            FireSeekingDrone.baseDuration = 0.25f;

            CreateDroneFollower();

            EntityStateMachine stateMachine = HANDBody.AddComponent<EntityStateMachine>();
            stateMachine.customName = "DroneLauncher";
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));

            SkillDef droneSkill = SkillDef.CreateInstance<SkillDef>();
            droneSkill.activationState = new SerializableEntityStateType(typeof(FireSeekingDrone));
            droneSkill.skillNameToken = "HAND_OVERCLOCKED_SPECIAL_NAME";
            droneSkill.skillName = "Drones";
            droneSkill.skillDescriptionToken = "<style=cIsHealing>Heal 8.5% HP</style>. Fire a drone that <style=cIsDamage>Debilitates</style> enemies for <style=cIsDamage>"
                + FireSeekingDrone.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style> and <style=cIsUtility>Energizes</style> allies.";
            droneSkill.skillDescriptionToken += " <style=cIsUtility>Kills and melee hits reduce cooldown</style>.";
            droneSkill.isCombatSkill = true;
            droneSkill.cancelSprintingOnActivation = false;
            droneSkill.canceledFromSprinting = false;
            droneSkill.baseRechargeInterval = 10f;  
            droneSkill.interruptPriority = EntityStates.InterruptPriority.Any;
            droneSkill.mustKeyPress = false;
            droneSkill.beginSkillCooldownOnSkillEnd = true;
            droneSkill.baseMaxStock = 10;
            droneSkill.fullRestockOnAssign = false;
            droneSkill.rechargeStock = 1;
            droneSkill.requiredStock = 1;
            droneSkill.stockToConsume = 1;
            droneSkill.icon = HANDContent.assets.LoadAsset<Sprite>("Drone2.png");
            droneSkill.activationStateMachineName = "DroneLauncher";
            droneSkill.keywordTokens = new string[] { "KEYWORD_HANDOVERCLOCKED_DEBILITATE", "KEYWORD_HANDOVERCLOCKED_ENERGIZE" };
            HANDContent.entityStates.Add(typeof(FireSeekingDrone));

            SkillFamily specialSkillFamily = skillLocator.special.skillFamily;
            HANDContent.skillDefs.Add(droneSkill);

            specialSkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = droneSkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(droneSkill.skillNameToken, false, null)
            };
            HANDContent.skillFamilies.Add(specialSkillFamily);
        }
        #endregion

        private void CreateBuffs()
        {
            BuffDef ParallelBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            ParallelBuffDef.buffColor = HANDColor;
            ParallelBuffDef.canStack = true;
            ParallelBuffDef.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffOverheat");
            ParallelBuffDef.isDebuff = false;
            ParallelBuffDef.name = "MoffeinHANDParallelComputing";
            HANDContent.buffDefs.Add(ParallelBuffDef);
            HANDContent.ParallelComputingBuff = ParallelBuffDef;

            BuffDef OverclockBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            OverclockBuffDef.buffColor = OverclockColor;
            OverclockBuffDef.canStack = false;
            OverclockBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffTeslaIcon");
            OverclockBuffDef.isDebuff = false;
            OverclockBuffDef.name = "MoffeinHANDOverclock";
            HANDContent.buffDefs.Add(OverclockBuffDef);
            HANDContent.OverclockBuff = OverclockBuffDef;

            BuffDef DroneBoostDef = ScriptableObject.CreateInstance<BuffDef>();
            DroneBoostDef.buffColor = OverclockColor;
            DroneBoostDef.canStack = false;
            DroneBoostDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texWarcryBuffIcon");
            DroneBoostDef.isDebuff = false;
            DroneBoostDef.name = "MoffeinHANDDroneBoost";
            HANDContent.buffDefs.Add(DroneBoostDef);
            HANDContent.DroneBuff = DroneBoostDef;

            BuffDef DroneDebuffDef = ScriptableObject.CreateInstance<BuffDef>();
            DroneDebuffDef.buffColor = HANDColor;
            DroneDebuffDef.canStack = false;
            DroneDebuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffWeakIcon");
            DroneDebuffDef.isDebuff = true;
            DroneDebuffDef.name = "MoffeinHANDDroneDebuff";
            HANDContent.buffDefs.Add(DroneDebuffDef);
            HANDContent.DroneDebuff = DroneDebuffDef;
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

                /*Material commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/BrotherGlassBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

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
                dave.UnlockableName = "";*/


                //, ));

                SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(defaultSkinInfo);
                //SkinDef daveSkin = LoadoutAPI.CreateNewSkinDef(dave);

                skinController.skins = new SkinDef[1]
                {
                defaultSkin
                //daveSkin
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
            HANDContent.masterPrefabs.Add(HANDMonsterMaster);

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
                Util.PlaySound("Play_HOC_StartPunch", base.gameObject);

                Animator modelAnimator = base.gameObject.GetComponent<Animator>();
                int layerIndex = modelAnimator.GetLayerIndex("Gesture");
                modelAnimator.SetFloat("FullSwing.playbackRate", 1f);
                modelAnimator.CrossFadeInFixedTime("FullSwing1", 0.2f, layerIndex);
                modelAnimator.Update(0f);
                float length = modelAnimator.GetNextAnimatorStateInfo(layerIndex).length;
                modelAnimator.SetFloat("FullSwing.playbackRate", length / 1.5f);
                yield return new WaitForSeconds(0.4f);
                Util.PlaySound("Play_HOC_Punch", base.gameObject);
                yield return new WaitForSeconds(0.4f);
                modelAnimator.CrossFadeInFixedTime("FullSwing2", 0.2f, layerIndex);
                yield return new WaitForSeconds(0.4f);
                Util.PlaySound("Play_HOC_Punch", base.gameObject);
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

        private void CreateDroneFollower()
        {
            GameObject droneFollower = PrefabAPI.InstantiateClone(HANDContent.assets.LoadAsset<GameObject>("DronePrefab.prefab"), "HANDOverclockedDroneFollower", false);
            droneFollower.GetComponentInChildren<MeshRenderer>().material.shader = hotpoo;
            droneFollower.transform.localScale = 2f * Vector3.one;

            droneFollower.layer = LayerIndex.noCollision.intVal;
            Destroy(droneFollower.GetComponentInChildren<ParticleSystem>());
            Destroy(droneFollower.GetComponentInChildren<Collider>());

            DroneFollowerController.dronePrefab = droneFollower;

            /*GameObject droneSpawnEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactLoaderFistSmall"), "HANDOverclockedDroneSpawnEffect", false);
            Destroy(droneSpawnEffect.GetComponent<ShakeEmitter>());
            droneSpawnEffect.GetComponent<EffectComponent>().soundName = "";
            HANDContent.effectDefs.Add(new EffectDef(droneSpawnEffect));*/
            DroneFollowerController.activateEffect = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");

            /*GameObject droneFireEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/effects/muzzleflashes/MuzzleflashEngiGrenade"), "HANDOverclockedDroneFireEffect", false);
            droneFireEffect.GetComponent<EffectComponent>().soundName = "";
            HANDContent.effectDefs.Add(new EffectDef(droneFireEffect));*/
            DroneFollowerController.deactivateEffect = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");
        }

        private GameObject CreateDroneProjectile()
        {
            GameObject droneProjectile = Resources.Load<GameObject>("prefabs/projectiles/EngiHarpoon").InstantiateClone("HANDOverclockedDroneProjectile", true);

            GameObject droneProjectileGhost = PrefabAPI.InstantiateClone(HANDContent.assets.LoadAsset<GameObject>("DronePrefab.prefab"),"HANDOverclockedDroneProjectileGhost", false);
            droneProjectileGhost.GetComponentInChildren<MeshRenderer>().material.shader = hotpoo;
            droneProjectileGhost.AddComponent<ProjectileGhostController>();
            droneProjectileGhost.transform.localScale = 2f * Vector3.one;

            droneProjectileGhost.layer = LayerIndex.noCollision.intVal;
            Destroy(droneProjectileGhost.GetComponentInChildren<Collider>());

            droneProjectile.GetComponent<ProjectileController>().ghostPrefab = droneProjectileGhost;

            HANDContent.projectilePrefabs.Add(droneProjectile);

            MissileController mc = droneProjectile.GetComponent<MissileController>();
            mc.maxVelocity = 25f;
            mc.acceleration = 3f;
            mc.maxSeekDistance = 160f;
            mc.giveupTimer = 20f;
            mc.deathTimer = 20f;

            Destroy(droneProjectile.GetComponent<AkGameObj>());
            Destroy(droneProjectile.GetComponent<AkEvent>());
            Destroy(droneProjectile.GetComponent<ProjectileSingleTargetImpact>());

            ProjectileStickOnImpact stick = droneProjectile.AddComponent<ProjectileStickOnImpact>();
            stick.ignoreWorld = true;
            stick.ignoreCharacters = false;
            stick.alignNormals = false;

            droneProjectile.AddComponent<DroneProjectileDamageController>();
            droneProjectile.AddComponent<PreventGroundCollision>();

            Destroy(droneProjectile.GetComponent<BoxCollider>());
            SphereCollider sc = droneProjectile.AddComponent<SphereCollider>();
            sc.radius = 0.6f;
            sc.contactOffset = 0.01f;

            //droneProjectile.layer = LayerIndex.entityPrecise.intVal;

            //droneProjectile.AddComponent<DroneProjectileRotation>();

            /*GameObject droneProjectileGhost = Resources.Load<GameObject>("path to drone model").InstantiateClone("HANDOverclockedDroneProjectileGhost", true);
            droneProjectileGhost.GetComponentInChildren<MeshRenderer>().material.shader = Resources.Load<Shader>("Shaders/Deferred/hgstandard");
            droneProjectileGhost.AddComponent<ProjectileGhostController>();
            droneProjectile.GetComponent<ProjectileController>().ghostPrefab = droneProjectileGhost;*/

            return droneProjectile;
        }
    }
}
