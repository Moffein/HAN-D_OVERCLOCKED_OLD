using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components
{
    public class DroneController : NetworkBehaviour//, IOnKilledOtherServerReceiver
    {
        public void Start()
        {
            characterBody.skillLocator.special.RemoveAllStocks();
            if (characterBody.master)
            {
                dronePersist = characterBody.master.gameObject.GetComponent<DronePersistComponent>();
                if (!dronePersist)
                {
                    dronePersist = characterBody.master.gameObject.AddComponent<DronePersistComponent>();
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

        private CharacterBody characterBody;
        private DronePersistComponent dronePersist;
    }
}
