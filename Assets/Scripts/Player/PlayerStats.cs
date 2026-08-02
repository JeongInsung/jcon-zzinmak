using UnityEngine;

namespace Zzinmak.Player
{
    public class PlayerStats : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float moveSpeed = 4f;
        public float pickupRange = 1.5f;
        public float damageMultiplier = 1f;
        public float attackSpeedMultiplier = 1f;
        public float critChance = 0f;
        public float critMultiplier = 1.5f;
        public int extraProjectiles = 0;
        public int fanShotCount = 0;
        public int pierceCount = 0;

        public void IncreaseMoveSpeedPercent(float percent)
        {
            moveSpeed *= 1f + percent;
        }

        public void IncreaseDamagePercent(float percent)
        {
            damageMultiplier *= 1f + percent;
        }

        public void IncreaseAttackSpeedPercent(float percent)
        {
            attackSpeedMultiplier *= 1f + percent;
        }

        public void IncreasePickupRangePercent(float percent)
        {
            pickupRange *= 1f + percent;
        }

        public void AddFanShot(int amount)
        {
            fanShotCount = Mathf.Clamp(fanShotCount + amount, 0, 2);
        }

        public void AddProjectileCount(int amount)
        {
            extraProjectiles += amount;
        }

        public void AddPierceCount(int amount)
        {
            pierceCount += amount;
        }
    }
}
