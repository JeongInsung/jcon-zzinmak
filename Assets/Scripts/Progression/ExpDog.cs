using UnityEngine;

namespace Zzinmak.Progression
{
    public class ExpDog : MonoBehaviour
    {
        [SerializeField] private float followSpeed = 6f;
        [SerializeField] private Vector2 followOffset = new Vector2(-0.6f, -0.3f);
        [SerializeField] private Transform shadow;

        public static ExpDog Instance { get; private set; }

        public float PickupRange { get; private set; }

        private Transform followTarget;
        private SpriteRenderer spriteRenderer;
        private SpriteRenderer shadowRenderer;
        private float shadowBaseX;

        // followSpeed is calibrated against this frame time, so the smoothing matches the old
        // `followSpeed * Time.deltaTime` lerp exactly at 60 fps and holds that same response at
        // other framerates instead of tightening/loosening with them.
        private const float ReferenceDeltaTime = 1f / 60f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (shadow != null)
            {
                shadowRenderer = shadow.GetComponent<SpriteRenderer>();
                shadowBaseX = shadow.localPosition.x;
            }
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        public void Initialize(Transform target, float initialPickupRange)
        {
            followTarget = target;
            PickupRange = initialPickupRange;
        }

        public void IncreasePickupRangePercent(float percent)
        {
            PickupRange *= 1f + percent;
        }

        private void Update()
        {
            if (followTarget == null) return;

            Vector3 desiredPosition = followTarget.position + (Vector3)followOffset;
            float dx = desiredPosition.x - transform.position.x;

            float retention = Mathf.Clamp01(1f - followSpeed * ReferenceDeltaTime);
            float t = 1f - Mathf.Pow(retention, Time.deltaTime / ReferenceDeltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

            if (Mathf.Abs(dx) > 0.01f)
            {
                bool flipX = dx > 0f;

                if (spriteRenderer != null) spriteRenderer.flipX = flipX;
                if (shadowRenderer != null) shadowRenderer.flipX = flipX;

                if (shadow != null)
                {
                    Vector3 shadowPos = shadow.localPosition;
                    shadowPos.x = flipX ? -shadowBaseX : shadowBaseX;
                    shadow.localPosition = shadowPos;
                }
            }
        }
    }
}
