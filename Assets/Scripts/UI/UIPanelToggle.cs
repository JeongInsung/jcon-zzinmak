using UnityEngine;

namespace Zzinmak.UI
{
    /// <summary>
    /// SettingsController처럼 특정 화면을 담당하는 전용 컨트롤러를 만들 필요 없이,
    /// GameObject 하나를 SetActive로 열고 닫기만 하면 되는 단순한 패널(크레딧 등)에 붙여서 쓴다.
    /// </summary>
    public class UIPanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        public void Open()
        {
            if (panel != null) panel.SetActive(true);
        }

        public void Close()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void Toggle()
        {
            if (panel != null) panel.SetActive(!panel.activeSelf);
        }
    }
}
