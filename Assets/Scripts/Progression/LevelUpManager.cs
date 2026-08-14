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
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioClip levelUpClip;

        private OrbitingOrbs orbitingOrbs;
        private TacticalDrone tacticalDrone;
        private TurretSpawner turretSpawner;
        private ExpDogSpawner expDogSpawner;
        private MineSpawner mineSpawner;
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
            expDogSpawner = playerStats != null ? playerStats.GetComponent<ExpDogSpawner>() : null;
            mineSpawner = playerStats != null ? playerStats.GetComponent<MineSpawner>() : null;

            if (sfxAudioSource != null)
            {
                sfxAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnLevelChanged += HandleLevelChanged;
            GameEvents.OnSfxVolumeChanged += HandleSfxVolumeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelChanged -= HandleLevelChanged;
            GameEvents.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
        }

        private void HandleLevelChanged(int level)
        {
            currentPlayerLevel = level;
        }

        private void HandleSfxVolumeChanged(float volume)
        {
            if (sfxAudioSource != null)
            {
                sfxAudioSource.volume = volume;
            }
        }

        public void TriggerLevelUp()
        {
            pendingLevelUps++;

            if (sfxAudioSource != null && levelUpClip != null)
            {
                sfxAudioSource.PlayOneShot(levelUpClip);
            }

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
            levelUpUI.Show(currentOptions.ConvertAll(u => (ICardOption)u), HandleCardSelected);
        }

        private void HandleCardSelected(ICardOption option)
        {
            OnUpgradeSelected((UpgradeData)option);
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
                var picked = PickWeighted(randomPool);
                result.Add(picked);
                randomPool.Remove(picked);
            }

            return result;
        }

        // Weighted-without-replacement draw: each upgrade's dropChance acts as its
        // relative weight against the others still left in the pool, not an independent
        // per-item probability.
        private static UpgradeData PickWeighted(List<UpgradeData> pool)
        {
            float totalWeight = 0f;
            foreach (var upgrade in pool) totalWeight += Mathf.Max(0f, upgrade.dropChance);

            if (totalWeight <= 0f) return pool[Random.Range(0, pool.Count)];

            float roll = Random.value * totalWeight;
            float cumulative = 0f;
            foreach (var upgrade in pool)
            {
                cumulative += Mathf.Max(0f, upgrade.dropChance);
                if (roll <= cumulative) return upgrade;
            }

            return pool[pool.Count - 1];
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
                case UpgradeType.ExpDog:
                    if (expDogSpawner != null)
                    {
                        expDogSpawner.Activate(upgrade.value);
                    }
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
                case UpgradeType.Mine:
                    if (mineSpawner != null)
                    {
                        mineSpawner.Activate();
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
