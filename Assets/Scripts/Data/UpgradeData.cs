using UnityEngine;

namespace Zzinmak.Data
{
    public enum UpgradeType { Damage, AttackSpeed, MoveSpeed, Heal, PickupRange }

    [CreateAssetMenu(fileName = "UpgradeData", menuName = "Zzinmak/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeName = "Upgrade";

        [TextArea]
        public string description = "";

        public Sprite icon;
        public UpgradeType type;
        public float value = 0.1f;
    }
}
