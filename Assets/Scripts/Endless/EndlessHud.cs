using UnityEngine;
using UnityEngine.UI;
using Reconstructed.Endless;
namespace Reconstructed.UI
{
    public sealed class EndlessHud : MonoBehaviour
    {
        [SerializeField] private Text label;
        public void SetLevel(long chapter,long level) { if(label) label.text=$"Chapter {chapter} - Level {level}"; }
        public void SetDefinition(LevelDefinition d) => SetLevel(d.chapterNumber,d.levelNumber);
    }
}
