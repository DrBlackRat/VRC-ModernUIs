
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Tab : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("Position the Tab should move to when selected.")]
        [SerializeField] private Vector2 tabsPos;
        [Tooltip("Canavs Group of which the Alpha will be set to when the Tab is hidden, this prevents it from rendering.")]
        [SerializeField] private CanvasGroup canvasGroup;

        // Tab Moving Stuff
        public Vector2 _GetPos()
        {
            return tabsPos;
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
