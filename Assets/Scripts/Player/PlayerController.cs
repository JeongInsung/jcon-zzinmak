using UnityEngine;
using UnityEngine.InputSystem;
using Zzinmak.Combat;
using Zzinmak.UI;

namespace Zzinmak.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;

        private Rigidbody2D body;
        private PlayerStats stats;
        private WeaponController weapon;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            weapon = GetComponent<WeaponController>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;

            if (input.sqrMagnitude < 0.01f)
            {
                input = ReadKeyboardFallback();
            }

            moveInput = input;

            if (animator != null)
            {
                animator.speed = moveInput.sqrMagnitude > 0.01f ? 1f : 0f;
            }

            Vector2 facing = Vector2.zero;
            if (weapon != null && weapon.CurrentTarget != null)
            {
                facing = (Vector2)weapon.CurrentTarget.position - (Vector2)transform.position;
            }
            else if (moveInput.sqrMagnitude > 0.01f)
            {
                facing = moveInput;
            }

            // Sprite is only ever flipped left/right, never rotated up/down.
            if (spriteRenderer != null && Mathf.Abs(facing.x) > 0.01f)
            {
                spriteRenderer.flipX = facing.x < 0f;
            }
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + moveInput * stats.moveSpeed * Time.fixedDeltaTime);
        }

        private Vector2 ReadKeyboardFallback()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;

            Vector2 dir = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) dir.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) dir.x += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) dir.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) dir.y -= 1f;

            return dir.sqrMagnitude > 1f ? dir.normalized : dir;
        }
    }
}
