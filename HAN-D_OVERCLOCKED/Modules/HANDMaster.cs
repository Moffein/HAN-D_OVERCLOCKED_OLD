using RoR2;
using UnityEngine;
using RoR2.CharacterAI;
using R2API;

namespace HandPlugin.Modules
{
    public class HANDMaster
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            HAND_OVERCLOCKED.HANDMonsterMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/charactermasters/commandomonstermaster"), "HANDOverclockedMonsterMaster", true);
            HANDContent.masterPrefabs.Add(HAND_OVERCLOCKED.HANDMonsterMaster);

            CharacterMaster cm = HAND_OVERCLOCKED.HANDMonsterMaster.GetComponent<CharacterMaster>();
            cm.bodyPrefab = HAND_OVERCLOCKED.HANDBody;

            Component[] toDelete = HAND_OVERCLOCKED.HANDMonsterMaster.GetComponents<AISkillDriver>();
            foreach (AISkillDriver asd in toDelete)
            {
                UnityEngine.Object.Destroy(asd);
            }

            AISkillDriver special = HAND_OVERCLOCKED.HANDMonsterMaster.AddComponent<AISkillDriver>();
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

            AISkillDriver utility = HAND_OVERCLOCKED.HANDMonsterMaster.AddComponent<AISkillDriver>();
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

            AISkillDriver secondary = HAND_OVERCLOCKED.HANDMonsterMaster.AddComponent<AISkillDriver>();
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

            AISkillDriver primary = HAND_OVERCLOCKED.HANDMonsterMaster.AddComponent<AISkillDriver>();
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

            AISkillDriver chase = HAND_OVERCLOCKED.HANDMonsterMaster.AddComponent<AISkillDriver>();
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

            AISkillDriver afk = HAND_OVERCLOCKED.HANDMonsterMaster.AddComponent<AISkillDriver>();
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
