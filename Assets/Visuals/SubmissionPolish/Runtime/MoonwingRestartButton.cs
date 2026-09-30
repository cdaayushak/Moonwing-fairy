using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Moonwing.Visuals
{
    public sealed class MoonwingRestartButton : MonoBehaviour
    {
        public Button button;
        public CanvasGroup visibility;
        public MoonwingVictorySequence victorySequence;
        bool restarting;

        void OnEnable()
        {
            Refresh();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        void Update() { Refresh(); }
        void Refresh()
        {
            bool ready = !restarting && (!victorySequence || victorySequence.Completed);
            visibility.alpha = ready ? 1 : 0;
            visibility.interactable = visibility.blocksRaycasts = ready;
            button.interactable = ready;
        }
        public void Restart()
        {
            if (restarting || !button.interactable) return;
            var scene = SceneManager.GetActiveScene();
            if (!Application.CanStreamedLevelBeLoaded(scene.path))
            {
                Debug.LogError("The active Moonwing scene must be included in Build Settings.");
                return;
            }
            restarting = true;
            Refresh();
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(scene.path, LoadSceneMode.Single);
        }
    }
}
