using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(Collider))]
    public class WhitelistZoneCounter : UdonSharpBehaviour
    {
        [SerializeField] private WhitelistSetterBase whitelist;
        
        private DataList inZoneUsers = new DataList();
        private bool hasInitialized;
        
        private void OnDisable()
        {
            inZoneUsers.Clear();
            CopyToWhitelist();
        }

        private void CopyToWhitelist()
        {
            whitelist._ReplaceWhitelist(inZoneUsers, (IUdonEventReceiver)this, "", true);
        }
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || 
                !player.IsValid())
            {
                return;
            }
            
            var playerName = player.displayName;
            if (inZoneUsers.Contains(playerName)) return;
            
            inZoneUsers.Add(playerName);
            CopyToWhitelist();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || 
                !player.IsValid())
            {
                return;
            }
            
            var playerName = player.displayName;
            if (!inZoneUsers.Contains(playerName)) return;
            
            inZoneUsers.Remove(playerName);
            CopyToWhitelist();
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || 
                !player.IsValid())
            {
                return;
            }
            
            var playerName = player.displayName;
            if (!inZoneUsers.Contains(playerName)) return;
            
            inZoneUsers.Remove(playerName);
            CopyToWhitelist();
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            gameObject.layer = 0;
        }
#endif
        
#if UNITY_EDITOR
        // Fix spawning inside the collider being broken in client sim
        private Collider _collider;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                return;
            }
            
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                return;
            }
            
            _collider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 1);
        }

        public void _EnableCollider()
        {
            _collider.enabled = true;
        }
#endif
    }
}