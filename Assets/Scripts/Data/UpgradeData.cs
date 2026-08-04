using UnityEngine;

namespace Zzinmak.Data
{
    public enum UpgradeType { Damage, AttackSpeed, MoveSpeed, Heal, PickupRange, FanShot, ProjectileCount, OrbitingOrbs, Piercing, TacticalDrone, Turret }

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

        [Tooltip("Minimum player level required for this upgrade to appear as an option.")]
        public int minLevel = 1;

        [Tooltip("If eligible (level/unique/stack checks pass), always include this upgrade as one of the offered options instead of competing with dropChance/random selection.")]
        public bool guaranteed = false;
    }
}
