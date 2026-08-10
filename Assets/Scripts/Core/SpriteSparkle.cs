using UnityEngine;

namespace Zzinmak.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteSparkle : MonoBehaviour
    {
        [SerializeField] private float pulseSpeed = 3f;
        [Tooltip("Sprite color is already white at full brightness, so it can't get any brighter - the pulse has to dim toward this value and back instead.")]
        [SerializeField] private float minBrightness = 0.35f;
        [SerializeField] private float maxBrightness = 1f;
        [SerializeField] private float minScale = 0.85f;
        [SerializeField] private float maxScale = 1.15f;

        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private Vector3 baseScale;
        private float phaseOffset;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;
            baseScale = transform.localScale;
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed + phaseOffset) + 1f) * 0.5f;

            float brightness = Mathf.Lerp(minBrightness, maxBrightness, t);
            Color color = baseColor;
            color.r = Mathf.Clamp01(color.r * brightness);
            color.g = Mathf.Clamp01(color.g * brightness);
            color.b = Mathf.Clamp01(color.b * brightness);
            spriteRenderer.color = color;

            transform.localScale = baseScale * Mathf.Lerp(minScale, maxScale, t);
        }
    }
}
