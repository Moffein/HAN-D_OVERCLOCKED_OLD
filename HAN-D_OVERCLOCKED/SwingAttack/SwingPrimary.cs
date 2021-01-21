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
                        }
                        else if (cb.isChampion) //deal less knockback against bosses if they're on the ground
                        {
                            force.x *= bossGroundedForceMult;
                            force.z *= bossGroundedForceMult;
                        }
                    }
                }

                //Scale force to match mass and reset current horizontal velocity to prevent launching at high attack speeds
                Rigidbody rb = cb.rigidbody;
                if (rb)
                {
                    //Debug.Log("Mass: " + rb.mass);
                    force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
                    //rb.velocity = new Vector3(0f, rb.velocity.y, 0f);//NEEDS TO BE FIXED IN MULTIPLAYER //nvm this is a feature now
                    //rb.angularVelocity = new Vector3(0f, rb.angularVelocity.y, 0f);//NEEDS TO BE FIXED IN MULTIPLAYER //nvm this is a feature now

                }
                /*CharacterMotor cm = cb.characterMotor;
                if (cm)//NEEDS TO BE FIXED IN MULTIPLAYER //nvm this is a feature now
                {
                    cm.velocity.x = 0f;
                    cm.velocity.z = 0f;
                    cm.rootMotion.x = 0f;
                    cm.rootMotion.z = 0f;
                }*/
            }
            return force;
        }
        public float maxForceScale = 6f;
        public float airborneHorizontalForceMult = 1f;
        public float flyingHorizontalForceMult = 1f;
        public float bossGroundedForceMult = 1f;
    }
}
