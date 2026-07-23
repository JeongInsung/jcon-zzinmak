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
    }
}
