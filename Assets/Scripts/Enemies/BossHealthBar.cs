using UnityEngine;
using UnityEngine.UI;

namespace Zzinmak.Enemies
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        public void SetFill(float current, float max)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
