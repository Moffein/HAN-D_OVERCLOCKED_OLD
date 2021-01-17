using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class API
{
    #region Catalog Helpers
    /// <summary>
    /// Adds a GameObject to the projectile catalog and returns true
    /// GameObject must be non-null and have a ProjectileController component
    /// returns false if GameObject is null or is missing the component        
    /// </summary>
    /// <param name="projectileObject">The projectile to register to the projectile catalog.</param>
    /// <returns></returns>
    public static bool RegisterNewProjectile(GameObject projectileObject)
    {
        if (projectileObject.HasComponent<ProjectileController>())
        {
            ProjectileCatalog.getAdditionalEntries += list => list.Add(projectileObject);
            LogCore.LogD("Registered projectile " + projectileObject.name + " to the projectile catalog!");
            return true;
        }
        LogCore.LogF("FATAL ERROR:" + projectileObject.name + " failed to register to the projectile catalog!");
        return false;
    }
    /// <summary>
    /// Adds a GameObject to the body catalog and returns true
    /// GameObject must be non-null and have a CharacterBody component
    /// returns false if GameObject is null or is missing the component
    /// </summary>
    /// <param name="bodyObject">The body to register to the body catalog.</param>
    /// <returns></returns>
    public static bool RegisterNewBody(GameObject bodyObject)
    {
        if (bodyObject)
        {
            BodyCatalog.getAdditionalEntries += list => list.Add(bodyObject);
            LogCore.LogD("Registered body " + bodyObject.name + " to the body catalog!");
            return true;
        }
        LogCore.LogF("FATAL ERROR:" + bodyObject.name + " failed to register to the body catalog!");
        return false;
    }
    /// <summary>
    /// Adds a GameObject to the master catalog and returns true
    /// GameObject must be non-null and have a CharacterMaster component
    /// returns false if GameObject is null or is missing the component
    /// </summary>
    /// <param name="master">The master to register to the master catalog.</param>
    /// <returns></returns>
    public static bool RegisterNewMaster(GameObject master)
    {
        if (master && master.HasComponent<CharacterMaster>())
        {
            MasterCatalog.getAdditionalEntries += list => list.Add(master);
            LogCore.LogD("Registered master " + master.name + " to the master catalog!");
            return true;
        }
        LogCore.LogF("FATAL ERROR: " + master.name + " failed to register to the master catalog!");
        return false;
    }
    /// <summary>
    /// Destroys all the AISkillDrivers on the master GameObject
    /// </summary>
    /// <param name="masterObject"></param>
    public static void DestroySkillDrivers(GameObject master)
    {
        foreach (AISkillDriver skill in master.GetComponentsInChildren<AISkillDriver>())
        {
            Object.Destroy(skill);
        }
    }


    #endregion
    #region R2API Expanded
    #endregion
    #region Projectiles
    /// <summary>
    /// Creates a valid projectile from a GameObject 
    /// </summary>
    /// <param name="projectile"></param>
    public static void CreateValidProjectile(GameObject projectile, float lifeTime, float velocity, bool updateAfterFiring)
    {
        var networkIdentity = projectile.AddComponent<NetworkIdentity>();
        var teamFilter = projectile.AddComponent<TeamFilter>();
        var projectileController = projectile.AddComponent<ProjectileController>();
        var networkTransform = projectile.AddComponent<ProjectileNetworkTransform>();
        var projectileSimple = projectile.AddComponent<ProjectileSimple>();
        var projectileDamage = projectile.AddComponent<ProjectileDamage>();

        //setup the projectile controller
        projectileController.allowPrediction = false;
        projectileController.predictionId = 0;
        projectileController.procCoefficient = 1;
        projectileController.owner = null;

        //setup the network transform
        networkTransform.allowClientsideCollision = false;
        networkTransform.interpolationFactor = 1;
        networkTransform.positionTransmitInterval = 0.03333334f;

        //setup the projectile simple
        projectileSimple.velocity = velocity;
        projectileSimple.lifetime = lifeTime;
        projectileSimple.updateAfterFiring = updateAfterFiring;
        projectileSimple.enableVelocityOverLifetime = false;
        //projectileSimple.velocityOverLifetime = UnityEngine.AnimationCurve.;
        projectileSimple.oscillate = false;
        projectileSimple.oscillateMagnitude = 20;
        projectileSimple.oscillateSpeed = 0;

        projectileDamage.damage = 0;
        projectileDamage.crit = false;
        projectileDamage.force = 0;
        projectileDamage.damageColorIndex = DamageColorIndex.Default;
        projectileDamage.damageType = DamageType.Shock5s;
    }
    #endregion
    #region AI
    /// <summary>
    /// Copies skilldriver settings from "beingCopiedFrom" to "copier"
    /// Don't forget to set requiredSkill!
    /// </summary>
    /// <param name="beingCopiedFrom"></param>
    /// <param name="copier"></param>
    public static void CopyAISkillSettings(AISkillDriver beingCopiedFrom, AISkillDriver copier)
    {
        copier.activationRequiresAimConfirmation = beingCopiedFrom.activationRequiresAimConfirmation;
        copier.activationRequiresTargetLoS = beingCopiedFrom.activationRequiresTargetLoS;
        copier.aimType = beingCopiedFrom.aimType;
        copier.buttonPressType = beingCopiedFrom.buttonPressType;
        copier.customName = beingCopiedFrom.customName;
        copier.driverUpdateTimerOverride = beingCopiedFrom.driverUpdateTimerOverride;
        copier.ignoreNodeGraph = beingCopiedFrom.ignoreNodeGraph;
        copier.maxDistance = beingCopiedFrom.maxDistance;
        copier.maxTargetHealthFraction = beingCopiedFrom.maxTargetHealthFraction;
        copier.maxUserHealthFraction = beingCopiedFrom.maxUserHealthFraction;
        copier.minDistance = beingCopiedFrom.minDistance;
        copier.minTargetHealthFraction = beingCopiedFrom.minTargetHealthFraction;
        copier.minUserHealthFraction = beingCopiedFrom.minUserHealthFraction;
        copier.moveInputScale = beingCopiedFrom.moveInputScale;
        copier.movementType = beingCopiedFrom.movementType;
        copier.moveTargetType = beingCopiedFrom.moveTargetType;
        copier.nextHighPriorityOverride = beingCopiedFrom.nextHighPriorityOverride;
        copier.noRepeat = beingCopiedFrom.noRepeat;
        //Don't do this because the skilldef is not the same.
        //_out.requiredSkill = _in.requiredSkill;
        copier.requireEquipmentReady = beingCopiedFrom.requireEquipmentReady;
        copier.requireSkillReady = beingCopiedFrom.requireSkillReady;
        copier.resetCurrentEnemyOnNextDriverSelection = beingCopiedFrom.resetCurrentEnemyOnNextDriverSelection;
        copier.selectionRequiresOnGround = beingCopiedFrom.selectionRequiresOnGround;
        copier.selectionRequiresTargetLoS = beingCopiedFrom.selectionRequiresTargetLoS;
        copier.shouldFireEquipment = beingCopiedFrom.shouldFireEquipment;
        copier.shouldSprint = beingCopiedFrom.shouldSprint;
        //shouldTapButton is deprecated, don't use it!
        //_out.shouldTapButton = _in.shouldTapButton;
        copier.skillSlot = beingCopiedFrom.skillSlot;

    }
    #endregion
    #region UNITY2ROR2
    public static void AddExplosionForce(CharacterMotor body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier = 0)
    {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);

        Vector3 baseForce = dir.normalized * explosionForce * wearoff;
        //baseForce.z = 0;
        body.ApplyForce(baseForce);

        //if (upliftModifier != 0)
        //{
        float upliftWearoff = 1 - upliftModifier / explosionRadius;
        Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
        //upliftForce.z = 0;
        body.ApplyForce(upliftForce);
        //}

    }
    #endregion
    #region Skills
    /// <summary>
    /// Destroys generic skill components attached to the survivor object and creates an empty SkillFamily.
    /// </summary>
    /// <param name="survivor"></param>
    public static void CreateEmptySkills(GameObject survivor)
    {
        if (survivor)
        {
            DestroyGenericSkillComponents(survivor);
            CreateEmptySkillFamily(survivor);
        }
        else LogCore.LogF("Tried to create empty skills on a null GameObject!");
    }
    /// <summary>
    /// Destroys generic skill components attached to the survivor object
    /// </summary>
    /// <param name="survivor"></param>
    public static void DestroyGenericSkillComponents(GameObject survivor)
    {
        foreach (GenericSkill skill in survivor.GetComponentsInChildren<GenericSkill>())
        {
            Object.DestroyImmediate(skill);
        }
    }
    /// <summary>
    /// Creates an EmptySkillFamily. Be sure to call DestroyGenericSkillComponents before doing this.
    /// </summary>
    /// <param name="survivor"></param>
    public static void CreateEmptySkillFamily(GameObject survivor)
    {
        SkillLocator skillLocator = survivor.GetComponent<SkillLocator>();
        skillLocator.SetFieldValue<GenericSkill[]>("allSkills", new GenericSkill[0]);
        {
            skillLocator.primary = survivor.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            skillLocator.primary.SetFieldValue("_skillFamily", newFamily);
        }
        {
            skillLocator.secondary = survivor.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            skillLocator.secondary.SetFieldValue("_skillFamily", newFamily);
        }
        {
            skillLocator.utility = survivor.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            skillLocator.utility.SetFieldValue("_skillFamily", newFamily);
        }
        {
            skillLocator.special = survivor.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            skillLocator.special.SetFieldValue("_skillFamily", newFamily);
        }
    }

    /// <summary>
    /// Copies skilldef settings from beingCopiedFrom to copier.
    /// </summary>
    /// <param name="beingCopiedFrom"></param>
    /// <param name="copier"></param>
    public static void CopySkillDefSettings(SkillDef beingCopiedFrom, SkillDef copier)
    {
        copier.activationState = beingCopiedFrom.activationState;
        copier.activationStateMachineName = beingCopiedFrom.activationStateMachineName;
        copier.baseMaxStock = beingCopiedFrom.baseMaxStock;
        copier.baseRechargeInterval = beingCopiedFrom.baseRechargeInterval;
        copier.beginSkillCooldownOnSkillEnd = beingCopiedFrom.beginSkillCooldownOnSkillEnd;
        copier.canceledFromSprinting = beingCopiedFrom.canceledFromSprinting;
        copier.dontAllowPastMaxStocks = beingCopiedFrom.dontAllowPastMaxStocks;
        copier.forceSprintDuringState = beingCopiedFrom.forceSprintDuringState;
        copier.fullRestockOnAssign = beingCopiedFrom.fullRestockOnAssign;
        copier.icon = beingCopiedFrom.icon;
        copier.interruptPriority = beingCopiedFrom.interruptPriority;
        copier.isBullets = beingCopiedFrom.isBullets;
        copier.isCombatSkill = beingCopiedFrom.isCombatSkill;
        copier.keywordTokens = beingCopiedFrom.keywordTokens;
        copier.mustKeyPress = beingCopiedFrom.mustKeyPress;
        copier.noSprint = beingCopiedFrom.noSprint;
        copier.rechargeStock = beingCopiedFrom.rechargeStock;
        copier.shootDelay = beingCopiedFrom.shootDelay;
        copier.skillDescriptionToken = beingCopiedFrom.skillDescriptionToken;
        copier.skillName = beingCopiedFrom.skillName;
        copier.skillNameToken = beingCopiedFrom.skillNameToken;
        copier.stockToConsume = beingCopiedFrom.stockToConsume;
    }
    #endregion
}
