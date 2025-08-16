
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(BoxCollider))]
    public class WhitelistZone : UdonSharpBehaviour
    {
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("Transform a player will be moved to if they are inside the Zone while they aren't allowed to.")]
        [SerializeField] private Transform exitTransform;
        
        [Tooltip("Checks if the players head is inside the bounds manually instead of relying on triggers. \nThis is more expensive but prevents things like stations or play space moving causing issues.")]
        [SerializeField] private bool advancedProtection;
        [Tooltip("Time in seconds after which the advanced protection check for entry again.")]
        [Range(0.1f, 5f)]
        [SerializeField] private float advancedUpdateInterval = 0.5f;

        private Collider zoneCollider;
        private VRCPlayerApi localPlayer;
        
        private bool hasAccess;
        private bool inZone;
        
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            localPlayer = Networking.LocalPlayer;
            
            if (advancedProtection) _AdvancedCheckUpdate();
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            CheckAccess();
        }
        
        private void CheckAccess()
        {
            if (inZone && !hasAccess)
            {
                MUIDebug.LogWarning("You were in a Whitelist Zone without being whitelisted! You have been moved to the exist position.");
                Networking.LocalPlayer.TeleportTo(exitTransform.position, exitTransform.rotation);

                if (advancedProtection) 
                    inZone = false;
            }
        }

        #region Advanced Protection
        public void _AdvancedCheckUpdate()
        {
            Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            bool newInZone = zoneCollider.bounds.Contains(headPos);

            if (newInZone != inZone)
            {
                inZone = newInZone;
                CheckAccess();
            }
            
            SendCustomEventDelayedSeconds(nameof(_AdvancedCheckUpdate), advancedUpdateInterval);
        }
        #endregion
        
        #region Simple Protection
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (advancedProtection || !player.isLocal || inZone) return;
            inZone = true;
            CheckAccess();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (advancedProtection || !player.isLocal) return;
            inZone = false;
        }
        
        // Spawn / Respawn Fixes
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal || advancedProtection) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!player.isLocal || advancedProtection) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public void _EnableCollider()
        {
            zoneCollider.enabled = true;
        }
        #endregion
       
    }
}

