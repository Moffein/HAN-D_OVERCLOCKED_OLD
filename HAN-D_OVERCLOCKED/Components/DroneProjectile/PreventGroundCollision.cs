using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components.DroneProjectile
{
    class PreventGroundCollision : NetworkBehaviour
    {
        public void Awake()
        {
            if (NetworkServer.active)
            {
                stick = base.GetComponent<ProjectileStickOnImpact>();
                previousPos = this.transform.position;
            }
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (stick && !stick.stuck)
                {
                    if (previousPos == this.transform.position)
                    {
                        this.transform.rotation = Quaternion.Inverse(this.transform.rotation);
                    }
                    previousPos = this.transform.position;
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        private Vector3 previousPos;
        private ProjectileStickOnImpact stick;
    }
}
