
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(BoxCollider))]
    public class WhitelistZone : UdonSharpBehaviour
    {
        [SerializeField] private WhitelistGetterBase whitelist;
        [SerializeField] private Transform exitTransform;

        private Collider zoneCollider;
        
        private bool hasAccess;
        private bool inZone;
        
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            whitelist._SetUpConnection(GetComponent<UdonBehaviour>());
        }

        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            CheckAccess();
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || inZone) return;
            inZone = true;
            CheckAccess();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            inZone = false;
        }

        private void CheckAccess()
        {
            if (inZone && !hasAccess)
            {
                MUIDebug.LogWarning("You were in a Whitelist Zone without being whitelisted! You have been moved to the exist position.");
                Networking.LocalPlayer.TeleportTo(exitTransform.position, exitTransform.rotation);
            }
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

