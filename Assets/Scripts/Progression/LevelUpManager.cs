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
        private TacticalDrone tacticalDrone;
        private TurretSpawner turretSpawner;
        private int pendingLevelUps;
        private int currentPlayerLevel = 1;
        private readonly List<UpgradeData> currentOptions = new List<UpgradeData>();
        private readonly HashSet<UpgradeData> obtainedUnique = new HashSet<UpgradeData>();
        private readonly Dictionary<UpgradeData, int> upgradeCounts = new Dictionary<UpgradeData, int>();

        private void Awake()
        {
            orbitingOrbs = playerStats != null ? playerStats.GetComponent<OrbitingOrbs>() : null;
            tacticalDrone = playerStats != null ? playerStats.GetComponent<TacticalDrone>() : null;
            turretSpawner = playerStats != null ? playerStats.GetComponent<TurretSpawner>() : null;
        }

        private void OnEnable()
        {
            GameEvents.OnLevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelChanged -= HandleLevelChanged;
        }

        private void HandleLevelChanged(int level)
        {
            currentPlayerLevel = level;
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
            var guaranteedPool = new List<UpgradeData>();
            var randomPool = new List<UpgradeData>();

            foreach (var upgrade in allUpgrades)
            {
                if (currentPlayerLevel < upgrade.minLevel) continue;
                if (upgrade.isUnique && obtainedUnique.Contains(upgrade)) continue;
                if (upgrade.maxStack > 0 && GetUpgradeCount(upgrade) >= upgrade.maxStack) continue;

                if (upgrade.guaranteed)
                {
                    guaranteedPool.Add(upgrade);
                    continue;
                }

                if (Random.value > upgrade.dropChance) continue;
                randomPool.Add(upgrade);
            }

            var result = new List<UpgradeData>();
            for (int i = 0; i < guaranteedPool.Count && result.Count < count; i++)
            {
                result.Add(guaranteedPool[i]);
            }

            int remaining = Mathf.Min(count - result.Count, randomPool.Count);
            for (int i = 0; i < remaining; i++)
            {
                int index = Random.Range(0, randomPool.Count);
                result.Add(randomPool[index]);
                randomPool.RemoveAt(index);
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
                case UpgradeType.TacticalDrone:
                    if (tacticalDrone != null)
                    {
                        tacticalDrone.Activate();
                    }
                    break;
                case UpgradeType.Turret:
                    if (turretSpawner != null)
                    {
                        turretSpawner.PlaceTurret(upgrade.value);
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
