using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zzinmak.UI
{
    /// <summary>
    /// 버튼을 누르면 클릭 사운드를 재생하고, 누르는 동안 살짝 작아지는 눌림 피드백을 준다.
    /// Button과 같은 GameObject에 붙인다. AudioSource도 같은 GameObject에 두면 자동으로 찾는다.
    /// </summary>
    [DisallowMultipleComponent]
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string SfxVolumeKey = "SFXVolume";

        [Header("사운드")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;

        [Tooltip("설정창의 SFXVolume(PlayerPrefs) 값을 곱해서 재생할지 여부.")]
        [SerializeField] private bool respectSfxVolumePref = true;

        [Header("눌림 스케일")]
        [Tooltip("누르고 있는 동안 원래 크기의 몇 배가 될지. 0.95 = 5% 작아짐.")]
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float pressDuration = 0.06f;
        [SerializeField] private float releaseDuration = 0.12f;

        private Transform scaleTarget;
        private Selectable selectable;
        private Vector3 baseScale = Vector3.one;
        private Coroutine scaleRoutine;
        private bool isPressed;

        private void Awake()
        {
            scaleTarget = transform;
            selectable = GetComponent<Selectable>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            // 기준 스케일은 애니메이션이 한 번도 돌기 전에 딱 한 번만 캡처한다.
            // (OnEnable에서 캡처하면 트윈 중간값을 새 기준으로 잡아버릴 수 있다)
            baseScale = scaleTarget.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // interactable=false여도 그래픽 레이캐스트는 계속 맞으므로 직접 걸러야 한다.
            if (selectable != null && !selectable.IsInteractable()) return;

            isPressed = true;
            PlayClick();
            StartScaleTo(baseScale * pressedScale, pressDuration);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;
            StartScaleTo(baseScale, releaseDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 누른 채로 버튼 밖으로 끌고 나가면 미리 원래 크기로 되돌린다.
            if (!isPressed) return;
            isPressed = false;
            StartScaleTo(baseScale, releaseDuration);
        }

        private void OnDisable()
        {
            // 뒤로 버튼처럼 클릭이 곧 패널 SetActive(false)로 이어지는 경우,
            // 코루틴은 조용히 멈추고 스케일이 눌린 상태로 굳어버린다. 여기서 무조건 복구한다.
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
                scaleRoutine = null;
            }

            isPressed = false;
            if (scaleTarget != null) scaleTarget.localScale = baseScale;
        }

        private void PlayClick()
        {
            if (audioSource == null || clickClip == null) return;

            float v = volume;
            if (respectSfxVolumePref) v *= PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
            v *= PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            if (v <= 0f) return;

            audioSource.PlayOneShot(clickClip, v);
        }

        private void StartScaleTo(Vector3 target, float duration)
        {
            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            // 현재 스케일에서 출발하므로 연타해도 튀지 않는다.
            scaleRoutine = StartCoroutine(ScaleRoutine(scaleTarget.localScale, target, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.0001f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;   // timeScale 0에서도 동작
                scaleTarget.localScale = Vector3.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            scaleTarget.localScale = to;
            scaleRoutine = null;
        }
    }
}
