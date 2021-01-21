using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace HAND_OVERCLOCKED
{
    public class HANDSwingAttackSecondary : HANDSwingAttack
    {
        public override Vector3 ModifyForce(GameObject go, Vector3 force)
        {
            if (go)
            {
                //Use separate knockback values when dealing with airborne/grounded targets.
                CharacterBody cb = go.GetComponent<CharacterBody>();
                if (cb)
                {
                    if (cb.characterMotor && cb.characterMotor.isGrounded)
                    {
                        force += groundedLaunchForce * Vector3.up;
                    }
                    else
                    {
                        force += airborneLaunchForce * Vector3.up;
                        /*if (cb.characterMotor && cb.characterMotor.velocity.y > 0f)
                        {
                            cb.characterMotor.velocity.y = 0f;  //NEEDS TO BE FIXED IN MULTIPLAYER
                        }*/
                    }
                }

                //Scale force to match mass
                Rigidbody rb = cb.rigidbody;
                if (rb)
                {
                    //Debug.Log("Mass: " + rb.mass);
                    force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), maxForceScale);
                    /*if (rb.velocity.y > 0f)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);    //NEEDS TO BE FIXED IN MULTIPLAYER
                    }
                    if (rb.angularVelocity.y > 0f)
                    {
                        rb.angularVelocity = new Vector3(rb.angularVelocity.x, 0f, rb.angularVelocity.z);   //NEEDS TO BE FIXED IN MULTIPLAYER
                    }*/
                }
            }
            return force;
        }
        public float maxForceScale = 6f;
        public float groundedLaunchForce = 0f;
        public float airborneLaunchForce = 0f;
    }
}
