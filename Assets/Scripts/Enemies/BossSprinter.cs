using System.Collections;
using UnityEngine;
using Zzinmak.Player;

namespace Zzinmak.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossSprinter : MonoBehaviour
    {
        [Header("Dash attack")]
        [SerializeField] private float dashTriggerRange = 6f;
        [SerializeField] private float telegraphDuration = 1f;
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.35f;
        [SerializeField] private float dashDamage = 25f;
        [SerializeField] private float dashCooldown = 4f;
        [SerializeField] private Color telegraphColor = new Color(1f, 1f, 0.2f, 1f);
        [SerializeField] private float telegraphScalePunch = 1.25f;

        [Header("Path indicator")]
        [SerializeField] private LineRenderer pathLine;

        [Header("UI")]
        [SerializeField] private BossHealthBar healthBar;

        private Enemy enemy;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Vector3 originalScale;
        private bool isDashing;
        private bool hasDealtDashDamage;
        private float dashTimer;
        private float damageMultiplier = 1f;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
            originalScale = transform.localScale;
            if (pathLine == null) pathLine = GetComponent<LineRenderer>();
            if (pathLine != null) pathLine.enabled = false;
        }

        public void ApplyDamageMultiplier(float multiplier)
        {
            damageMultiplier = Mathf.Max(0.01f, multiplier);
        }

        private void OnEnable()
        {
            isDashing = false;
            hasDealtDashDamage = false;
            dashTimer = 0f;
            damageMultiplier = 1f;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
            if (pathLine != null) pathLine.enabled = false;
            enemy.OnDeath += HandleDeath;
            enemy.OnHealthChanged += HandleHealthChanged;
            if (healthBar != null) healthBar.Show();
        }

        private void OnDisable()
        {
            enemy.OnDeath -= HandleDeath;
            enemy.OnHealthChanged -= HandleHealthChanged;
            StopAllCoroutines();
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthBar != null) healthBar.SetFill(current, max);
        }

        private void Update()
        {
            if (isDashing || enemy.IsDead) return;

            dashTimer -= Time.deltaTime;

            var target = enemy.Target;
            if (target == null || dashTimer > 0f) return;

            float distance = Vector2.Distance(transform.position, target.position);
            if (distance <= dashTriggerRange)
            {
                dashTimer = dashCooldown;
                StartCoroutine(DashAttack(target));
            }
        }

        private IEnumerator DashAttack(Transform target)
        {
            isDashing = true;
            enemy.CanMove = false;

            // Lock the aim and path in at the moment the telegraph starts - the dash
            // later follows this exact line, it does not re-aim at the target.
            Vector2 dir = target != null
                ? ((Vector2)target.position - (Vector2)transform.position).normalized
                : Vector2.right;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)(dir * dashSpeed * dashDuration);

            if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
            transform.localScale = originalScale * telegraphScalePunch;

            if (pathLine != null)
            {
                pathLine.enabled = true;
                pathLine.SetPosition(0, startPos);
                pathLine.SetPosition(1, endPos);
            }

            yield return new WaitForSeconds(telegraphDuration);

            if (pathLine != null) pathLine.enabled = false;

            if (enemy.IsDead)
            {
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
                transform.localScale = originalScale;
                yield break;
            }

            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            transform.localScale = originalScale;

            hasDealtDashDamage = false;
            float elapsed = 0f;
            while (elapsed < dashDuration && !enemy.IsDead)
            {
                body.MovePosition(body.position + dir * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            enemy.CanMove = true;
            isDashing = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isDashing || hasDealtDashDamage) return;
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(dashDamage * damageMultiplier);
                hasDealtDashDamage = true;
            }
        }

        private void HandleDeath()
        {
            StopAllCoroutines();
            isDashing = false;
            enemy.CanMove = false;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
            if (pathLine != null) pathLine.enabled = false;
            if (healthBar != null) healthBar.Hide();
        }
    }
}
