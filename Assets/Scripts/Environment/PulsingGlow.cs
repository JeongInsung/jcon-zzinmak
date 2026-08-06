using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Zzinmak.Environment
{
    [RequireComponent(typeof(Light2D))]
    public class PulsingGlow : MonoBehaviour
    {
        [SerializeField] private float minIntensity = 0.6f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float pulseSpeed = 2f;

        private Light2D light2D;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }
}
