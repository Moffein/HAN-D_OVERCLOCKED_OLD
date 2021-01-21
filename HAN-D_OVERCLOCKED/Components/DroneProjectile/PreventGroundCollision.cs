using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
                stopwatch = 0f;
                distanceTravelled = 0f;
                stuckCounter = 0;
                intangibleStopwatch = 0f;
            }
        }

        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {

                if (stick && !stick.stuck)
                {
                    if (intangibleStopwatch > 0f)
                    {
                        intangibleStopwatch -= Time.fixedDeltaTime;
                        if (intangibleStopwatch <= 0f)
                        {
                            this.gameObject.layer = LayerIndex.projectile.intVal;
                        }
                    }
                    else
                    {
                        distanceTravelled += Mathf.Abs((this.transform.position - previousPos).magnitude);
                        stopwatch += Time.fixedDeltaTime;
                        if (stopwatch > 0.15f)
                        {
                            stopwatch -= 0.15f;
                            //Debug.Log("Distance Travelled: " + distanceTravelled);
                            if (distanceTravelled < 0.02f)
                            {
                                stuckCounter++;
                            }
                            distanceTravelled = 0f;
                        }

                        if (previousPos == this.transform.position || stuckCounter > 1)
                        {
                            stuckCounter = 0;
                            this.transform.rotation = Quaternion.Inverse(this.transform.rotation);
                            intangibleStopwatch = 0.15f;
                            this.gameObject.layer = LayerIndex.entityPrecise.intVal;

                            stopwatch = 0f;
                            distanceTravelled = 0f;
                        }
                    }
                    previousPos = this.transform.position;
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        private float intangibleStopwatch;
        private int stuckCounter;
        private float distanceTravelled;
        private float stopwatch;
        private Vector3 previousPos;
        private ProjectileStickOnImpact stick;
    }
}
