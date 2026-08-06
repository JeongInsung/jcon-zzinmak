using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Environment;
using Zzinmak.Player;
using Zzinmak.UI;

namespace Zzinmak.Progression
{
    public class ItemSelectionManager : MonoBehaviour
    {
        public static ItemSelectionManager Instance { get; private set; }

        private const string LaserSightUnlockedKey = "Item_LaserSightUnlocked";
        private const float LaserSightDamageBonus = 0.05f;

        [SerializeField] private LevelUpUI selectionUI;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerLevel playerLevel;
        [Tooltip("Off by default (dev keeps it on to see while working); NightVision item turns it on.")]
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Zzinmak.Player.LaserSight laserSight;
        [SerializeField] private ItemData[] allItems;
        [SerializeField] private int optionCount = 3;

        private readonly List<ItemData> currentOptions = new List<ItemData>();

        private void Awake()
        {
            Instance = this;
            ApplyPersistentUnlocks();
        }

        // Re-applies unlocks that were saved to PlayerPrefs by a previous run, so they carry
        // over into a fresh game start instead of needing to be picked up again.
        private void ApplyPersistentUnlocks()
        {
            if (PlayerPrefs.GetInt(LaserSightUnlockedKey, 0) != 1) return;

            if (laserSight != null) laserSight.Enable();
            if (playerStats != null) playerStats.IncreaseDamagePercent(LaserSightDamageBonus);
        }

        public void Open()
        {
            GameManager.Instance.EnterLevelUp();

            currentOptions.Clear();
            currentOptions.AddRange(PickRandomItems(optionCount));
            selectionUI.Show(currentOptions.ConvertAll(i => (ICardOption)i), HandleItemSelected);
        }

        private List<ItemData> PickRandomItems(int count)
        {
            var pool = new List<ItemData>(allItems);

            if (playerHealth != null && playerHealth.HasGuardianCharm)
            {
                pool.RemoveAll(item => item.type == ItemType.GuardianAngel);
            }

            var result = new List<ItemData>();

            int take = Mathf.Min(count, pool.Count);
            for (int i = 0; i < take; i++)
            {
                int index = Random.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private void HandleItemSelected(ICardOption option)
        {
            ApplyItem((ItemData)option);
            selectionUI.Hide();
            GameManager.Instance.ExitLevelUp();

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.ExitBuilding();
            }
        }

        private void ApplyItem(ItemData item)
        {
            switch (item.type)
            {
                case ItemType.Heal:
                    if (playerHealth != null && playerStats != null)
                    {
                        playerHealth.Heal(playerStats.maxHealth * item.value);
                    }
                    break;
                case ItemType.BonusExp:
                    if (playerLevel != null)
                    {
                        playerLevel.AddExp(item.value);
                    }
                    break;
                case ItemType.NightVision:
                    if (globalLight != null)
                    {
                        globalLight.gameObject.SetActive(true);
                        globalLight.enabled = true;
                    }
                    break;
                case ItemType.Damage:
                    if (playerStats != null)
                    {
                        playerStats.IncreaseDamagePercent(item.value);
                    }
                    break;
                case ItemType.AttackSpeed:
                    if (playerStats != null)
                    {
                        playerStats.IncreaseAttackSpeedPercent(item.value);
                    }
                    break;
                case ItemType.MoveSpeed:
                    if (playerStats != null)
                    {
                        playerStats.IncreaseMoveSpeedPercent(item.value);
                    }
                    break;
                case ItemType.GuardianAngel:
                    if (playerHealth != null)
                    {
                        playerHealth.ArmGuardianCharm();
                    }
                    break;
                case ItemType.LaserSight:
                    if (laserSight != null)
                    {
                        laserSight.Enable();
                    }
                    if (playerStats != null)
                    {
                        playerStats.IncreaseDamagePercent(LaserSightDamageBonus);
                    }
                    PlayerPrefs.SetInt(LaserSightUnlockedKey, 1);
                    PlayerPrefs.Save();
                    break;
            }
        }
    }
}
