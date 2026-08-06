using UnityEngine;
using Zzinmak.Environment;

namespace Zzinmak.Player
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        // followSpeed is calibrated against this frame time, so the smoothing matches the old
        // `followSpeed * Time.deltaTime` lerp exactly at 60 fps and holds that same response at
        // 30, 90 or 144 fps instead of tightening/loosening with the framerate.
        private const float ReferenceDeltaTime = 1f / 60f;

        // Floor sprites are placed as individual bilinear-filtered quads rather than a Tilemap, so
        // adjacent tiles don't always abut to sub-pixel precision. With the camera drifting by a
        // continuously varying fractional amount every frame, that seam lands on a different screen
        // pixel each frame and reads as a crawling line. Snapping the rendered camera position to
        // the floor texture's own texel grid (100 px/unit) makes the seam's screen position change
        // only when the camera crosses a whole texel, instead of every frame - the camera motion
        // itself still looks smooth, but the crawl is gone.
        private const float FloorTexelsPerUnit = 100f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            float retentionPerReferenceFrame = Mathf.Clamp01(1f - followSpeed * ReferenceDeltaTime);
            float t = 1f - Mathf.Pow(retentionPerReferenceFrame, Time.deltaTime / ReferenceDeltaTime);
            Vector3 position = Vector3.Lerp(transform.position, desiredPosition, t);

            if (cam != null && cam.orthographic && MapBounds.Instance != null)
            {
                Vector2 halfExtents = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
                Vector2 clamped = MapBounds.Instance.ClampCameraCenter(position, halfExtents);
                position.x = clamped.x;
                position.y = clamped.y;
            }

            position.x = Mathf.Round(position.x * FloorTexelsPerUnit) / FloorTexelsPerUnit;
            position.y = Mathf.Round(position.y * FloorTexelsPerUnit) / FloorTexelsPerUnit;

            transform.position = position;
        }
    }
}
