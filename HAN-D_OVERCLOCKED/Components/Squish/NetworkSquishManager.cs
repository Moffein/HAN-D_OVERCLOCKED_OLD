using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components
{
    public class NetworkSquishManager : NetworkBehaviour
    {
        [Client]
        public void SquashEnemy(uint networkID)
        {
            if (this.hasAuthority)
            {
                CmdAddSquash(networkID);
            }
        }

        [Command]
        private void CmdAddSquash(uint networkID)
        {
            RpcAddSquash(networkID);
        }

        [ClientRpc]
        private void RpcAddSquash(uint networkID)
        {
            GameObject go = ClientScene.FindLocalObject(new NetworkInstanceId(networkID));
            if (go)
            {
                CharacterMaster cm = go.GetComponent<CharacterMaster>();
                if (cm)
                {
                    GameObject bodyObject = cm.GetBodyObject();
                    if (bodyObject)
                    {
                        SquashedComponent sq = bodyObject.GetComponent<SquashedComponent>();
                        if (sq)
                        {
                            sq.ResetGraceTimer();
                        }
                        else
                        {
                            bodyObject.AddComponent<SquashedComponent>();
                        }
                    }
                }
            }
        }
    }
}
