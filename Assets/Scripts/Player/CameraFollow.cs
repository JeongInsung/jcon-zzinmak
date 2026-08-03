using UnityEngine;
using Zzinmak.Environment;

namespace Zzinmak.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            if (cam != null && cam.orthographic && MapBounds.Instance != null)
            {
                Vector2 halfExtents = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
                Vector2 clamped = MapBounds.Instance.ClampCameraCenter(position, halfExtents);
                position.x = clamped.x;
                position.y = clamped.y;
            }

            transform.position = position;
        }
    }
}
