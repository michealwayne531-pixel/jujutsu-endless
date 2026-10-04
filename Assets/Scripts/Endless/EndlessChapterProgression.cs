using UnityEngine;
using Reconstructed.Endless;

namespace Reconstructed.Gameplay
{
    /// <summary>Attach to the recovered level-complete event target; prevents any original-content hard stop.</summary>
    public sealed class EndlessChapterProgression : MonoBehaviour
    {
        [SerializeField] private EndlessGameBootstrap bootstrap;
        public void OnLevelComplete()
        {
            if (bootstrap == null) bootstrap = EndlessGameBootstrap.Instance;
            if (bootstrap != null) bootstrap.CompleteLevel();
        }
    }
}
