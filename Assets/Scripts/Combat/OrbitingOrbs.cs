using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Core;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class OrbitingOrbs : MonoBehaviour
    {
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private int orbCount = 3;
        [SerializeField] private int levelsPerBonusOrb = 3;
        [SerializeField] private int maxOrbCount = 8;
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float spriteAngleOffset = 90f;

        private readonly List<Transform> orbs = new List<Transform>();
        private PlayerStats stats;
        private float currentAngle;
        private bool active;
        private int currentPlayerLevel = 1;
        private int levelAtActivation;
        private int bonusOrbsGranted;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
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

            if (!active || levelsPerBonusOrb <= 0) return;

            int levelsSinceActivation = currentPlayerLevel - levelAtActivation;
            int targetBonusOrbs = levelsSinceActivation / levelsPerBonusOrb;

            while (bonusOrbsGranted < targetBonusOrbs && orbs.Count < maxOrbCount)
            {
                SpawnOrb();
                bonusOrbsGranted++;
            }
        }

        public void Activate()
        {
            if (active) return;
            active = true;
            levelAtActivation = currentPlayerLevel;

            int initialOrbCount = Mathf.Min(orbCount, maxOrbCount);
            for (int i = 0; i < initialOrbCount; i++)
            {
                SpawnOrb();
            }
        }

        private void SpawnOrb()
        {
            var orbObj = Instantiate(orbPrefab, transform.position, Quaternion.identity, transform);
            var orbDamage = orbObj.GetComponent<OrbDamage>();
            if (orbDamage != null)
            {
                orbDamage.Initialize(this);
            }

            orbs.Add(orbObj.transform);
        }

        public float GetDamage()
        {
            return damage * (stats != null ? stats.damageMultiplier : 1f);
        }

        private void Update()
        {
            if (!active) return;

            currentAngle += rotationSpeed * Time.deltaTime;

            for (int i = 0; i < orbs.Count; i++)
            {
                float angle = currentAngle + (360f / orbs.Count) * i;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
                orbs[i].position = transform.position + offset;
                orbs[i].rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
            }
        }
    }
}
