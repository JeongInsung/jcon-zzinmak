using System.Collections;
using UnityEngine;
using Zzinmak.Enemies;

namespace Zzinmak.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlashbangGrenade : MonoBehaviour
    {
        [SerializeField] private float flightDuration = 0.35f;
        [SerializeField] private float arcHeight = 1f;
        [SerializeField] private float spinSpeed = 540f;
        [Tooltip("Zombies within this distance of the landing spot are stunned. Also drives the splash effect's visual size (see Explode).")]
        [SerializeField] private float stunRadius = 5f;
        [SerializeField] private float stunDuration = 3f;
        [Tooltip("Prefab's own visual radius should be 1 unit at scale 1, so scaling its transform by stunRadius matches the stun range exactly.")]
        [SerializeField] private GameObject splashEffectPrefab;

        public void Throw(Vector3 landingPosition)
        {
            StartCoroutine(FlightRoutine(landingPosition));
        }

        private IEnumerator FlightRoutine(Vector3 landingPosition)
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flightDuration);

                Vector3 pos = Vector3.Lerp(startPos, landingPosition, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                transform.position = pos;
                transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

                yield return null;
            }

            transform.position = landingPosition;
            Explode();
        }

        private void Explode()
        {
            if (splashEffectPrefab != null)
            {
                var effect = Instantiate(splashEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = Vector3.one * stunRadius;
            }

            StunZombiesInRange();
            Destroy(gameObject);
        }

        private void StunZombiesInRange()
        {
            var active = EnemyTracker.Active;
            float radiusSqr = stunRadius * stunRadius;

            for (int i = 0; i < active.Count; i++)
            {
                var enemy = active[i];
                if (enemy == null || !enemy.isActiveAndEnabled || enemy.IsDead) continue;
                if ((enemy.transform.position - transform.position).sqrMagnitude > radiusSqr) continue;

                enemy.Stun(stunDuration);
            }
        }
    }
}
