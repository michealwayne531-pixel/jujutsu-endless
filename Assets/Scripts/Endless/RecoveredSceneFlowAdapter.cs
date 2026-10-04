using UnityEngine;
using UnityEngine.SceneManagement;
using Reconstructed.Endless;

namespace Reconstructed.Integration
{
    /// <summary>Maps recovered build-scene names to the endless runtime without editing recovered combat scripts.</summary>
    public sealed class RecoveredSceneFlowAdapter : MonoBehaviour
    {
        [SerializeField] private EndlessGameBootstrap bootstrap;
        private static readonly string[] RecoveredLevelScenes = { "Game_01", "Game_02", "Game_03", "Game_04", "Game_05", "Game_06", "Game_07", "Game_08", "Game_09", "Game_10" };
        public bool IsRecoveredLevelScene(string name) { for (int i=0;i<RecoveredLevelScenes.Length;i++) if (name == RecoveredLevelScenes[i]) return true; return false; }
        public void OnRecoveredLevelComplete() { if (bootstrap == null) bootstrap = EndlessGameBootstrap.Instance; if (bootstrap != null) bootstrap.CompleteLevel(); }
        public void OnChapterSelect(long chapter) { if (bootstrap == null) bootstrap = EndlessGameBootstrap.Instance; if (bootstrap != null) bootstrap.SelectChapter(chapter); }
        public void OnLevelSelect(long level) { if (bootstrap == null) bootstrap = EndlessGameBootstrap.Instance; if (bootstrap != null) bootstrap.SelectLevel(level); }
        private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
        private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { if (IsRecoveredLevelScene(scene.name) && bootstrap != null) bootstrap.GenerateCurrentLevel(); }
    }
}
