
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(BoxCollider))]
    public class UIZone : UdonSharpBehaviour
    {
        private UIMover uiMover;
        private int id;
        private Collider zoneCollider;
        private bool inZone; // Workaround for OnPlayerTriggerEnter being fired twice when respawning

        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
        }

        public void _SetupUIMover(UIMover newUIMover, int newID)
        {
            uiMover = newUIMover;
            id = newID;
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || inZone) return;
            inZone = true;
            uiMover._PlayerEnteredZone(id);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            inZone = false;
            uiMover._PlayerLeftZone(id);
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

    }
}
