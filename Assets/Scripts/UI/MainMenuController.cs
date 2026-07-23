using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zzinmak.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Game";

        public void LoadGameScene()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
