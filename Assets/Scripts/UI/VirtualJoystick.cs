using UnityEngine;
using UnityEngine.EventSystems;

namespace Zzinmak.UI
{
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 70f;

        public Vector2 Direction { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null) return;

            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
                handle.anchoredPosition = clamped;
                Direction = clamped / handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            Direction = Vector2.zero;
        }
    }
}
