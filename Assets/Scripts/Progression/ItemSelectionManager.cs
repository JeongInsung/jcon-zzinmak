using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zzinmak.Combat;
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

        // PlaySaveBridge가 클라우드 세이브 병합 대상으로 그대로 참조하는 키라 public.
        public const string LaserSightUnlockedKey = "Item_LaserSightUnlocked";
        private const float LaserSightDamageBonus = 0.05f;
        public const string NightVisionUnlockedKey = "Item_NightVisionUnlocked";
        public const string DungeonLocatorUnlockedKey = "Item_DungeonLocatorUnlocked";

        [SerializeField] private LevelUpUI selectionUI;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerLevel playerLevel;
        [Tooltip("Off by default (dev keeps it on to see while working); NightVision item turns it on.")]
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Zzinmak.Player.LaserSight laserSight;
        [SerializeField] private DungeonDirectionIndicator dungeonIndicator;
        [SerializeField] private FlashbangSpawner flashbangSpawner;
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
            if (PlayerPrefs.GetInt(LaserSightUnlockedKey, 0) == 1)
            {
                if (laserSight != null) laserSight.Enable();
                if (playerStats != null) playerStats.IncreaseDamagePercent(LaserSightDamageBonus);
            }

            if (PlayerPrefs.GetInt(NightVisionUnlockedKey, 0) == 1)
            {
                EnableNightVision();
            }

            if (PlayerPrefs.GetInt(DungeonLocatorUnlockedKey, 0) == 1)
            {
                if (dungeonIndicator != null) dungeonIndicator.Unlock();
            }
        }

        private void EnableNightVision()
        {
            if (globalLight == null) return;

            globalLight.gameObject.SetActive(true);
            globalLight.enabled = true;
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
            // Close out of this screen and its shared LevelUpUI/state *before* applying the
            // item - some items (BonusExp) can themselves trigger LevelUpManager, which shows
            // its cards on that same shared panel. Applying first would have it clobber (or get
            // clobbered by) this screen's still-open cards and leave GameManager stuck mid-transition.
            selectionUI.Hide();
            GameManager.Instance.ExitLevelUp();

            ApplyItem((ItemData)option);

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.ExitBuilding();
            }
        }

        private void ApplyItem(ItemData item)
        {
            // 도감 등록용 "한 번이라도 획득함" 표시. 3종의 영구 효과 재적용용 PlayerPrefs
            // 키(LaserSightUnlockedKey 등)와는 별개 관심사라 전체 아이템 종류 여기서 균일하게 기록한다.
            ItemCollection.MarkCollected(item);
            PlayServices.Instance?.SaveToCloud();

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
                    EnableNightVision();
                    PlayerPrefs.SetInt(NightVisionUnlockedKey, 1);
                    PlayerPrefs.Save();
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
                case ItemType.DungeonLocator:
                    if (dungeonIndicator != null)
                    {
                        dungeonIndicator.Unlock();
                    }
                    PlayerPrefs.SetInt(DungeonLocatorUnlockedKey, 1);
                    PlayerPrefs.Save();
                    break;
                case ItemType.Flashbang:
                    if (flashbangSpawner != null)
                    {
                        flashbangSpawner.Activate();
                    }
                    break;
            }
        }
    }
}
