using UnityEngine;
using UnityEditor;

namespace HellishBattle
{
    public class MainMenu : MonoBehaviour
    {
        public void OpenScene(int id)
        {
            LoadingScreen.instance.loadingScreen(id);
        }

        public void QuitApplication()
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }
    }
}
