using System;
using System.Collections.Generic;
using System.Text;
using HAND_OVERCLOCKED.Components;
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
                bool upwardsForce = false;
                //Use separate knockback values when dealing with airborne/grounded targets.
                CharacterBody cb = go.GetComponent<CharacterBody>();
                if (cb)
                {
                    if (cb.characterMotor && cb.characterMotor.isGrounded)
                    {
                        force += groundedLaunchForce * Vector3.up;
                        upwardsForce = true;
                    }
                    else
                    {
                        force += airborneLaunchForce * Vector3.up;
                    }
                }

                //Scale force to match mass
                Rigidbody rb = cb.rigidbody;
                if (rb)
                {
                    Debug.Log(rb.mass);
                    force *= Mathf.Min(Mathf.Max(rb.mass / 100f, 1f), upwardsForce? Mathf.Infinity : maxForceScale);
                }
            }
            return force;
        }
        public float maxForceScale = 6f;
        public float groundedLaunchForce = 0f;
        public float airborneLaunchForce = 0f;
    }
}
