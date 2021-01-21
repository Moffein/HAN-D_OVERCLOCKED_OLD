using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components
{
    class HANDNetworkSounds : NetworkBehaviour
    {
        [ClientRpc]
        public void RpcPlayOverclockEnd()
        {
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
        }

        [ClientRpc]
        public void RpcPlayOverclockStart()
        {
            Util.PlaySound("Play_MULT_shift_start", base.gameObject);
        }

        private void OnDestroy()
        {
            Util.PlaySound("Play_MULT_shift_end", base.gameObject);
        }
    }
}
