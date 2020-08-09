using BepInEx;
using EntityStates;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Orbs;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HAND_OVERCLOCKED
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.HAND_Overclocked", "HAN-D OVERCLOCKED", "1.0.0")]
    [R2APISubmoduleDependency(nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(PrefabAPI), nameof(ResourcesAPI), nameof(BuffAPI), nameof(LanguageAPI))]
    class HAND_OVERCLOCKED : BaseUnityPlugin
    {
        public static GameObject HANDBody = null;
        public static GameObject droneProjectileOrb = null;
        Texture2D HANDIcon = null;
        Color HANDColor = new Color(0.556862745f, 0.682352941f, 0.690196078f);
        Color OverclockColor = new Color(1.0f, 0.45f, 0f);
        String HANDBodyName = "";
        String HANDDesc = "";
        public static BuffIndex OverclockBuff;
        public static int maxOverclock;
        public static float overclockDecay;
        public static float overclockBaseDecay;
        public static float overclockArmor;
        public static float overclockAtkSpd;
        public static float overclockSpd;

        const String assetPrefix = "@MoffeinHAND_OVERCLOCKED";
        const String portraitPath = assetPrefix + ":HAND_Overclocked_portrait.png";

        bool useBodyClone = true;

        public void Start()
        {
            SetAttributes();
            InitSkills();
            AssignSkills();
        }

        public void Awake()
        {
            SetBody();
            AddSkin();
            HANDDesc += "HAN-D is a tanky survivor who pounds enemies into submission.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > All of HAN-D's attacks can be used while sprinting." + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > HURT has increased knockback against airborne enemies. Use FORCED_REASSEMBLY to pop enemies in the air, then HURT them to send them flying!" + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > Alternate between BLASTOFF and FORCED_REASSEMBLY to keep crowds of enemies stunned." + Environment.NewLine + Environment.NewLine;
            HANDDesc += "< ! > Use DRONES to heal and stay in the fight!" + Environment.NewLine + Environment.NewLine;

            GameObject HANDDisplay = HANDBody.GetComponent<ModelLocator>().modelTransform.gameObject;
            HANDDisplay.AddComponent<MenuAnimComponent>();
            SurvivorDef item = new SurvivorDef
            {
                bodyPrefab = HANDBody,
                descriptionToken = HANDDesc,
                displayPrefab = HANDDisplay,
                primaryColor = HANDColor,
                unlockableName = ""
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

            On.RoR2.HealthComponent.TakeDamage += (orig, self, di) =>
            {
                bool attackerHAND = di.attacker != null && di.attacker.name == (HANDBodyName + "(Clone)");
                bool  aliveBeforeHit = self.alive;
                CharacterBody handBody = null;
                if (attackerHAND)
                {
                    handBody = di.attacker.gameObject.GetComponent<CharacterBody>();
                }
                orig(self, di);
                if (!self.alive && aliveBeforeHit && attackerHAND)
                {
                    if (handBody.skillLocator.special.stock < handBody.skillLocator.special.maxStock)
                    {
                        if (Util.CheckRoll(self.globalDeathEventChanceCoefficient*100f, handBody.master? handBody.master.luck : 0f, null))
                        {
                            handBody.skillLocator.special.AddOneStock();
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
                    self.SetPropertyValue<float>("attackSpeed", self.attackSpeed * (1 + overclockAtkSpd * ovcCount));
                    self.SetPropertyValue<float>("armor", self.armor * (1 + overclockArmor * ovcCount));
                    self.SetPropertyValue<float>("moveSpeed", self.moveSpeed * (1 + overclockSpd * ovcCount));
                }
            };
        }

        private void UpdateOverclock(CharacterBody cb)
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
                    HANDBody = Resources.Load<GameObject>("prefabs/characterbodies/handbody").InstantiateClone("HANDOverclocked", true);
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
                if (useBodyClone)
                {
                    droneProjectileOrb = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/ArrowOrbEffect").InstantiateClone("HOCDroneOrb", false);
                }
                else
                {
                    droneProjectileOrb = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/ArrowOrbEffect");
                }
            }
        }

        private void SetAttributes()
        {
            HANDBody.AddComponent<HANDController>();
            SfxLocator sfx = HANDBody.GetComponent<SfxLocator>();
            sfx.landingSound = "play_char_land";
            sfx.fallDamageSound = "Play_MULT_shift_hit";

            HANDBody.tag = "SkinReady";

            CharacterBody cb = HANDBody.GetComponent<CharacterBody>();
            cb.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            cb.subtitleNameToken = "Cleanup Crew";
            cb.crosshairPrefab = Resources.Load<GameObject>("prefabs/crosshair/simpledotcrosshair");
            cb.hideCrosshair = false;

            CharacterDeathBehavior handCDB = cb.gameObject.GetComponent<CharacterDeathBehavior>();
            handCDB.deathState = Resources.Load<GameObject>("prefabs/characterbodies/WispBody").GetComponent<CharacterDeathBehavior>().deathState;

            cb.baseNameToken = "HAND_OVERCLOCKED_NAME";
            LanguageAPI.Add("HAND_OVERCLOCKED_NAME", "HAN-D");

            String tldr = "<style=cMono>\r\n//--AUTO-TRANSCRIPTION FROM BASED DEPARTMENT OF UES SAFE TRAVELS--//</style>\r\n\r\n<i>*hits <color=#327FFF>Spinel Tonic</color>*</i>\n\nIs playing without the <color=#6955A6>Command</color> artifact the ultimate form of cuckoldry?\n\nI cannot think or comprehend of anything more cucked than playing without <color=#6955A6>Command</color>. Honestly, think about it rationally. You are shooting, running, jumping for like 60 minutes solely so you can get a fucking <color=#77FF16>Squid Polyp</color>. All that hard work you put into your run - dodging <style=cIsHealth>Stone Golem</style> lasers, getting annoyed by six thousand <style=cIsHealth>Lesser Wisps</color> spawning above your head, activating <color=#E5C962>Shrines of the Mountain</color> all for one simple result: your inventory is filled up with <color=#FFFFFF>Warbanners</color> and <color=#FFFFFF>Monster Tooth</color> necklaces which cost money.\n\nOn a god run? Great. A bunch of shitty items which add nothing to your run end up coming out of the <color=#E5C962>Chests</color> you buy. They get the benefit of your hard earned dosh that came from killing <style=cIsHealth>Lemurians</style>.\n\nAs a man who plays this game you are <style=cIsHealth>LITERALLY</style> dedicating two hours of your life to opening boxes and praying it's not another <color=#77FF16>Chronobauble</color>. It's the ultimate and final cuck. Think about it logically.\r\n<style=cMono>\r\nTranscriptions complete.\r\n</style>\r\n \r\n\r\n";
            LanguageAPI.Add("HAND_OVERCLOCKED_LORE", tldr);

            cb.baseMaxHealth = 150f;
            cb.baseRegen = 2.5f;
            cb.baseMaxShield = 0f;
            cb.baseMoveSpeed = 7f;
            cb.baseAcceleration = 25f;
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
            maxOverclock = 10;
            overclockDecay = 1.5f;
            overclockBaseDecay = 4.5f;
            overclockArmor = 0f;
            overclockAtkSpd = 0.03f;
            overclockSpd = 0.03f;

            EntityStates.HANDOverclocked.Overheat.damageCoefficient = 2.3f;
            EntityStates.HANDOverclocked.Overheat.radius = 9f;
            EntityStates.HANDOverclocked.Overheat.healPercent = 0.1f;
            EntityStates.HANDOverclocked.Overheat.effectPrefab = Resources.Load<GameObject>("prefabs/effects/WilloWispExplosion");
            EntityStates.HANDOverclocked.Overheat.baseDuration = 0.25f;
            EntityStates.HANDOverclocked.Overheat.soundString = "Play_clayboss_M1_explo";

            EntityStates.HANDOverclocked.FullSwing.damageCoefficient = 4f;
            EntityStates.HANDOverclocked.FullSwing.baseDuration = 1f;
            EntityStates.HANDOverclocked.FullSwing.airbornVerticalForce = 0f;
            EntityStates.HANDOverclocked.FullSwing.forceMagnitude = 1200f;
            EntityStates.HANDOverclocked.FullSwing.airbornHorizontalForceMult = 2.2f;
            EntityStates.HANDOverclocked.FullSwing.flyingHorizontalForceMult = 1.1f;
            EntityStates.HANDOverclocked.FullSwing.shorthopVelocityFromHit = 8f;
            EntityStates.HANDOverclocked.FullSwing.returnToIdlePercentage = 0.443662f;
            EntityStates.HANDOverclocked.FullSwing.swingEffectPrefab = null;
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.FullSwing));

            EntityStates.HANDOverclocked.BlastOff.effectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/omniexplosionvfx");
            EntityStates.HANDOverclocked.BlastOff.jumpForce = 27f;
            EntityStates.HANDOverclocked.BlastOff.forceMagnitude = 16f;
            EntityStates.HANDOverclocked.BlastOff.damageCoefficient = 2.5f;
            EntityStates.HANDOverclocked.BlastOff.radius = 9f;
            EntityStates.HANDOverclocked.BlastOff.baseDuration = 0.2f;
            EntityStates.HANDOverclocked.BlastOff.airbornVerticalForce = -2000f;
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.BlastOff));

            EntityStates.HANDOverclocked.ChargeSlam.baseDuration = 1.0f;
            EntityStates.HANDOverclocked.Slam.baseDuration = 0.8f;
            EntityStates.HANDOverclocked.Slam.damageCoefficient = 6f;
            EntityStates.HANDOverclocked.Slam.baseMinDuration = 0.4f;
            EntityStates.HANDOverclocked.Slam.forceMagnitude = 2000f;
            EntityStates.HANDOverclocked.Slam.airbornVerticalForce = -2400f;
            EntityStates.HANDOverclocked.Slam.shorthopVelocityFromHit = 22.5f;
            EntityStates.HANDOverclocked.Slam.hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/ImpactToolbotDashLarge");
            EntityStates.HANDOverclocked.Slam.impactEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/PodGroundImpact");
            
            EntityStates.HANDOverclocked.Slam.swingEffectPrefab = Resources.Load<GameObject>("prefabs/effects/handslamtrail");
            EntityStates.HANDOverclocked.Slam.returnToIdlePercentage = 0.443662f;
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.ChargeSlam));
            LoadoutAPI.AddSkill(typeof(EntityStates.HANDOverclocked.Slam));

            EntityStates.HANDOverclocked.FireSeekingDrone.orbDamageCoefficient = 2.7f;
            EntityStates.HANDOverclocked.FireSeekingDrone.orbProcCoefficient = 1.0f;
            EntityStates.HANDOverclocked.FireSeekingDrone.baseDuration = 0.25f;
            EntityStates.HANDOverclocked.FireSeekingDrone.healPercent = 0.085f;
            EntityStates.HANDOverclocked.FireSeekingDrone.effectPrefab = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");

            /*if (useBodyClone)
            {
                AkEvent[] events = droneProjectileOrb.GetComponents<AkEvent>();
                foreach(AkEvent ak in events)
                {
                    ak.data = null;
                }
            }*/
        }

        private void AssignSkills()
        {
            SkillLocator skillComponent = HANDBody.GetComponent<SkillLocator>();

            skillComponent.passiveSkill.enabled = true;
            skillComponent.passiveSkill.icon = skillComponent.utility.skillFamily.variants[0].skillDef.icon;
            skillComponent.passiveSkill.skillNameToken = "OVERCLOCK";
            skillComponent.passiveSkill.skillDescriptionToken = "Gain increased <style=cIsUtility>movement speed</style> and <style=cIsDamage>attack speed</style> when <style=cIsDamage>hitting enemies</style>. <style=cIsUtility>Stacks up to 10 times</style>.";

            SkillDef primarySkill = SkillDef.CreateInstance<SkillDef>();
            primarySkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.FullSwing));
            primarySkill.skillNameToken = "HURT";
            primarySkill.skillName = "FullSwing";
            primarySkill.skillDescriptionToken = "Swing your hammer in a wide arc, hurting enemies for <style=cIsDamage>" + EntityStates.HANDOverclocked.FullSwing.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>.";
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
            LoadoutAPI.AddSkillDef(primarySkill);

            SkillDef secondarySkill = SkillDef.CreateInstance<SkillDef>();
            secondarySkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.ChargeSlam));
            secondarySkill.skillNameToken = "FORCED_REASSEMBLY";
            secondarySkill.skillName = "ChargeSlam";
            secondarySkill.skillDescriptionToken = "Slam your hammer downwards with great force, dealing <style=cIsDamage>" + EntityStates.HANDOverclocked.Slam.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style> and <style=cIsDamage>stunning</style> enemies.";
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
            LoadoutAPI.AddSkillDef(secondarySkill);

            SkillDef utilitySkill = SkillDef.CreateInstance<SkillDef>();
            utilitySkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.BlastOff));
            utilitySkill.skillNameToken = "BLASTOFF";
            utilitySkill.skillName = "BlastOff";
            utilitySkill.skillDescriptionToken = "Engage thrusters and <style=cIsUtility>take flight</style>, <style=cIsDamage>stunning</style> nearby enemies for <style=cIsDamage>" + EntityStates.HANDOverclocked.BlastOff.damageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>.";
            utilitySkill.noSprint = false;
            utilitySkill.canceledFromSprinting = false;
            utilitySkill.baseRechargeInterval = 5f;
            utilitySkill.baseMaxStock = 1;
            utilitySkill.rechargeStock = 1;
            utilitySkill.isBullets = false;
            utilitySkill.shootDelay = 0.1f;
            utilitySkill.beginSkillCooldownOnSkillEnd = false;
            utilitySkill.activationStateMachineName = "Hook";
            utilitySkill.interruptPriority = EntityStates.InterruptPriority.Skill;
            utilitySkill.isCombatSkill = false;
            utilitySkill.mustKeyPress = true;
            utilitySkill.icon = skillComponent.utility.skillFamily.variants[0].skillDef.icon;
            utilitySkill.requiredStock = 1;
            utilitySkill.stockToConsume = 1;
            LoadoutAPI.AddSkillDef(utilitySkill);

            SkillDef specialSkill = SkillDef.CreateInstance<SkillDef>();
            specialSkill.activationState = new SerializableEntityStateType(typeof(EntityStates.HANDOverclocked.FireSeekingDrone));
            specialSkill.skillNameToken = "DRONES";
            specialSkill.skillName = "Drones";
            //specialSkill.skillDescriptionToken = "Increase <style=cIsUtility>move speed</style> and <style=cIsDamage>attack speed by " + EntityStates.HANDOverclocked.Overclock.attackSpeedBonus.ToString("P0").Replace(" ", "") + "</style>.";
            //specialSkill.skillDescriptionToken += " All attacks <style=cIsHealing>give a temporary barrier on hit.</style> <style=cIsUtility>Increase duration by attacking enemies</style>.";
            specialSkill.skillDescriptionToken = "Expell a helper drone, <style=cIsHealing>healing yourself</style> and dealing <style=cIsDamage>" + EntityStates.HANDOverclocked.FireSeekingDrone.orbDamageCoefficient.ToString("P0").Replace(" ", "") + " damage</style>. ";
            specialSkill.skillDescriptionToken += "<style=cIsUtility>Gain charges by hitting or killing enemies.</style>";
            specialSkill.isCombatSkill = true;
            specialSkill.noSprint = false;
            specialSkill.canceledFromSprinting = false;
            specialSkill.baseRechargeInterval = 0f;
            specialSkill.interruptPriority = EntityStates.InterruptPriority.Skill;
            specialSkill.mustKeyPress = false;
            specialSkill.beginSkillCooldownOnSkillEnd = true;
            specialSkill.baseMaxStock = 10;
            specialSkill.fullRestockOnAssign = false;
            specialSkill.rechargeStock = 0;
            specialSkill.requiredStock = 1;
            specialSkill.stockToConsume = 1;
            specialSkill.icon = skillComponent.secondary.skillFamily.variants[0].skillDef.icon;
            specialSkill.activationStateMachineName = "Hook";
            specialSkill.isBullets = false;
            specialSkill.shootDelay = 0.08f;
            LoadoutAPI.AddSkillDef(specialSkill);

            SkillFamily.Variant[] primaryVariants = new SkillFamily.Variant[1];
            primaryVariants[0].skillDef = primarySkill;
            primaryVariants[0].unlockableName = "";
            skillComponent.primary.skillFamily.variants = primaryVariants;

            SkillFamily.Variant[] secondaryVariants = new SkillFamily.Variant[1];
            secondaryVariants[0].skillDef = secondarySkill;
            secondaryVariants[0].unlockableName = "";
            skillComponent.secondary.skillFamily.variants = secondaryVariants;

            SkillFamily.Variant[] utilityVariants = new SkillFamily.Variant[1];
            utilityVariants[0].skillDef = utilitySkill;
            utilityVariants[0].unlockableName = "";
            skillComponent.utility.skillFamily.variants = utilityVariants;

            SkillFamily.Variant[] specialVariants = new SkillFamily.Variant[1];
            specialVariants[0].skillDef = specialSkill;
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
                canStack = true,
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

            LanguageAPI.Add("HANDOCBODY_DEFAULT_SKIN_NAME", "Default");

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

            SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            skinController.skins = new SkinDef[1]
            {
                defaultSkin,
            };
        }
    }
}
