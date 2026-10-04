using UnityEngine;
using UnityEngine.SceneManagement;
using Reconstructed.UI;

namespace Reconstructed.Endless
{
    /// <summary>Runtime bridge for the recovered scene flow. New integration code; original combat remains untouched.</summary>
    public sealed class EndlessGameBootstrap : MonoBehaviour
    {
        public static EndlessGameBootstrap Instance { get; private set; }
        [SerializeField] private EndlessSaveSystem saveSystem;
        [SerializeField] private EndlessHud hud;
        [SerializeField] private string recoveredGameplayScene = "Game_01";
        public LevelDefinition CurrentLevel { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            if (saveSystem == null) saveSystem = gameObject.AddComponent<EndlessSaveSystem>();
            saveSystem.Load();
            SceneManager.sceneLoaded += OnSceneLoaded;
            GenerateCurrentLevel();
        }
        private void OnDestroy() { if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded; }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { GenerateCurrentLevel(); }
        public void GenerateCurrentLevel()
        {
            CurrentLevel = EndlessProgression.GenerateLevel(saveSystem.Data.currentChapter, saveSystem.Data.currentLevel);
            if (hud != null) hud.SetDefinition(CurrentLevel);
        }
        public void SelectChapter(long chapter)
        {
            var c = EndlessProgression.GenerateChapter(chapter);
            saveSystem.Data.currentChapter = chapter;
            if (saveSystem.Data.currentLevel < 1 || saveSystem.Data.currentLevel > c.totalLevels) saveSystem.Data.currentLevel = 1;
            GenerateCurrentLevel();
        }
        public void SelectLevel(long level)
        {
            var c = EndlessProgression.GenerateChapter(saveSystem.Data.currentChapter);
            if (level < 1 || level > c.totalLevels) return;
            saveSystem.Data.currentLevel = level; GenerateCurrentLevel();
        }
        public void StartSelectedLevel()
        {
            // Existing level scenes remain the combat source for the recovered game. Generated content reuses the same scene.
            SceneManager.LoadScene(recoveredGameplayScene);
        }
        public void CompleteLevel() { saveSystem.CompleteCurrentLevel(); GenerateCurrentLevel(); }
        public ProgressionSaveData Progress => saveSystem.Data;
    }
}
