using UnityEngine;
using UnityEngine.InputSystem;
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
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;

            if (input.sqrMagnitude < 0.01f)
            {
                input = ReadKeyboardFallback();
            }

            moveInput = input;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, -angle);
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
