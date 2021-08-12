using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;

namespace HAND_OVERCLOCKED
{
    public class HANDSwingAttackPrimary : HANDSwingAttack
    {
        public override Vector3 ModifyForce(GameObject go, Vector3 force)
        {
            if (go)
            {
                CharacterBody cb = go.GetComponent<CharacterBody>();
                if (cb)
                {
                    if (cb.isFlying)
                    {
                        force.x *= this.flyingHorizontalForceMult;
                        force.z *= this.flyingHorizontalForceMult;
                    }
                    else if (cb.characterMotor)
                    {
                        if (!cb.characterMotor.isGrounded)    //Multiply launched enemy force
                        {
                            force.x *= this.airborneHorizontalForceMult;
                            force.z *= this.airborneHorizontalForceMult;
                            if (cb.isChampion) //deal less knockback against bosses if they're on the ground
                            {
                                force.x *= bossAirborneForceMult;
                                force.z *= bossAirborneForceMult;
                            }
                        }
                        else
                        {
                            if (cb.isChampion) //deal less knockback against bosses if they're on the ground
                            {
                                force.x *= bossGroundedForceMult;
                                force.z *= bossGroundedForceMult;
                            }
                        }
                    }

                    //Scale force to match mass
                    Rigidbody rb = cb.rigidbody;
                    if (rb)
                    {
                        force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
                    }
                }
            }
            return force;
        }
        public float maxForceScale = 6f;
        public float airborneHorizontalForceMult = 1f;
        public float flyingHorizontalForceMult = 1f;
        public float bossGroundedForceMult = 1f;
        public float bossAirborneForceMult = 1f;
    }
}
