using UnityEngine;

namespace Zzinmak.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool padTop = true;
        [SerializeField] private bool padBottom = false;
        [SerializeField] private bool padLeft = false;
        [SerializeField] private bool padRight = false;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Vector2 baseOffsetMin;
        private Vector2 baseOffsetMax;
        private Rect appliedSafeArea;
        private Vector2Int appliedScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null) rootCanvas = rootCanvas.rootCanvas;

            baseOffsetMin = rectTransform.offsetMin;
            baseOffsetMax = rectTransform.offsetMax;
        }

        private void OnEnable()
        {
            Apply(true);
        }

        private void Update()
        {
            Apply(false);
        }

        private void Apply(bool force)
        {
            Rect safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);

            if (!force && safeArea == appliedSafeArea && screenSize == appliedScreenSize) return;

            appliedSafeArea = safeArea;
            appliedScreenSize = screenSize;

            float scale = rootCanvas != null && rootCanvas.scaleFactor > 0f ? rootCanvas.scaleFactor : 1f;

            float topInset = (screenSize.y - (safeArea.y + safeArea.height)) / scale;
            float bottomInset = safeArea.y / scale;
            float leftInset = safeArea.x / scale;
            float rightInset = (screenSize.x - (safeArea.x + safeArea.width)) / scale;

            Vector2 offsetMin = baseOffsetMin;
            Vector2 offsetMax = baseOffsetMax;

            if (padBottom) offsetMin.y += bottomInset;
            if (padLeft) offsetMin.x += leftInset;
            if (padTop) offsetMax.y -= topInset;
            if (padRight) offsetMax.x -= rightInset;

            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
