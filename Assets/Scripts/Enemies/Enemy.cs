using System;
using System.Collections;
using UnityEngine;
using Zzinmak.Combat;
using Zzinmak.Core;
using Zzinmak.Data;
using Zzinmak.Environment;
using Zzinmak.Progression;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private GameObject expGemPrefab;
        [Tooltip("Seconds to wait after death before returning to the pool. 0 = return immediately (default zombie behavior). Set >0 to let a death animation play first.")]
        [SerializeField] private float deathReturnDelay = 0f;
        [Tooltip("Set true if the source art faces left by default (most sprites here face right). Flips the flipX mapping so the enemy still visually faces the direction it's moving/chasing.")]
        [SerializeField] private bool spriteFacesLeftByDefault = false;
        [SerializeField] private Transform shadow;

        private static readonly Collider2D[] ObstacleOverlapBuffer = new Collider2D[8];

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer shadowRenderer;
        private HitFlash hitFlash;
        private float shadowBaseX;
        private float obstacleCheckRadius = 0.3f;
        private Transform target;
        private EnemyData data;
        private float currentHealth;
        private float maxHealth;
        private float lastContactDamageTime;
        private float stunEndTime = -1f;

        public event Action OnDeath;
        public event Action<float, float> OnHealthChanged;

        public Transform Target => target;
        public bool IsDead { get; private set; }
        public bool CanMove { get; set; } = true;
        public bool IsStunned => Time.time < stunEndTime;

        // Freezes movement, contact damage, and (via IsStunned checks in boss scripts)
        // attack initiation for the given duration. Stacking calls extend rather than
        // shorten/refresh-shorten the remaining stun, so overlapping flashbangs don't
        // cut an earlier stun short.
        public void Stun(float duration)
        {
            if (IsDead) return;

            stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitFlash = GetComponent<HitFlash>();

            if (shadow != null)
            {
                shadowRenderer = shadow.GetComponent<SpriteRenderer>();
                shadowBaseX = shadow.localPosition.x;
            }

            var bodyCollider = GetComponent<CircleCollider2D>();
            if (bodyCollider != null)
            {
                obstacleCheckRadius = bodyCollider.radius * transform.lossyScale.x;
            }
        }

        private void OnEnable()
        {
            EnemyTracker.Register(this);
        }

        private void OnDisable()
        {
            EnemyTracker.Unregister(this);
        }

        public void Initialize(EnemyData enemyData, Transform playerTarget, float healthMultiplier)
        {
            data = enemyData;
            target = playerTarget;
            maxHealth = data.baseHealth * healthMultiplier;
            currentHealth = maxHealth;
            lastContactDamageTime = -data.contactDamageInterval;
            IsDead = false;
            CanMove = true;
            stunEndTime = -1f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void FixedUpdate()
        {
            if (target == null || data == null || IsDead || !CanMove || IsStunned) return;

            Vector2 dir = ((Vector2)target.position - body.position).normalized;

            body.MovePosition(ResolveMovement(body.position, dir));

            UpdateFacing(dir.x);
        }

        // Angles to steer toward on either side of the direct path, tried nearest-first.
        // 15 deg increment out to 150 deg gives a smooth turn around most building
        // corners while still bailing out (returning to "stay put") if fully boxed in.
        private static readonly float[] SteerAngles =
        {
            15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f, 75f, -75f,
            90f, -90f, 105f, -105f, 120f, -120f, 135f, -135f, 150f, -150f
        };

        // Buildings aren't on their own physics layer, so blocking is done by name:
        // BuildingColliderTool.cs generates a non-trigger "fw" ("footwall") child under
        // each building for exactly this purpose. Resolving X/Y independently made the
        // enemy grind along the wall face when approaching at an angle, so instead the
        // direct path is tried first and, if blocked, progressively wider angles to
        // either side until one is clear - steering around the obstacle instead of
        // sliding along it. Most enemies take the direct path every step (one overlap
        // check); the angle scan only runs for the minority currently pressed against
        // something, and stops at the first clear angle rather than checking all 20.
        private Vector2 ResolveMovement(Vector2 current, Vector2 dir)
        {
            float step = data.moveSpeed * Time.fixedDeltaTime;

            Vector2 direct = current + dir * step;
            if (!IsBlockedByBuilding(direct)) return direct;

            foreach (float angle in SteerAngles)
            {
                Vector2 steeredDir = Quaternion.Euler(0f, 0f, angle) * dir;
                Vector2 candidate = current + steeredDir * step;
                if (!IsBlockedByBuilding(candidate)) return candidate;
            }

            return current;
        }

        private bool IsBlockedByBuilding(Vector2 position)
        {
            return BuildingCollision.OverlapsBuilding(position, obstacleCheckRadius, ObstacleOverlapBuffer);
        }

        // Movement (and the facing update tied to it) pauses whenever CanMove is false,
        // e.g. mid-attack. Callers that lock movement for an attack should call this first
        // so the enemy still snaps to face the target instead of freezing mid-turn.
        public void FaceTarget()
        {
            if (target == null) return;

            UpdateFacing(target.position.x - transform.position.x);
        }

        private void UpdateFacing(float dirX)
        {
            if (spriteRenderer == null || Mathf.Abs(dirX) <= 0.01f) return;

            bool shouldFlip = dirX < 0f;
            bool flipX = spriteFacesLeftByDefault ? !shouldFlip : shouldFlip;
            spriteRenderer.flipX = flipX;

            if (shadowRenderer != null) shadowRenderer.flipX = flipX;

            if (shadow != null)
            {
                Vector3 shadowPos = shadow.localPosition;
                shadowPos.x = flipX ? -shadowBaseX : shadowBaseX;
                shadow.localPosition = shadowPos;
            }
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            currentHealth -= amount;
            OnHealthChanged?.Invoke(Mathf.Max(currentHealth, 0f), maxHealth);
            hitFlash?.Flash();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            GameManager.Instance.AddKill();

            if (expGemPrefab != null)
            {
                var gemObj = ObjectPool.Instance.Get(expGemPrefab, transform.position, Quaternion.identity);
                var gem = gemObj.GetComponent<ExpGem>();
                gem.SetValue(data.expReward);
            }

            OnDeath?.Invoke();

            if (deathReturnDelay > 0f)
            {
                StartCoroutine(ReturnToPoolAfterDelay(deathReturnDelay));
            }
            else
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ObjectPool.Instance.Return(gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (data == null || IsDead || IsStunned) return;
            if (!other.CompareTag("Player")) return;
            if (Time.time - lastContactDamageTime < data.contactDamageInterval) return;

            lastContactDamageTime = Time.time;
            var health = other.GetComponent<Player.PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(data.contactDamage);
            }
        }
    }
}
