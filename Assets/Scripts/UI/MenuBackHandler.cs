using UnityEngine;
using UnityEngine.InputSystem;

namespace Zzinmak.UI
{
    /// <summary>
    /// 안드로이드의 하드웨어/제스처 뒤로가기는 Input System에서 Escape 키로 들어온다.
    /// 이게 없으면 설정창이나 크레딧이 열려 있어도 뒤로가기 한 번에 앱이 그대로 종료된다.
    /// </summary>
    public class MenuBackHandler : MonoBehaviour
    {
        [Tooltip("열려 있으면 뒤로가기로 닫을 패널들. 뒤쪽 항목을 먼저 확인한다.")]
        [SerializeField] private GameObject[] closablePanels;

        [Tooltip("다른 패널이 아무것도 열려 있지 않을 때 뒤로가기를 누르면 이 패널을 연다.")]
        [SerializeField] private GameObject quitConfirmPanel;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            HandleBack();
        }

        private void HandleBack()
        {
            if (quitConfirmPanel != null && quitConfirmPanel.activeSelf)
            {
                quitConfirmPanel.SetActive(false);
                return;
            }

            // 열려 있는 패널이 있으면 그것부터 닫는다. 나중에 연 패널을 먼저 닫도록 뒤에서부터 확인.
            for (int i = closablePanels.Length - 1; i >= 0; i--)
            {
                var panel = closablePanels[i];
                if (panel != null && panel.activeSelf)
                {
                    panel.SetActive(false);
                    return;
                }
            }

            if (quitConfirmPanel != null) quitConfirmPanel.SetActive(true);
        }
    }
}
