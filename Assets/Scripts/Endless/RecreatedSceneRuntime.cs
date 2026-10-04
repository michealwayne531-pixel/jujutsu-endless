using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reconstructed.Endless
{
    public enum RecreatedSceneRole { Loading, MainMenu, CharacterSelection, ChapterSelect, Training, Gameplay }

    /// <summary>Functional scene UI used by the authorized recreation; gameplay progression is backed by EndlessGameBootstrap.</summary>
    public sealed class RecreatedSceneRuntime : MonoBehaviour
    {
        [SerializeField] private RecreatedSceneRole role;
        private Canvas canvas;
        private Text status;
        private EndlessGameBootstrap bootstrap;

        private void Awake()
        {
            bootstrap = EndlessGameBootstrap.Instance;
            if (bootstrap == null)
            {
                GameObject manager = new GameObject("EndlessRuntime");
                bootstrap = manager.AddComponent<EndlessGameBootstrap>();
                manager.AddComponent<PackedDataBootstrap>();
            }
            BuildUI();
        }

        private void BuildUI()
        {
            GameObject canvasObject = new GameObject("RuntimeCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            AddText(SceneTitle(), 36, new Vector2(0.5f, 0.82f), new Vector2(0.9f, 0.16f));
            status = AddText(StatusText(), 22, new Vector2(0.5f, 0.68f), new Vector2(0.9f, 0.14f));
            if (role == RecreatedSceneRole.Loading) AddButton("Continue", 0.5f, 0.42f, delegate { SceneManager.LoadScene("00_MainMenu"); });
            if (role == RecreatedSceneRole.MainMenu)
            {
                AddButton("Chapter Select", 0.5f, 0.50f, delegate { SceneManager.LoadScene("03_LevelSelection"); });
                AddButton("Continue", 0.5f, 0.34f, delegate { bootstrap.StartSelectedLevel(); });
            }
            if (role == RecreatedSceneRole.ChapterSelect)
            {
                AddButton("Chapter 1", 0.28f, 0.48f, delegate { SelectChapter(1); });
                AddButton("Chapter 5", 0.50f, 0.48f, delegate { SelectChapter(5); });
                AddButton("Chapter 100", 0.72f, 0.48f, delegate { SelectChapter(100); });
                AddButton("Chapter 9999", 0.50f, 0.30f, delegate { SelectChapter(9999); });
                AddButton("Start Selected", 0.50f, 0.16f, delegate { bootstrap.StartSelectedLevel(); });
            }
            if (role == RecreatedSceneRole.CharacterSelection) AddButton("Select Recovered Character", 0.5f, 0.40f, delegate { SceneManager.LoadScene("03_LevelSelection"); });
            if (role == RecreatedSceneRole.Training) AddButton("Return to Menu", 0.5f, 0.35f, delegate { SceneManager.LoadScene("00_MainMenu"); });
            if (role == RecreatedSceneRole.Gameplay)
            {
                AddButton("Complete Level", 0.5f, 0.30f, delegate { bootstrap.CompleteLevel(); RefreshStatus(); });
                AddButton("Menu", 0.5f, 0.14f, delegate { SceneManager.LoadScene("00_MainMenu"); });
            }
        }

        private void SelectChapter(long chapter) { bootstrap.SelectChapter(chapter); RefreshStatus(); }
        private string SceneTitle()
        {
            switch (role)
            {
                case RecreatedSceneRole.MainMenu: return "Jujutsu Kaisen Fighting Game";
                case RecreatedSceneRole.ChapterSelect: return "Chapter Select";
                case RecreatedSceneRole.CharacterSelection: return "Character Selection";
                case RecreatedSceneRole.Training: return "Training Room";
                case RecreatedSceneRole.Gameplay: return "Endless Combat";
                default: return "Loading";
            }
        }
        private string StatusText()
        {
            if (bootstrap == null) return "Initializing recovered game data...";
            return string.Format("Chapter {0} - Level {1}", bootstrap.Progress.currentChapter, bootstrap.Progress.currentLevel);
        }
        private void RefreshStatus() { if (status != null) status.text = StatusText(); }
        private Text AddText(string value, int size, Vector2 anchor, Vector2 dimensions)
        {
            GameObject obj = new GameObject("Text"); obj.transform.SetParent(canvas.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>(); rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(dimensions.x * 1000f, dimensions.y * 600f);
            Text text = obj.AddComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; return text;
        }
        private void AddButton(string caption, float x, float y, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject(caption); obj.transform.SetParent(canvas.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>(); rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(x, y); rect.sizeDelta = new Vector2(300f, 72f);
            Image image = obj.AddComponent<Image>(); image.color = new Color(0.12f, 0.16f, 0.24f, 0.96f);
            Button button = obj.AddComponent<Button>(); button.onClick.AddListener(action);
            Text label = AddText(caption, 22, new Vector2(0.5f, 0.5f), new Vector2(0.9f, 0.8f)); label.transform.SetParent(obj.transform, false); RectTransform labelRect = label.GetComponent<RectTransform>(); labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
        }
    }
}
