using UnityEngine;
using Zzinmak.Core;

namespace Zzinmak.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] private Sprite[] flashSprites;
        [SerializeField] private float displayDuration = 0.06f;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (spriteRenderer != null && flashSprites != null && flashSprites.Length > 0)
            {
                spriteRenderer.sprite = flashSprites[Random.Range(0, flashSprites.Length)];
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
