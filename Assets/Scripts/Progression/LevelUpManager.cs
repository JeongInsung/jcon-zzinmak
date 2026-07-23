using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Player;
using Zzinmak.UI;

namespace Zzinmak.Progression
{
    public class LevelUpManager : MonoBehaviour
    {
        [SerializeField] private LevelUpUI levelUpUI;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private UpgradeData[] allUpgrades;

        private int pendingLevelUps;
        private readonly List<UpgradeData> currentOptions = new List<UpgradeData>();

        public void TriggerLevelUp()
        {
            pendingLevelUps++;
            if (pendingLevelUps == 1)
            {
                ShowNextLevelUp();
            }
        }

        private void ShowNextLevelUp()
        {
            GameManager.Instance.EnterLevelUp();

            currentOptions.Clear();
            currentOptions.AddRange(PickRandomUpgrades(3));
            levelUpUI.Show(currentOptions, OnUpgradeSelected);
        }

        private List<UpgradeData> PickRandomUpgrades(int count)
        {
            var pool = new List<UpgradeData>(allUpgrades);
            var result = new List<UpgradeData>();
            count = Mathf.Min(count, pool.Count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private void OnUpgradeSelected(UpgradeData upgrade)
        {
            ApplyUpgrade(upgrade);
            levelUpUI.Hide();
            GameManager.Instance.ExitLevelUp();

            pendingLevelUps--;
            if (pendingLevelUps > 0)
            {
                ShowNextLevelUp();
            }
        }

        private void ApplyUpgrade(UpgradeData upgrade)
        {
            switch (upgrade.type)
            {
                case UpgradeType.Heal:
                    playerHealth.Heal(playerStats.maxHealth * upgrade.value);
                    break;
                case UpgradeType.MoveSpeed:
                    playerStats.IncreaseMoveSpeedPercent(upgrade.value);
                    break;
                case UpgradeType.Damage:
                    playerStats.IncreaseDamagePercent(upgrade.value);
                    break;
                case UpgradeType.AttackSpeed:
                    playerStats.IncreaseAttackSpeedPercent(upgrade.value);
                    break;
                case UpgradeType.PickupRange:
                    playerStats.IncreasePickupRangePercent(upgrade.value);
                    break;
            }
        }
    }
}
