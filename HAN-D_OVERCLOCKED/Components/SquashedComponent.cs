using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

//https://github.com/GnomeModder/EnforcerMod/blob/master/EnforcerMod_VS/Nemesis/SquashedComponent.cs
//Credits to the EnforcerGang for lending me this code!
namespace HAND_OVERCLOCKED.Components
{
    public class SquashedComponent : MonoBehaviour
    {
        public float speed = 5f;
        //public float squashMult = 1f;
        private Vector3 originalScale;

        public void Awake()
        {
            originalScale = transform.localScale;
            transform.localScale = new Vector3(1.25f * transform.localScale.x, 0.05f * transform.localScale.y, 1.25f * transform.localScale.z); //originally 0.05 y mult

            StartCoroutine("EndSquash");
        }

        IEnumerator EndSquash()
        {
            yield return new WaitForSeconds(10f);

            float t = 0f;
            while (t < 1f)
            {
                t += speed * Time.deltaTime;
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t);

                yield return 0;
            }

            transform.localScale = originalScale;
            Destroy(this);

            yield return null;
        }
    }
}
