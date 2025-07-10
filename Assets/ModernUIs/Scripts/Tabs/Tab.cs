
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Tab : UdonSharpBehaviour
    {
        private RectTransform rectTransform;
        private Vector2 tabPos;
        private CanvasGroup canvasGroup;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            tabPos = TabPos(rectTransform);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        /// <summary>
        /// Only used in Editor to switch to this Tab
        /// </summary>
        [ContextMenu("Move To Tab")]
        public void MoveToTab()
        {
            if (Application.isPlaying) return;
            var parentTransform = (RectTransform)transform.parent;
            parentTransform.anchoredPosition = TabPos((RectTransform)transform);
        }
#endif
        
        private Vector2 TabPos(RectTransform tabTransform)
        {
            return new Vector2(-tabTransform.anchoredPosition.x, -tabTransform.anchoredPosition.y);
        }

        // Tab Moving Stuff
        public Vector2 _GetPos()
        {
            return tabPos;
        }
        public void _Hide()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0;
        }
        public void _Show()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 1;
        }
    }
}
