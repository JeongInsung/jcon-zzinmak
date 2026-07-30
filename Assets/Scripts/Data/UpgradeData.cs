using UnityEngine;

namespace Zzinmak.Data
{
    public enum UpgradeType { Damage, AttackSpeed, MoveSpeed, Heal, PickupRange, FanShot, ProjectileCount, OrbitingOrbs }

    [CreateAssetMenu(fileName = "UpgradeData", menuName = "Zzinmak/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeName = "Upgrade";

        [TextArea]
        public string description = "";

        public Sprite icon;
        public UpgradeType type;
        public float value = 0.1f;

        [Range(0f, 1f)]
        public float dropChance = 1f;

        public bool isUnique = false;

        [Tooltip("Max number of times this upgrade can be obtained. 0 = unlimited.")]
        public int maxStack = 0;
    }
}
