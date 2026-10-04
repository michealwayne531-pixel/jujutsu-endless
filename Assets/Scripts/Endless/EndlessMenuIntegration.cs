using UnityEngine;
using UnityEngine.SceneManagement;
using Reconstructed.Endless;

namespace Reconstructed.UI
{
    /// <summary>Assign this component to the recovered main-menu Chapter Select button.</summary>
    public sealed class EndlessMenuIntegration : MonoBehaviour
    {
        [SerializeField] private string chapterSelectScene = "03_LevelSelection";
        [SerializeField] private EndlessGameBootstrap bootstrap;
        public void OpenChapterSelect()
        {
            if (bootstrap == null) bootstrap = EndlessGameBootstrap.Instance;
            SceneManager.LoadScene(chapterSelectScene);
        }
        public void ContinueCurrentLevel()
        {
            if (bootstrap == null) bootstrap = EndlessGameBootstrap.Instance;
            if (bootstrap != null) bootstrap.StartSelectedLevel();
        }
    }
}
