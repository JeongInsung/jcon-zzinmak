using UnityEngine;
using UnityEngine.EventSystems;

namespace Zzinmak.UI
{
    // Attached to a full-screen invisible touch area. On touch, the visual
    // background/handle float to the touch position instead of sitting fixed.
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform touchArea;
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 90f;

        public Vector2 Direction { get; private set; }

        private void Awake()
        {
            if (background != null)
            {
                background.gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (touchArea == null || background == null || handle == null) return;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(touchArea, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                background.anchoredPosition = localPoint;
                handle.anchoredPosition = Vector2.zero;
                background.gameObject.SetActive(true);
                Direction = Vector2.zero;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (touchArea == null || background == null || handle == null) return;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(touchArea, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                Vector2 delta = localPoint - background.anchoredPosition;
                Vector2 clamped = Vector2.ClampMagnitude(delta, handleRange);
                handle.anchoredPosition = clamped;
                Direction = clamped / handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;

            if (background != null)
            {
                background.gameObject.SetActive(false);
            }
        }
    }
}
