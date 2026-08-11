using UnityEngine;
using UnityEngine.UI;

namespace Zzinmak.UI
{
    // Cycles a UI Image through a flipbook of sprites. Frame state resets on enable so
    // the flash restarts cleanly each time the icon is shown (e.g. at the start of a wave).
    [RequireComponent(typeof(Image))]
    public class SirenBlinker : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameRate = 12f;

        private Image image;
        private float frameTimer;
        private int frameIndex;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            frameTimer = 0f;
            frameIndex = 0;
            if (frames != null && frames.Length > 0) image.sprite = frames[0];
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(0.01f, frameRate);
            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % frames.Length;
                image.sprite = frames[frameIndex];
            }
        }
    }
}
