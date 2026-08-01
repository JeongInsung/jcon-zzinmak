using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Combat;
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

        private OrbitingOrbs orbitingOrbs;
        private int pendingLevelUps;
        private readonly List<UpgradeData> currentOptions = new List<UpgradeData>();
        private readonly HashSet<UpgradeData> obtainedUnique = new HashSet<UpgradeData>();
        private readonly Dictionary<UpgradeData, int> upgradeCounts = new Dictionary<UpgradeData, int>();

        private void Awake()
        {
            orbitingOrbs = playerStats != null ? playerStats.GetComponent<OrbitingOrbs>() : null;
        }

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
            var pool = new List<UpgradeData>();
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade.isUnique && obtainedUnique.Contains(upgrade)) continue;
                if (upgrade.maxStack > 0 && GetUpgradeCount(upgrade) >= upgrade.maxStack) continue;
                if (Random.value > upgrade.dropChance) continue;

                pool.Add(upgrade);
            }

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
                case UpgradeType.FanShot:
                    playerStats.AddFanShot(Mathf.RoundToInt(upgrade.value));
                    break;
                case UpgradeType.ProjectileCount:
                    playerStats.AddProjectileCount(Mathf.RoundToInt(upgrade.value));
                    break;
                case UpgradeType.Piercing:
                    playerStats.AddPierceCount(Mathf.RoundToInt(upgrade.value));
                    break;
                case UpgradeType.OrbitingOrbs:
                    if (orbitingOrbs != null)
                    {
                        orbitingOrbs.Activate();
                    }
                    break;
            }

            upgradeCounts[upgrade] = GetUpgradeCount(upgrade) + 1;

            if (upgrade.isUnique)
            {
                obtainedUnique.Add(upgrade);
            }
        }

        private int GetUpgradeCount(UpgradeData upgrade)
        {
            return upgradeCounts.TryGetValue(upgrade, out int count) ? count : 0;
        }
    }
}
