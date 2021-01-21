using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components
{
    public class DroneFollowerController : NetworkBehaviour
    {
        public void Awake()
        {
            characterBody = base.GetComponent<CharacterBody>();
            if (NetworkServer.active)
            {
                _droneCountServer = 0;
            }
        }

        public void FixedUpdate()
        {
            if (hasAuthority)
            {
                if (characterBody.skillLocator.special.stock != _droneCountServer)
                {
                    CmdUpdateDroneCount(characterBody.skillLocator.special.stock);
                }
            }
        }

        [Command]
        private void CmdUpdateDroneCount(int newCount)
        {
            _droneCountServer = newCount;
        }

        private CharacterBody characterBody;

        [SyncVar]
        private int _droneCountServer;
    }
}
