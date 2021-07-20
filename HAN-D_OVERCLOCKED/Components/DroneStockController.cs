using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components
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
                    characterBody.skillLocator.special.stock = dronePersist.droneCount;
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
                if (dronePersist)
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
                    if (tc.body && (tc.body.bodyFlags & CharacterBody.BodyFlags.Mechanical) > 0 && tc.body != characterBody )
                    {
                        pcCount++;
                    }
                }
                if (pcCount != oldPCCount)
                {
                    CmdUpdatePCBuff(pcCount);
                }
                oldPCCount = pcCount;
            }
        }

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
            if (hasAuthority && characterBody.skillLocator.special.stock < characterBody.skillLocator.special.maxStock)
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
                if (characterBody.GetBuffCount(HANDContent.ParallelComputingBuff) != newCount)
                {
                    while (characterBody.HasBuff(HANDContent.ParallelComputingBuff))
                    {
                        characterBody.RemoveBuff(HANDContent.ParallelComputingBuff);
                    }
                    for (int i = 0; i < newCount; i++)
                    {
                        characterBody.AddBuff(HANDContent.ParallelComputingBuff);
                    }
                }
            }
        }

        private int oldPCCount = 0;
        private CharacterBody characterBody;
        private DroneStockPersist dronePersist;
    }
}
