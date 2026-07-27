using UnityEngine;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    public class OrbDamage : MonoBehaviour
    {
        private OrbitingOrbs owner;

        public void Initialize(OrbitingOrbs orbitingOwner)
        {
            owner = orbitingOwner;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;
            if (owner == null) return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(owner.GetDamage());
            }
        }
    }
}
