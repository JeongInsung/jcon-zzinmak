using System.Collections;
using UnityEngine;

namespace Zzinmak.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.6f);
        [SerializeField] private float flashDuration = 0.15f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine flashRoutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            flashRoutine = null;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        public void Flash()
        {
            if (spriteRenderer == null || !isActiveAndEnabled) return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            flashRoutine = null;
        }
    }
}
