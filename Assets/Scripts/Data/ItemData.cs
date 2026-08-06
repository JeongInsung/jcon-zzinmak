using UnityEngine;

namespace Zzinmak.Data
{
    public enum ItemType { Heal, BonusExp, NightVision, Damage, AttackSpeed, MoveSpeed, GuardianAngel, LaserSight }

    [CreateAssetMenu(fileName = "ItemData", menuName = "Zzinmak/Item Data")]
    public class ItemData : ScriptableObject, ICardOption
    {
        public string itemName = "Item";

        [TextArea]
        public string description = "";

        public Sprite icon;
        public ItemType type;

        [Tooltip("Heal: fraction of max health restored. BonusExp: flat exp granted. NightVision: unused. Damage/AttackSpeed/MoveSpeed: percent increase, same units as the matching UpgradeType.")]
        public float value = 0.5f;

        public string CardTitle => itemName;
        public string CardDescription => description;
        public Sprite CardIcon => icon;
    }
}
