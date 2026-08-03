using UnityEngine;
using Zzinmak.Core;

namespace Zzinmak.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] private Sprite flashSprite;
        [SerializeField] private float displayDuration = 0.08f;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (spriteRenderer != null && flashSprite != null)
            {
                spriteRenderer.sprite = flashSprite;
            }

            Invoke(nameof(ReturnToPool), displayDuration);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void ReturnToPool()
        {
            ObjectPool.Instance.Return(gameObject);
        }
    }
}
