using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using RoR2.Skills;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using HandPlugin.Modules;

namespace HandPlugin.Components
{
    public class DroneStockController : NetworkBehaviour//, IOnKilledOtherServerReceiver
    {
        public void Start()
        {
            characterBody.skillLocator.special.RemoveAllStocks();
            if (characterBody.master)
            {
                dronePersist = characterBody.master.gameObject.GetComponent<DroneStockPersist>();
                if (!dronePersist)
                {
                    dronePersist = characterBody.master.gameObject.AddComponent<DroneStockPersist>();
                }
                else
                {
                    if (characterBody.skillLocator.special.skillDef == droneSkill)
                    {
                        characterBody.skillLocator.special.stock = dronePersist.droneCount;
                    }
                }
            }
        }

        public void Awake()
        {
            characterBody = base.GetComponent<CharacterBody>();
        }

        public void FixedUpdate()
        {
            if (hasAuthority)
            {
                if (dronePersist && characterBody.skillLocator.special.skillDef == droneSkill)
                {
                    if (characterBody.skillLocator.special.stock > dronePersist.droneCount)
                    {
                        Util.PlaySound("Play_HOC_DroneGain", base.gameObject);
                    }
                    dronePersist.droneCount = characterBody.skillLocator.special.stock;
                }

                int pcCount = characterBody.skillLocator.special.stock;
                ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(characterBody.teamComponent.teamIndex);
                foreach (TeamComponent tc in teamMembers)
                {
                    if (tc.body && tc.body != characterBody )
                    {
                        if ((tc.body.bodyFlags & CharacterBody.BodyFlags.Mechanical) > 0 || CheckMechanicalBody(tc.body.baseNameToken))
                        {
                            pcCount++;
                        }
                    }
                }
                if (pcCount != oldPCCount)
                {
                    CmdUpdatePCBuff(pcCount);
                }
                oldPCCount = pcCount;
            }
        }

        public static bool CheckMechanicalBody(string str)
        {
            foreach (string name in mechanicalBodies)
            {
                if (str == name)
                {
                    return true;
                }
            }
            return false;
        }
        //Sniper comes with a non-ally drone that isn't counted as an ally.
        //You can add your survivor to this list if they don't have a Mechanical bodyflag but you want them to count. Use their BaseNameToken.
        public static List<string> mechanicalBodies = new List<string> { "SNIPERCLASSIC_BODY_NAME" };

        /*public void OnKilledOtherServer(DamageReport damageReport) //This seems to be called by both OnCharacterDeath and TakeDamage, resulting in it being called twice
        {
            if (hasAuthority && damageReport.attacker == base.gameObject)
            {
                Debug.Log("Calling OnKilledOtherServer");
                //RpcAddSpecialStock();
                if (characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
                {
                    characterBody.skillLocator.special.AddOneStock();
                }
            }
        }*/

        [ClientRpc]
        public void RpcAddSpecialStock()
        {
            if (hasAuthority && characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock && characterBody.skillLocator.special.skillDef == droneSkill)
            {
                characterBody.skillLocator.special.stock++;
                if (characterBody.skillLocator.special.stock == characterBody.skillLocator.special.maxStock)
                {
                    characterBody.skillLocator.special.rechargeStopwatch = 0f;
                }
            }
        }

        [Command]
        public void CmdUpdatePCBuff(int newCount)
        {
            if (NetworkServer.active)
            {
                int buffCount = characterBody.GetBuffCount(HANDBuffs.ParallelComputingBuff);
                if (buffCount < newCount)
                {
                    int diff = newCount - buffCount;
                    for (int i = 0; i < diff; i++)
                    {
                        characterBody.AddBuff(HANDBuffs.ParallelComputingBuff);
                    }
                }
                else if (buffCount > newCount)
                {
                    for (int i = 0; i < buffCount; i++)
                    {
                        characterBody.RemoveBuff(HANDBuffs.ParallelComputingBuff);
                    }
                    for (int i = 0; i < newCount; i++)
                    {
                        characterBody.AddBuff(HANDBuffs.ParallelComputingBuff);
                    }
                }


            }
        }

        private int oldPCCount = 0;
        private CharacterBody characterBody;
        private DroneStockPersist dronePersist;
        public static SkillDef droneSkill;
    }
}
