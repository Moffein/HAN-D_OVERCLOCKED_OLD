using RoR2;
using System.Collections;
using UnityEngine;

namespace HandPlugin.Components
{
    public class MenuAnimComponent : MonoBehaviour
    {
        internal void OnEnable()
        {
            if (base.gameObject && base.transform.parent && base.gameObject.transform.parent.gameObject && base.gameObject.transform.parent.gameObject.name == "CharacterPad")
            {
                base.StartCoroutine(this.SelectSound());
            }
        }

        private IEnumerator SelectSound()
        {
            Util.PlaySound("Play_HOC_StartPunch", base.gameObject);

            Animator modelAnimator = base.gameObject.GetComponent<Animator>();
            int layerIndex = modelAnimator.GetLayerIndex("Gesture");
            modelAnimator.SetFloat("FullSwing.playbackRate", 1f);
            modelAnimator.CrossFadeInFixedTime("FullSwing1", 0.2f, layerIndex);
            modelAnimator.Update(0f);
            float length = modelAnimator.GetNextAnimatorStateInfo(layerIndex).length;
            modelAnimator.SetFloat("FullSwing.playbackRate", length / 1.5f);
            yield return new WaitForSeconds(0.4f);
            Util.PlaySound("Play_HOC_Punch", base.gameObject);
            yield return new WaitForSeconds(0.4f);
            modelAnimator.CrossFadeInFixedTime("FullSwing2", 0.2f, layerIndex);
            yield return new WaitForSeconds(0.4f);
            Util.PlaySound("Play_HOC_Punch", base.gameObject);
            yield break;
        }
    }
}
