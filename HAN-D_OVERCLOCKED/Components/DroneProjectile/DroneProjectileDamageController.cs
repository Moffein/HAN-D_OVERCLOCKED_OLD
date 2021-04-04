using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace HAND_OVERCLOCKED.Components.DroneProjectile
{
    class DroneProjectileDamageController : NetworkBehaviour
    {
        [ClientRpc]
        private void RpcPlayDrillSound()
        {
            Util.PlaySound("Play_HOC_Drill", this.gameObject);
        }

        public void Awake()
        {
            if (NetworkServer.active)
            {
                stick = this.gameObject.GetComponent<ProjectileStickOnImpact>();
                stopwatch = 0f;
                damageTicks = 0;
                firstHit = true;
                projectileDamage = this.gameObject.GetComponent<ProjectileDamage>();
                healPerTick = totalHeal / damageTicksTotal;
                projectileController = this.gameObject.GetComponent<ProjectileController>();
            }
        }

        public void OnDestroy()
        {
            if (bleedEffect)
            {
                Destroy(bleedEffect);
            }
            if (NetworkServer.active)
            {
                if (damageTicks < damageTicksTotal && ownerHealthComponent)
                {
                    HealOrb healOrb = new HealOrb();
                    healOrb.origin = this.transform.position;
                    healOrb.target = ownerHealthComponent.body.mainHurtBox;
                    healOrb.healValue = ownerHealthComponent.body.maxHealth * healPerTick * (damageTicksTotal - damageTicks) * 0.5f;
                    healOrb.overrideDuration = 0.3f;
                    OrbManager.instance.AddOrb(healOrb);
                }
            }
        }

        public void FixedUpdate()
        {
            if (!firstHit && !bleedEffect)
            {
                bleedEffect = UnityEngine.Object.Instantiate<GameObject>(bleedEffectPrefab, this.transform);
            }
            if (NetworkServer.active)
            {
                if (projectileController && !owner)
                {
                    owner = projectileController.owner;
                    if (owner)
                    {
                        ownerHealthComponent = projectileController.owner.GetComponent<HealthComponent>();
                        TeamComponent tc = owner.GetComponent<TeamComponent>();
                        teamIndex = tc.teamIndex;
                    }
                }
                if (stick.stuck)
                {
                    if (stick.victim)
                    {
                        if (!victimHealthComponent)
                        {
                            victimHealthComponent = stick.victim.GetComponent<HealthComponent>();
                            if (!victimHealthComponent)
                            {
                                Destroy(this.gameObject);
                                return;
                            }
                        }
                        else if (victimHealthComponent && !victimHealthComponent.alive)
                        {
                            Destroy(this.gameObject);
                            return;
                        }

                        stopwatch += Time.fixedDeltaTime;
                        if (stopwatch > damageTimer)
                        {
                            damageTicks++;
                            if (damageTicks > damageTicksTotal)
                            {
                                Destroy(this.gameObject);
                            }

                            RpcPlayDrillSound();
                            if (ownerHealthComponent)
                            {
                                HealOrb healOrb = new HealOrb();
                                healOrb.origin = this.transform.position;
                                healOrb.target = ownerHealthComponent.body.mainHurtBox;
                                healOrb.healValue = ownerHealthComponent.body.maxHealth * healPerTick;
                                healOrb.overrideDuration = 0.3f;
                                OrbManager.instance.AddOrb(healOrb);
                            }

                            if (victimHealthComponent && projectileDamage)
                            {
                                if (firstHit)
                                {
                                    firstHit = false;
                                    if (victimHealthComponent.body)
                                    {
                                        if (victimHealthComponent.body.teamComponent && victimHealthComponent.body.teamComponent.teamIndex == teamIndex)
                                        {
                                            victimHealthComponent.body.AddTimedBuff(HANDContent.DroneBuff, (float)damageTicksTotal * damageTimer);
                                        }
                                        else
                                        {
                                            victimHealthComponent.body.AddTimedBuff(HANDContent.DroneDebuff, (float)damageTicksTotal * damageTimer);
                                        }
                                    }
                                }

                                if (victimHealthComponent.body && victimHealthComponent.body.teamComponent && victimHealthComponent.body.teamComponent.teamIndex == teamIndex)
                                {
                                    HealOrb healOrb = new HealOrb();
                                    healOrb.origin = this.transform.position;
                                    healOrb.target = victimHealthComponent.body.mainHurtBox;
                                    healOrb.healValue = victimHealthComponent.body.maxHealth * healPerTick;
                                    healOrb.overrideDuration = 0.3f;
                                    OrbManager.instance.AddOrb(healOrb);
                                }
                                else
                                {
                                    victimHealthComponent.TakeDamage(new DamageInfo
                                    {
                                        attacker = owner,
                                        inflictor = owner,
                                        damage = projectileDamage.damage / (float)damageTicksTotal,
                                        damageColorIndex = DamageColorIndex.Default,
                                        damageType = DamageType.Generic,
                                        crit = projectileDamage.crit,
                                        dotIndex = DotController.DotIndex.None,
                                        force = projectileDamage.force * Vector3.down,
                                        position = this.transform.position,
                                        procChainMask = default(ProcChainMask),
                                        procCoefficient = procCoefficient
                                    });

                                    GlobalEventManager.instance.OnHitEnemy(new DamageInfo
                                    {
                                        attacker = owner,
                                        inflictor = owner,
                                        damage = projectileDamage.damage / (float)damageTicksTotal,
                                        damageColorIndex = DamageColorIndex.Default,
                                        damageType = DamageType.Generic,
                                        crit = projectileDamage.crit,
                                        dotIndex = DotController.DotIndex.None,
                                        force = projectileDamage.force * Vector3.down,
                                        position = this.transform.position,
                                        procChainMask = default(ProcChainMask),
                                        procCoefficient = procCoefficient
                                    }, victimHealthComponent.gameObject);
                                }
                            }

                            stopwatch -= damageTimer;
                        }
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }    
                }
            }
        }

        public static float procCoefficient = 0.5f;
        public static float damageTimer = 0.5f;
        public static uint damageTicksTotal = 8;
        public static float totalHeal = 0.085f;
        public static GameObject bleedEffectPrefab = Resources.Load<GameObject>("Prefabs/BleedEffect");

        private float stopwatch;
        private ProjectileStickOnImpact stick;
        private uint damageTicks;

        [SyncVar]
        private bool firstHit;

        private GameObject bleedEffect;
        private float healPerTick;
        private GameObject owner;
        private TeamIndex teamIndex;
        private ProjectileController projectileController;
        private ProjectileDamage projectileDamage;
        private HealthComponent ownerHealthComponent;
        private HealthComponent victimHealthComponent;
    }
}
