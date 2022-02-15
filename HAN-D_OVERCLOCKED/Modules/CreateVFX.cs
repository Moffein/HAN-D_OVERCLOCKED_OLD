using EntityStates.HANDOverclocked;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HAND_OVERCLOCKED.Modules
{
    class CreateVFX
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            CreateSlamEffect();
            RepairSwingEffect();
        }
        private static void CreateSlamEffect()
        {
            GameObject slamEffect = Resources.Load<GameObject>("prefabs/effects/impacteffects/ParentSlamEffect").InstantiateClone("HANDOVerclockedSlamImpactEffect", false);

            var particleParent = slamEffect.transform.Find("Particles");
            var debris = particleParent.Find("Debris, 3D");
            var debris2 = particleParent.Find("Debris");
            var sphere = particleParent.Find("Nova Sphere");

            debris.gameObject.SetActive(false);
            debris2.gameObject.SetActive(false);
            sphere.gameObject.SetActive(false);

            ShakeEmitter se = slamEffect.AddComponent<ShakeEmitter>();
            se.shakeOnStart = true;
            se.duration = 0.65f;
            se.scaleShakeRadiusWithLocalScale = false;
            se.radius = 30f;
            se.wave = new Wave()
            {
                amplitude = 7f,
                cycleOffset = 0f,
                frequency = 6f
            };

            slamEffect.GetComponent<EffectComponent>().soundName = "";
            //Play_parent_attack1_slam

            HANDContent.effectDefs.Add(new EffectDef(slamEffect));

            Slam2.impactEffectPrefab = slamEffect;
            SlamScepter.impactEffectPrefab = slamEffect;
        }

        //Credits to Enigma
        private static void RepairSwingEffect()
        {
            GameObject handSwingTrail = Resources.Load<GameObject>("prefabs/effects/handslamtrail").InstantiateClone("HANDOCSwingTrail", false);
            Transform HANDSwingTrailTransform = handSwingTrail.transform.Find("SlamTrail");

            var HANDrenderer = HANDSwingTrailTransform.GetComponent<Renderer>();

            if (HANDrenderer)
            {
                HANDrenderer.material = Resources.Load<GameObject>("prefabs/effects/LemurianBiteTrail").transform.Find("SwingTrail").GetComponent<Renderer>().material;
            }

            ShakeEmitter se = handSwingTrail.AddComponent<ShakeEmitter>();
            se.shakeOnEnable = false;
            se.shakeOnStart = true;
            se.duration = 0.25f;
            se.radius = 20f;
            se.scaleShakeRadiusWithLocalScale = false;
            se.amplitudeTimeDecay = true;
            se.wave = new Wave()
            {
                amplitude = 3f,
                cycleOffset = 0f,
                frequency = 4f
            };

            HANDContent.effectDefs.Add(new EffectDef(handSwingTrail));

            FullSwing.swingEffectPrefab = handSwingTrail;
            Slam2.swingEffectPrefab = handSwingTrail;
            SlamScepter.swingEffectPrefab = handSwingTrail;
        }
    }
}
