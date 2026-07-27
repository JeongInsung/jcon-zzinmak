using System.Collections.Generic;
using UnityEngine;
using Zzinmak.Player;

namespace Zzinmak.Combat
{
    public class OrbitingOrbs : MonoBehaviour
    {
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private int orbCount = 3;
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float spriteAngleOffset = 90f;

        private readonly List<Transform> orbs = new List<Transform>();
        private PlayerStats stats;
        private float currentAngle;
        private bool active;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        public void Activate()
        {
            if (active) return;
            active = true;

            for (int i = 0; i < orbCount; i++)
            {
                var orbObj = Instantiate(orbPrefab, transform.position, Quaternion.identity, transform);
                var orbDamage = orbObj.GetComponent<OrbDamage>();
                if (orbDamage != null)
                {
                    orbDamage.Initialize(this);
                }

                orbs.Add(orbObj.transform);
            }
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
