using UnityEngine;

namespace Zzinmak.Data
{
    // Shared by any ScriptableObject data (UpgradeData, ItemData, ...) that wants to be
    // displayed and picked through the existing card selection UI (LevelUpUI/CardInfo).
    public interface ICardOption
    {
        string CardTitle { get; }
        string CardDescription { get; }
        Sprite CardIcon { get; }
    }
}
