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
    public class WhitelistZoneAdd : UdonSharpBehaviour
    {
        [SerializeField] private WhitelistSetterBase whitelist;
        
        private DataList inZoneUsers = new DataList();
        private Collider zoneCollider;
        private bool hasInitialized;
        
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            hasInitialized = true;
        }

        private void OnEnable()
        {
            if (!hasInitialized) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 1);
        }
        
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
            var playerName = player.displayName;
            if (inZoneUsers.Contains(playerName)) return;
            if (!player.IsValid()) return;
            
            inZoneUsers.Add(playerName);
            CopyToWhitelist();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            var playerName = player.displayName;
            if (!inZoneUsers.Contains(playerName)) return;
            if (!player.IsValid()) return;
            
            inZoneUsers.Remove(playerName);
            CopyToWhitelist();
        }
        
        //Spawn / Respawn Fixes
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            var playerName = player.displayName;
            if (!inZoneUsers.Contains(playerName)) return;
            if (!player.IsValid()) return;
            
            inZoneUsers.Remove(playerName);
            CopyToWhitelist();
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
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            gameObject.layer = 0;
        }
#endif
    }
}