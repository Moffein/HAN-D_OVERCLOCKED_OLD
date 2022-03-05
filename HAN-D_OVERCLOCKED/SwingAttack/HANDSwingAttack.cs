using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using R2API.Networking;
using UnityEngine.Networking;
using HandPlugin.Components;
using UnityEngine.Networking.Types;

namespace HandPlugin
{
    public class HANDSwingAttack
    {
        public int Fire()
        {
            NetworkCommands networkCommands = null;
            if (squish || stopMomentum)
            {
                networkCommands = this.attacker.GetComponent<NetworkCommands>();
                if (!networkCommands)
                {
                    squish = false;
                    stopMomentum = false;
                }
            }

            HealthComponent myHC = this.attacker.GetComponent<CharacterBody>().healthComponent;
            Collider[] array = Physics.OverlapBox(position, extents, orientation, LayerIndex.entityPrecise.mask);
            for (int i = 0; i < array.Length; i++)
            {
                Collider collider = array[i];
                HurtBox component = collider.GetComponent<HurtBox>();
                if (component)
                {
                    HealthComponent healthComponent = component.healthComponent;
                    if (healthComponent && healthComponent != myHC && (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, this.teamIndex)))
                    {
                        HANDSwingAttack.HitPoint hitPoint = default(HANDSwingAttack.HitPoint);

                        hitPoint.hurtBox = component;
                        hitPoint.hitPosition = collider.transform.position;
                        hitPoint.hitNormal = this.position - hitPoint.hitPosition;
                        if (!HANDSwingAttack.bestHitPoints.ContainsKey(healthComponent))
                        {
                            HANDSwingAttack.bestHitPoints[healthComponent] = hitPoint;
                        }
                    }
                }
            }
            HANDSwingAttack.HitPoint[] array2 = new HANDSwingAttack.HitPoint[HANDSwingAttack.bestHitPoints.Count];
            int num2 = 0;
            foreach (KeyValuePair<HealthComponent, HANDSwingAttack.HitPoint> keyValuePair in HANDSwingAttack.bestHitPoints)
            {
                array2[num2++] = keyValuePair.Value;
            }
            HANDSwingAttack.bestHitPoints.Clear();
            foreach (HANDSwingAttack.HitPoint hitPoint2 in array2)
            {
                if ((squish || stopMomentum) && hitPoint2.hurtBox.healthComponent.body && hitPoint2.hurtBox.healthComponent.body.masterObject)
                {
                    NetworkIdentity ni = hitPoint2.hurtBox.healthComponent.body.masterObject.GetComponent<NetworkIdentity>();
                    if (ni)
                    {
                        uint netID = ni.netId.Value;
                        if (squish)
                        {
                            networkCommands.SquashEnemy(netID);
                        }
                        if (stopMomentum)
                        {
                            networkCommands.StopMomentum(netID);
                        }
                    }
                }

                DamageInfo damageInfo = new DamageInfo()
                {
                    damage = this.baseDamage,
                    attacker = this.attacker,
                    inflictor = this.inflictor,
                    crit = this.crit,
                    force = this.force,
                    procChainMask = this.procChainMask,
                    procCoefficient = this.procCoefficient,
                    damageType = this.damageType,
                    damageColorIndex = this.damageColorIndex,
                    position = hitPoint2.hitPosition
                };
                damageInfo.ModifyDamageInfo(hitPoint2.hurtBox.damageModifier);
                damageInfo.force = ModifyForce(hitPoint2.hurtBox.healthComponent.gameObject, damageInfo.force);

                NetworkingHelpers.DealDamage(damageInfo, hitPoint2.hurtBox, true, true, true);

                EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
                {
                    origin = hitPoint2.hitPosition
                }, false);
            }
            return array2.Length;
        }

        public virtual Vector3 ModifyForce(GameObject go, Vector3 force)
        {
            return force;
        }

        private struct HitPoint
        {
            public HurtBox hurtBox;
            public Vector3 hitPosition;
            public Vector3 hitNormal;
        }
        public GameObject hitEffectPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/omniimpactvfx");
        public GameObject attacker;
        public GameObject inflictor;
        public TeamIndex teamIndex;
        public Vector3 position;
        public Vector3 extents;
        public Quaternion orientation;
        public float baseDamage;
        public Vector3 force;
        public bool crit;
        public bool squish = false;
        public bool stopMomentum = false;
        public DamageType damageType;
        public DamageColorIndex damageColorIndex;
        public ProcChainMask procChainMask;
        public float procCoefficient = 1f;

        public float radius = 8f;


        private static readonly Dictionary<HealthComponent, HANDSwingAttack.HitPoint> bestHitPoints = new Dictionary<HealthComponent, HANDSwingAttack.HitPoint>();
    }
}
