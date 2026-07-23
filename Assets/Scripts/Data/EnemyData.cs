using UnityEngine;

namespace Zzinmak.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Zzinmak/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName = "Zombie";
        public float baseHealth = 10f;
        public float moveSpeed = 2f;
        public float contactDamage = 5f;
        public float contactDamageInterval = 1f;
        public int expReward = 1;
    }
}
