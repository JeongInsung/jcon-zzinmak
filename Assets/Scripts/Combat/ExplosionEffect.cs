using System.Collections;
using UnityEngine;

namespace Zzinmak.Combat
{
    public class ExplosionEffect : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameDuration = 0.035f;

        private void Start()
        {
            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && frames != null)
            {
                foreach (var frame in frames)
                {
                    if (frame == null) continue;

                    spriteRenderer.sprite = frame;
                    yield return new WaitForSeconds(frameDuration);
                }
            }

            Destroy(gameObject);
        }
    }
}
