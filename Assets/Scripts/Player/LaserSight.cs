using UnityEngine;

namespace Zzinmak.Player
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserSight : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform origin;
        [SerializeField] private float length = 10f;

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        public void Enable()
        {
            lineRenderer.enabled = true;
        }

        private void LateUpdate()
        {
            if (!lineRenderer.enabled || playerController == null) return;

            Vector3 start = origin != null ? origin.position : transform.position;
            Vector3 end = start + (Vector3)(playerController.AimDirection * length);

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }
}
