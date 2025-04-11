using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIDistanceHider : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [Tooltip("Canvas Group that will be hidden when Player is outside of the trigger.")]
        [SerializeField] private CanvasGroup[] hidingCanvases;
        [Tooltip("Canvas Group that will be shown when Player is outside of the trigger. Use full for things like a distance hidden info message.")]
        [SerializeField] private CanvasGroup infoCanvas;

        private Collider zoneCollider;

        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider == null)
            {
                MUIDebug.LogError("UI Distance Hider: No collider found! Add a Box or Sphere Collider set to \"is Trigger\" for this to work correctly!");
                return;
            }
            UpdateCanvasGroups(true);
        }
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            UpdateCanvasGroups(false);
        }
        
        
        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            UpdateCanvasGroups(true);
        }
        
        // Spawn / Respawn Fixes
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public void _EnableCollider()
        {
            zoneCollider.enabled = true;
        }


        private void UpdateCanvasGroups(bool hidden)
        {
            foreach (var hidingCanvas in hidingCanvases)
            {
                hidingCanvas.alpha = hidden ? 0 : 1;  
            }
            infoCanvas.alpha = hidden ? 1 : 0;
        }
    }
}
