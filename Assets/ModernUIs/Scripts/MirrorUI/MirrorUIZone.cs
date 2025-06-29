﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace DrBlackRat.VRC.ModernUIs.MirrorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(BoxCollider))]
    public class MirrorUIZone : UdonSharpBehaviour
    {
        [Tooltip("Mirror UI the Trigger Events should be sent to. You can have multiple of these attached to a Mirror UI.")]
        [SerializeField] private MirrorUI mirrorBehaviour;
        
        private Collider zoneCollider;
        private bool inZone; // Workaround for OnPlayerTriggerEnter being fired twice when respawning

        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
        }
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            Debug.LogWarning("Enter");
            if (!player.isLocal || inZone) return;
            inZone = true;
            mirrorBehaviour._ZoneUpdated(true);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            Debug.LogWarning("Exit");
            if (!player.isLocal) return;
            inZone = false;
            mirrorBehaviour._ZoneUpdated(false);
            
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
