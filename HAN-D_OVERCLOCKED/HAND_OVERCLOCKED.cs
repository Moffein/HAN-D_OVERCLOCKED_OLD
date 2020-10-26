using BepInEx;
using EntityStates;
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
    [R2APISubmoduleDependency(nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(ResourcesAPI), nameof(BuffAPI), nameof(LanguageAPI), nameof(NetworkingAPI))]
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
        String HANDDesc = "";
        public static BuffIndex OverclockBuff;

        public static float overclockAtkSpd;
        public static float overclockSpd;

        const String assetPrefix = "@MoffeinHAND_OVERCLOCKED";
        const String portraitPath = assetPrefix + ":handprofile2.png";

        bool useBodyClone = true;

        public void Start()
        {
            SetAttributes();
            InitSkills();
            AssignSkills();
            //MutilateBody();
        }

        public void Awake()
        {
            Debug.Log("\n\nSTATUS UPDATE:\n\nMACHINE ID:\t\tHAN-D\nLOCATION:\t\tAPPROACHING PETRICHOR V\nCURRENT OBJECTIVE:\tFIND AND ACTIVATE THE TELEPORTER\n\nPROVIDENCE IS DEAD.\nBLOOD IS FUEL.\nSPEED IS WAR.\n");

            SetBody();

            //HANDBody.GetComponent<ModelLocator>().modelTransform.localScale *= 1.2f;

            RepairSwingEffect();
            AddSkin();
            CreateMaster();
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

            GameObject HANDDisplay = HANDBody.GetComponent<ModelLocator>().modelTransform.gameObject;
            HANDDisplay.AddComponent<MenuAnimComponent>();
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

            On.RoR2.CameraRigController.OnEnable += (orig, self) =>
            {
                if (RoR2.SceneCatalog.GetSceneDefForCurrentScene().baseSceneName.Equals("lobby"))
                {
                    self.enableFading = false;
                }
                orig(self);
            };

            On.RoR2.UI.LogBook.LogBookController.Init += (orig) =>
            {
                SetBody();
                orig();
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, report) =>
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
            };

            On.RoR2.CharacterModel.EnableItemDisplay += (orig, self, itemIndex) =>
            {
                if ((itemIndex != ItemIndex.Bear) || self.name != "mdlHAND")
                {
                    orig(self, itemIndex);
                }
            };

            On.RoR2.BuffCatalog.Init += (orig) =>
            {
                CreateOverclockBuff();
                orig();
            };

            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);
                if (self && self.HasBuff(OverclockBuff))
                {
                    int ovcCount = self.GetBuffCount(OverclockBuff);
                    self.SetPropertyValue<float>("attackSpeed", self.attackSpeed * (1 + overclockAtkSpd));
                    //self.SetPropertyValue<float>("armor", self.armor + overclockArmor);
                    self.SetPropertyValue<float>("moveSpeed", self.moveSpeed * (1 + overclockSpd));
                }
            };
        }

        /*private void UpdateOverclock(CharacterBody cb)
        {
            int ovcCount = cb.GetBuffCount(OverclockBuff);
            cb.ClearTimedBuffs(OverclockBuff);
            if (ovcCount < maxOverclock)
            {
                ovcCount++;
            }
            for (int i = 0; i < ovcCount; i++)
            {
                cb.AddTimedBuff(HAND_OVERCLOCKED.OverclockBuff, HAND_OVERCLOCKED.overclockBaseDecay + i * HAND_OVERCLOCKED.overclockDecay);
            }
        }*/

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

        private void SetIcon()
        {
            if (HANDIcon == null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HAN_D_OVERCLOCKED.hand_overclocked_portrait"))
                {
                    var bundle = AssetBundle.LoadFromStream(stream);
                    var provider = new AssetBundleResourcesProvider(assetPrefix, bundle);
                    ResourcesAPI.AddProvider(provider);
                }
                HANDIcon = Resources.Load<Texture2D>(portraitPath);
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
                SetIcon();
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
            HANDBody.AddComponent<HANDController>();
            SfxLocator sfx = HANDBody.GetComponent<SfxLocator>();
            sfx.landingSound = "play_char_land";
            sfx.fallDamageSound = "Play_MULT_shift_hit";

            HANDBody.tag = "Player";

            CharacterBody cb = HANDBody.GetComponent<CharacterBody>();
            cb.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            cb.subtitleNameToken = "Disposal Unit";
            cb.crosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/simpledotcrosshair");
            cb.hideCrosshair = false;

            CharacterDeathBehavior handCDB = cb.gameObject.GetComponent<CharacterDeathBehavior>();
            handCDB.deathState = Resources.Load<GameObject>("prefabs/characterbodies/WispBody").GetComponent<CharacterDeathBehavior>().deathState;

            cb.baseNameToken = "HAND_OVERCLOCKED_NAME"; 

            cb.baseMaxHealth = 150f;
            cb.baseRegen = 2.5f;
            cb.baseMaxShield = 0f;
            cb.baseMoveSpeed = 7f;
            cb.baseAcceleration = 30f;
            cb.baseJumpPower = 15f;
            cb.baseDamage = 12f;
            cb.baseAttackSpeed = 1f;
            cb.baseCrit = 1f;
            cb.baseArmor = 20f;
            cb.baseJumpCount = 1;

            cb.autoCalculateLevelStats = true;
            cb.levelMaxHealth = cb.baseMaxHealth * 0.3f;
            cb.levelRegen = cb.baseRegen * 0.2f;
            cb.levelMaxShield = 0f;
            cb.levelMoveSpeed = 0f;
            cb.levelJumpPower = 0f;
            cb.levelDamage = cb.baseDamage * 0.2f;
            cb.levelAttackSpeed = 0f;
            cb.levelCrit = 0f;
            cb.levelArmor = 0f;

            cb.spreadBloomDecayTime = 1f;

            cb.preferredPodPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/robocratepod");

            HANDBody.AddComponent<SetStateOnHurt>();
            SetStateOnHurt ssoh = HANDBody.GetComponent<SetStateOnHurt>();
            ssoh.canBeStunned = false;
            ssoh.canBeHitStunned = false;
            ssoh.canBeFrozen = true;
            ssoh.hitThreshold = 5;

            HANDBody.GetComponent<CameraTargetParams>().idealLocalCameraPos = new Vector3(0f, 0f, -4.7f);

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

        private void InitSkills()
        {
            //overclockArmor = 0f;
            overclockAtkSpd = 0.4f;
            overclockSpd = 0.3f;

            EntityStates.HANDOverclocked.FullSwing.damageCoefficient = 3.9f;
            EntityStates.HANDOverclocked.FullSwing.baseDuration = 1f;
            EntityStates.HANDOverclocked.FullSwing.airbornVerticalForce = 0f;
            EntityStates.HANDOverclocked.FullSwing.forceMagnitude = 1400f;
            EntityStates.HANDOverclocked.FullSwing.airbornHorizontalForceMult = 1.8f;
            EntityStates.HANDOverclocked.FullSwing.flyingHorizontalForceMult = 0.5f;
            EntityStates.HANDOverclocked.FullSwing.shorthopVelocityFromHit = 10f;
            EntityStates.HANDOverclocked.FullSwing.returnToIdlePercentage = 0.443662f;
            EntityStates.HANDOverclocked.FullSwing.swingEffectPrefab = Resources.Load<GameObject>("prefabs/effects/handslamtrail");
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.FullSwing));


            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.Overclock));

            EntityStates.HANDOverclocked.ChargeSlam2.baseMinDuration = 0.6f;
            EntityStates.HANDOverclocked.ChargeSlam2.baseChargeDuration = 1.4f;
            EntityStates.HANDOverclocked.Slam2.baseDuration = 0.6f;
            EntityStates.HANDOverclocked.Slam2.damageCoefficientMin = 4f;
            EntityStates.HANDOverclocked.Slam2.damageCoefficientMax = 12f;
            EntityStates.HANDOverclocked.Slam2.baseMinDuration = 0.4f;
            EntityStates.HANDOverclocked.Slam2.forceMagnitudeMin = 2000f;
            EntityStates.HANDOverclocked.Slam2.forceMagnitudeMax = 2000f;
            EntityStates.HANDOverclocked.Slam2.airbornVerticalForceMin = -2400f;
            EntityStates.HANDOverclocked.Slam2.airbornVerticalForceMax = -3000f;
            EntityStates.HANDOverclocked.Slam2.shorthopVelocityFromHit = 24f;
            EntityStates.HANDOverclocked.Slam2.hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactToolbotDashLarge");
            EntityStates.HANDOverclocked.Slam2.impactEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/PodGroundImpact");
            EntityStates.HANDOverclocked.Slam2.swingEffectPrefab = Resources.Load<GameObject>("prefabs/effects/handslamtrail");
            EntityStates.HANDOverclocked.Slam2.returnToIdlePercentage = 0.443662f;
            EntityStates.HANDOverclocked.Slam2.minRange = 9;
            EntityStates.HANDOverclocked.Slam2.maxRange = 22;
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.ChargeSlam2));
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.Slam2));


            EntityStates.HANDOverclocked.FireSeekingDrone.damageCoefficient = 2.7f;
            EntityStates.HANDOverclocked.FireSeekingDrone.projectilePrefab = Resources.Load<GameObject>("prefabs/projectiles/EngiHarpoon");
            EntityStates.HANDOverclocked.FireSeekingDrone.baseDuration = 0.25f;
            EntityStates.HANDOverclocked.FireSeekingDrone.healPercent = 0.1f;
            EntityStates.HANDOverclocked.FireSeekingDrone.effectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");
        }

        private void AssignSkills()
        {
            SkillLocator skillComponent = HANDBody.GetComponent<SkillLocator>();

            SkillDef primarySkill = SkillDef.CreateInstance<SkillDef>();
            primarySkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.FullSwing));
            primarySkill.skillNameToken = "HURT";
            primarySkill.skillName = "FullSwing";
            primarySkill.skillDescriptionToken = "<style=cIsUtility>Agile</style>. Swing your hammer in a wide arc, hurting enemies for <style=cIsDamage>" + EntityStates.HANDOverclocked.FullSwing.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>.";
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
            primarySkill.icon = skillComponent.primary.skillFamily.variants[0].skillDef.icon;
            primarySkill.requiredStock = 1;
            primarySkill.stockToConsume = 1;
            primarySkill.keywordTokens = new string[] { "KEYWORD_AGILE"};
            LoadoutAPI.AddSkillDef(primarySkill);

            SkillDef secondarySkill = SkillDef.CreateInstance<SkillDef>();
            secondarySkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.ChargeSlam2));
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
            secondarySkill.icon = skillComponent.special.skillFamily.variants[0].skillDef.icon;
            secondarySkill.isBullets = false;
            secondarySkill.shootDelay = 0.08f;
            secondarySkill.beginSkillCooldownOnSkillEnd = true;
            secondarySkill.keywordTokens = new string[] {"KEYWORD_STUNNING","KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            LoadoutAPI.AddSkillDef(secondarySkill);
            
            SkillDef droneSkill = SkillDef.CreateInstance<SkillDef>();
            droneSkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.FireSeekingDrone));
            droneSkill.skillNameToken = "DRONES";
            droneSkill.skillName = "Drones";
            //droneSkill.skillDescriptionToken = "Increase <style=cIsUtility>move speed</style> and <style=cIsDamage>attack speed by " + EntityStates.HANDOverclocked.Overclock.attackSpeedBonus.ToString("P0").Replace(" ", "") + "</style>.";
            //droneSkill.skillDescriptionToken += " All attacks <style=cIsHealing>give a temporary barrier on hit.</style> <style=cIsUtility>Increase duration by attacking enemies</style>.";
            droneSkill.skillDescriptionToken = "Expel a helper drone, <style=cIsHealing>healing yourself for 10% HP</style> and dealing <style=cIsDamage>" + EntityStates.HANDOverclocked.FireSeekingDrone.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>. ";
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
            droneSkill.icon = skillComponent.secondary.skillFamily.variants[0].skillDef.icon;
            droneSkill.activationStateMachineName = "Hook";
            droneSkill.isBullets = false;
            droneSkill.shootDelay = 0.1f;
            LoadoutAPI.AddSkillDef(droneSkill);

            SkillDef ovcSkill = SkillDef.CreateInstance<SkillDef>();
            ovcSkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.Overclock));
            ovcSkill.skillNameToken = "OVERCLOCK";
            ovcSkill.skillName = "Overclock";
            //ovcSkill.skillDescriptionToken = "<style=cIsUtility>Springy</style>. Gain a brief <style=cIsUtility>burst of speed</style> and activate <style=cIsDamage>OVERCLOCK</style> if it is available.";
            ovcSkill.skillDescriptionToken = "Gain <style=cIsUtility>+30% movement speed</style>, <style=cIsDamage>+40% attack speed</style>, and <style=cIsDamage>stun chance</style>. <style=cIsUtility>Hit enemies to increase duration</style>. Cancel OVERCLOCK to release steam and <style=cIsUtility>Spring</style> into the air, <style=cIsDamage>igniting</style> enemies for <style=cIsDamage>200%-600% damage</style>.";
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
            ovcSkill.icon = skillComponent.utility.skillFamily.variants[0].skillDef.icon;
            ovcSkill.activationStateMachineName = "Hook";
            ovcSkill.isBullets = false;
            ovcSkill.shootDelay = 0f;
            ovcSkill.keywordTokens = new string[] { "KEYWORD_HANDOVERCLOCKED_SPRINGY"};
            LoadoutAPI.AddSkillDef(ovcSkill);

            SkillFamily.Variant[] primaryVariants = new SkillFamily.Variant[1];
            primaryVariants[0].skillDef = primarySkill;
            primaryVariants[0].unlockableName = "";
            skillComponent.primary.skillFamily.variants = primaryVariants;

            SkillFamily.Variant[] secondaryVariants = new SkillFamily.Variant[1];
            secondaryVariants[0].skillDef = secondarySkill;
            secondaryVariants[0].unlockableName = "";
            skillComponent.secondary.skillFamily.variants = secondaryVariants;

            SkillFamily.Variant[] utilityVariants = new SkillFamily.Variant[1];
            utilityVariants[0].skillDef = ovcSkill;
            utilityVariants[0].unlockableName = "";
            skillComponent.utility.skillFamily.variants = utilityVariants;

            SkillFamily.Variant[] specialVariants = new SkillFamily.Variant[1];
            specialVariants[0].skillDef = droneSkill;
            specialVariants[0].unlockableName = "";
            skillComponent.special.skillFamily.variants = specialVariants;

            LoadoutAPI.AddSkillFamily(skillComponent.primary.skillFamily);
            LoadoutAPI.AddSkillFamily(skillComponent.secondary.skillFamily);
            LoadoutAPI.AddSkillFamily(skillComponent.utility.skillFamily);
            LoadoutAPI.AddSkillFamily(skillComponent.special.skillFamily);
        }

        private void CreateOverclockBuff()
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
            HAND_OVERCLOCKED.OverclockBuff = BuffAPI.Add(new CustomBuff(OverclockBuffDef));
        }

        private void AddSkin()    //credits to rob
        {
            GameObject bodyPrefab = HANDBody;
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = null;
            if (model.GetComponent<ModelSkinController>())
                skinController = model.GetComponent<ModelSkinController>();
            else
                skinController = model.AddComponent<ModelSkinController>();

            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");
            if (mainRenderer == null)
            {
                CharacterModel.RendererInfo[] bRI = Reflection.GetFieldValue<CharacterModel.RendererInfo[]>(characterModel, "baseRendererInfos");
                if (bRI != null)
                {
                    foreach (CharacterModel.RendererInfo rendererInfo in bRI)
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

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(new Color(0f, 156f / 255f, 188f / 255f), new Color(186f / 255f, 128f / 255f, 52f / 255f), new Color(58f / 255f, 49f / 255f, 24f / 255f), new Color(2f / 255f, 29f / 255f, 55f / 255f));
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            skinDefInfo.Name = "HANDOCBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "HANDOCBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.UnlockableName = "";
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            skinController.skins = new SkinDef[1]
            {
                defaultSkin,
            };
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
    }
}
