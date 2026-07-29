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
        [SerializeField] private Transform muzzle;

        private Rigidbody2D body;
        private PlayerStats stats;
        private WeaponController weapon;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;
        private float muzzleOffsetX;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            weapon = GetComponent<WeaponController>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // The muzzle is a child transform positioned for the unflipped (facing right)
            // sprite. Since facing is done via SpriteRenderer.flipX rather than rotation,
            // the muzzle's own local X must be mirrored by hand to stay on the gun side.
            if (muzzle != null) muzzleOffsetX = Mathf.Abs(muzzle.localPosition.x);
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
            if (Mathf.Abs(facing.x) > 0.01f)
            {
                bool flipped = facing.x < 0f;

                if (spriteRenderer != null) spriteRenderer.flipX = flipped;

                if (muzzle != null)
                {
                    Vector3 localPos = muzzle.localPosition;
                    localPos.x = flipped ? -muzzleOffsetX : muzzleOffsetX;
                    muzzle.localPosition = localPos;
                }
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
