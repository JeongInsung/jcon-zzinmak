using UnityEngine;

namespace Zzinmak.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Zzinmak/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName = "Rifle";
        public GameObject projectilePrefab;
        public float damage = 5f;
        public float fireInterval = 0.5f;
        public float projectileSpeed = 12f;
        public int projectileCount = 1;
        public float range = 8f;
    }
}
