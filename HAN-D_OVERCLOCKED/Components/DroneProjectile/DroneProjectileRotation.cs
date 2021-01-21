using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2.Projectile;

namespace HAND_OVERCLOCKED.Components.DroneProjectile
{
    class DroneProjectileRotation : MonoBehaviour
    {
        public void Awake()
        {
            projectileImpactExplosion = base.gameObject.GetComponent<ProjectileImpactExplosion>();
        }

        public void FixedUpdate()
        {
            if (projectileImpactExplosion.hasImpact)
            {
                Destroy(this);
            }
        }

        public void Update()
        {
            base.transform.rotation = Quaternion.AngleAxis(720f * Time.deltaTime, Vector3.right) * base.transform.rotation;
        }

        private ProjectileImpactExplosion projectileImpactExplosion;
    }
}
