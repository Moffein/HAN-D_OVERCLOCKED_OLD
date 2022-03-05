using EntityStates;
using EntityStates.HANDOverclocked;
using HAND_OVERCLOCKED.Components.DroneProjectile;
using HandPlugin.Components;
using HandPlugin.Components.DroneProjectile;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HandPlugin.Modules
{
    public class HANDSkills
    {
        private static bool initialized = false;
        public static void Init(GameObject HANDBody)
        {
            if (initialized) return;
            initialized = true;

            SkillLocator skillLocator = HANDBody.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = "HAND_OVERCLOCKED_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = "HAND_OVERCLOCKED_PASSIVE_DESC";
            skillLocator.passiveSkill.icon = HANDContent.assets.LoadAsset<Sprite>("Passive.png");

            CreatePrimary(skillLocator);
            CreateSecondary(skillLocator);
            CreateUtility(skillLocator, HANDBody);
            CreateSpecial(skillLocator, HANDBody);
        }

        private static void CreatePrimary(SkillLocator skillLocator)
        {
            HANDContent.entityStates.Add(typeof(FullSwing));

            SkillDef primarySkill = SkillDef.CreateInstance<SkillDef>();
            primarySkill.activationState = new SerializableEntityStateType(typeof(FullSwing));
            primarySkill.skillNameToken = "HAND_OVERCLOCKED_PRIMARY_NAME";
            primarySkill.skillName = "FullSwing";
            primarySkill.skillDescriptionToken = "HAND_OVERCLOCKED_PRIMARY_DESC";
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
            primarySkill.keywordTokens = new string[] { };
            FixScriptableObjectName(primarySkill);
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

        private static void CreateSecondary(SkillLocator skillLocator)
        {
            HANDContent.entityStates.Add(typeof(ChargeSlam2));
            HANDContent.entityStates.Add(typeof(Slam2));

            SkillDef secondarySkill = SkillDef.CreateInstance<SkillDef>();
            secondarySkill.activationState = new SerializableEntityStateType(typeof(ChargeSlam2));
            secondarySkill.skillNameToken = "HAND_OVERCLOCKED_SECONDARY_NAME";
            secondarySkill.skillName = "ChargeSlam";
            secondarySkill.skillDescriptionToken = "HAND_OVERCLOCKED_SECONDARY_DESC";
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
            FixScriptableObjectName(secondarySkill);
            HANDContent.skillDefs.Add(secondarySkill);

            SkillFamily secondarySkillFamily = skillLocator.secondary.skillFamily;

            secondarySkillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = secondarySkill,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(secondarySkill.skillNameToken, false, null)

            };
            HANDContent.skillFamilies.Add(secondarySkillFamily);

            HANDContent.entityStates.Add(typeof(ChargeSlamScepter));
            HANDContent.entityStates.Add(typeof(SlamScepter));

            SkillDef secondarySkillScepter = SkillDef.CreateInstance<SkillDef>();
            secondarySkillScepter.activationState = new SerializableEntityStateType(typeof(ChargeSlamScepter));
            secondarySkillScepter.skillNameToken = "HAND_OVERCLOCKED_SECONDARY_SCEPTER_NAME";
            secondarySkillScepter.skillName = "ChargeSlam";
            secondarySkillScepter.skillDescriptionToken = "HAND_OVERCLOCKED_SECONDARY_SCEPTER_DESC";
            secondarySkillScepter.cancelSprintingOnActivation = true;
            secondarySkillScepter.canceledFromSprinting = false;
            secondarySkillScepter.baseRechargeInterval = secondarySkill.baseRechargeInterval;
            secondarySkillScepter.baseMaxStock = 1;
            secondarySkillScepter.rechargeStock = 1;
            secondarySkillScepter.requiredStock = 1;
            secondarySkillScepter.stockToConsume = 1;
            secondarySkillScepter.activationStateMachineName = "Weapon";
            secondarySkillScepter.interruptPriority = EntityStates.InterruptPriority.Skill;
            secondarySkillScepter.isCombatSkill = true;
            secondarySkillScepter.mustKeyPress = false;
            secondarySkillScepter.icon = HANDContent.assets.LoadAsset<Sprite>("Unethical_Reassembly.png");
            secondarySkillScepter.beginSkillCooldownOnSkillEnd = true;
            secondarySkillScepter.keywordTokens = new string[] { "KEYWORD_STUNNING", "KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            FixScriptableObjectName(secondarySkillScepter);
            HANDContent.skillDefs.Add(secondarySkillScepter);

            HAND_OVERCLOCKED.scepterDef = secondarySkillScepter;
        }

        private static void CreateUtility(SkillLocator skillLocator, GameObject HANDBody)
        {
            HANDContent.entityStates.Add(typeof(BeginOverclock));
            HANDContent.entityStates.Add(typeof(CancelOverclock));

            EntityStateMachine stateMachine = HANDBody.AddComponent<EntityStateMachine>();
            stateMachine.customName = "Overclock";
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            NetworkStateMachine nsm = HANDBody.GetComponent<NetworkStateMachine>();
            nsm.stateMachines = nsm.stateMachines.Append(stateMachine).ToArray();

            SkillDef ovcSkill = SkillDef.CreateInstance<SkillDef>();
            ovcSkill.activationState = new SerializableEntityStateType(typeof(BeginOverclock));
            ovcSkill.skillNameToken = "HAND_OVERCLOCKED_UTILITY_NAME";
            ovcSkill.skillName = "BeginOverclock";
            ovcSkill.skillDescriptionToken = "HAND_OVERCLOCKED_UTILITY_DESC";
            ovcSkill.isCombatSkill = false;
            ovcSkill.cancelSprintingOnActivation = false;
            ovcSkill.canceledFromSprinting = false;
            ovcSkill.baseRechargeInterval = 7f;
            ovcSkill.interruptPriority = EntityStates.InterruptPriority.Any;
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
            FixScriptableObjectName(ovcSkill);
            HANDContent.skillDefs.Add(ovcSkill);

            SkillDef ovcCancelDef = SkillDef.CreateInstance<SkillDef>();
            ovcCancelDef.activationState = new SerializableEntityStateType(typeof(CancelOverclock));
            ovcCancelDef.activationStateMachineName = "Overclock";
            ovcCancelDef.baseMaxStock = 1;
            ovcCancelDef.baseRechargeInterval = 7f;
            ovcCancelDef.beginSkillCooldownOnSkillEnd = true;
            ovcCancelDef.canceledFromSprinting = false;
            ovcCancelDef.dontAllowPastMaxStocks = true;
            ovcCancelDef.forceSprintDuringState = false;
            ovcCancelDef.fullRestockOnAssign = true;
            ovcCancelDef.icon = HANDContent.assets.LoadAsset<Sprite>("Overclock_Cancel.png");
            ovcCancelDef.interruptPriority = InterruptPriority.Skill;
            ovcCancelDef.isCombatSkill = false;
            ovcCancelDef.keywordTokens = new string[] { "KEYWORD_HANDOVERCLOCKED_SPRINGY" };
            ovcCancelDef.mustKeyPress = true;
            ovcCancelDef.cancelSprintingOnActivation = false;
            ovcCancelDef.rechargeStock = 1;
            ovcCancelDef.requiredStock = 0;
            ovcCancelDef.skillName = "CancelOverclock";
            ovcCancelDef.skillNameToken = "HAND_OVERCLOCKED_UTILITY_CANCEL_NAME";
            ovcCancelDef.skillDescriptionToken = "HAND_OVERCLOCKED_UTILITY_CANCEL_DESCRIPTION";
            ovcCancelDef.stockToConsume = 0;
            FixScriptableObjectName(ovcCancelDef);
            HANDContent.skillDefs.Add(ovcCancelDef);
            BeginOverclock.cancelSkillDef = ovcCancelDef;


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
            OverclockController.ovcDef = ovcSkill;
        }

        private static void CreateSpecial(SkillLocator skillLocator, GameObject HANDBody)
        {
            FireSeekingDrone.projectilePrefab = HANDSkills.CreateDroneProjectile();

            HANDSkills.CreateDroneFollower();

            EntityStateMachine stateMachine = HANDBody.AddComponent<EntityStateMachine>();
            stateMachine.customName = "DroneLauncher";
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseBodyAttachmentState));
            NetworkStateMachine nsm = HANDBody.GetComponent<NetworkStateMachine>();
            nsm.stateMachines = nsm.stateMachines.Append(stateMachine).ToArray();

            SkillDef droneSkill = SkillDef.CreateInstance<SkillDef>();
            droneSkill.activationState = new SerializableEntityStateType(typeof(FireSeekingDrone));
            droneSkill.skillNameToken = "HAND_OVERCLOCKED_SPECIAL_NAME";
            droneSkill.skillName = "Drones";
            droneSkill.skillDescriptionToken = "HAND_OVERCLOCKED_SPECIAL_DESC";
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
            droneSkill.keywordTokens = new string[] { "KEYWORD_HANDOVERCLOCKED_DEBILITATE" };
            FixScriptableObjectName(droneSkill);
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

            DroneStockController.droneSkill = droneSkill;
        }

        private static void FixScriptableObjectName(SkillDef sk)
        {
            (sk as ScriptableObject).name = sk.skillName;
        }



        private static void CreateDroneFollower()
        {
            GameObject droneFollower = PrefabAPI.InstantiateClone(HANDContent.assets.LoadAsset<GameObject>("DronePrefab.prefab"), "HANDOverclockedDroneFollower", false);
            droneFollower.GetComponentInChildren<MeshRenderer>().material.shader = HAND_OVERCLOCKED.hotpoo;
            droneFollower.transform.localScale = 2f * Vector3.one;

            droneFollower.layer = LayerIndex.noCollision.intVal;
            UnityEngine.Object.Destroy(droneFollower.GetComponentInChildren<ParticleSystem>());
            Collider[] colliders = droneFollower.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                UnityEngine.Object.Destroy(c);
            }

            DroneFollowerController.dronePrefab = droneFollower;

            DroneFollowerController.activateEffect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");
            DroneFollowerController.deactivateEffect = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniImpactVFXLoader");

            droneFollower.GetComponentInChildren<SkinnedMeshRenderer>().material = Modules.Skins.CreateMaterial("DroneBody", 3f, Color.white);

            MeshRenderer[] mr = droneFollower.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mr)
            {
                if (m.name.ToLower() == "saw")
                {
                    m.material.shader = HAND_OVERCLOCKED.hotpoo;
                }
            }

            SkinnedMeshRenderer[] smr = droneFollower.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer m in smr)
            {
                m.material.shader = HAND_OVERCLOCKED.hotpoo;
            }
        }

        private static GameObject CreateDroneProjectile()
        {
            GameObject droneProjectile = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/EngiHarpoon").InstantiateClone("HANDOverclockedDroneProjectile", true);

            GameObject droneProjectileGhost = PrefabAPI.InstantiateClone(HANDContent.assets.LoadAsset<GameObject>("DronePrefab.prefab"), "HANDOverclockedDroneProjectileGhost", false);

            MeshRenderer[] mr = droneProjectileGhost.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer m in mr)
            {
                if (m.name.ToLower() == "saw")
                {
                    m.material.shader = HAND_OVERCLOCKED.hotpoo;
                }
            }

            SkinnedMeshRenderer[] smr = droneProjectileGhost.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer m in smr)
            {
                m.material.shader = HAND_OVERCLOCKED.hotpoo;
            }

            droneProjectileGhost.AddComponent<ProjectileGhostController>();
            droneProjectileGhost.transform.localScale = 2f * Vector3.one;

            droneProjectileGhost.layer = LayerIndex.noCollision.intVal;

            droneProjectile.GetComponent<ProjectileController>().ghostPrefab = droneProjectileGhost;

            droneProjectileGhost.GetComponentInChildren<SkinnedMeshRenderer>().material = Modules.Skins.CreateMaterial("DroneBody", 3f, Color.white);

            Collider[] collidersG = droneProjectileGhost.GetComponentsInChildren<Collider>();
            foreach (Collider cG in collidersG)
            {
                UnityEngine.Object.Destroy(cG);
            }

            HANDContent.projectilePrefabs.Add(droneProjectile);

            MissileController mc = droneProjectile.GetComponent<MissileController>();
            mc.maxVelocity = 25f;
            mc.acceleration = 3f;
            mc.maxSeekDistance = 160f;
            mc.giveupTimer = 20f;
            mc.deathTimer = 20f;

            UnityEngine.Object.Destroy(droneProjectile.GetComponent<AkGameObj>());
            UnityEngine.Object.Destroy(droneProjectile.GetComponent<AkEvent>());
            UnityEngine.Object.Destroy(droneProjectile.GetComponent<ProjectileSingleTargetImpact>());

            ProjectileStickOnImpact stick = droneProjectile.AddComponent<ProjectileStickOnImpact>();
            stick.ignoreWorld = true;
            stick.ignoreCharacters = false;
            stick.alignNormals = false;

            droneProjectile.AddComponent<DroneProjectileDamageController>();
            droneProjectile.AddComponent<PreventGroundCollision>();

            Collider[] colliders = droneProjectile.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                UnityEngine.Object.Destroy(c);
            }
            SphereCollider sc = droneProjectile.AddComponent<SphereCollider>();
            sc.radius = 0.6f;
            sc.contactOffset = 0.01f;

            return droneProjectile;
        }
    }
}
