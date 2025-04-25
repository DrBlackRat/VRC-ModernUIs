
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
            tabPos = new Vector2(-rectTransform.anchoredPosition.x, -rectTransform.anchoredPosition.y);
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
