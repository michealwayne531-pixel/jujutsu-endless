using System;
using UnityEngine;
using UnityEngine.UI;
using Reconstructed.Endless;

namespace Reconstructed.UI
{
    /// <summary>Virtualized chapter selector: reuses a bounded row pool instead of creating thousands of objects.</summary>
    public sealed class ChapterSelectController : MonoBehaviour
    {
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform content;
        [SerializeField] private Button rowPrefab;
        [SerializeField] private int visibleRows = 12;
        private long firstChapter = 1;
        private Button[] rows;
        private void Awake() { rows = new Button[Mathf.Max(3, visibleRows + 2)]; for(int i=0;i<rows.Length;i++) rows[i]=Instantiate(rowPrefab,content); Refresh(); }
        public void JumpToChapter(long chapter) { firstChapter = Math.Max(1L, chapter); Refresh(); }
        public void Refresh() { for(int i=0;i<rows.Length;i++){ long n=firstChapter+i; var d=EndlessProgression.GenerateChapter(n); rows[i].GetComponentInChildren<Text>().text=$"{d.chapterName}\n{d.totalLevels} levels"; rows[i].onClick.RemoveAllListeners(); long chosen=n; rows[i].onClick.AddListener(()=>JumpToChapter(chosen)); } }
    }
}
