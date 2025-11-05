
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistAutoAddAll : UdonSharpBehaviour
    {
        [Tooltip("Whitelist Setter to add all users to.")] 
        [SerializeField] private WhitelistSetterBase whitelist;
        [Tooltip("If enabled the Whitelist will only be added to if the local player is the owner of it.\nWill be force enabled for synced whitelists.")]
        [SerializeField] private bool onlyAddIfOwner;
        [Range(0, 20)] [Tooltip("Initial delay before the Whitelist is filled with all players. Useful if you want to wait for persistence to finish loading.")]
        [SerializeField] private float initialDelay = 5;
        
        private bool hasInitialized;
        private bool isFillQueued;

        private bool CanAdd() => whitelist != null && enabled && gameObject.activeInHierarchy &&
                                 (!onlyAddIfOwner || Networking.LocalPlayer.IsOwner(whitelist.gameObject));

        private void OnEnable()
        {
            if (isFillQueued) return;
            isFillQueued = true;
            SendCustomEventDelayedSeconds(nameof(_FillEntireWhitelist), hasInitialized ? 0 : initialDelay);
        }

        public void _FillEntireWhitelist()
        {
            isFillQueued = false;
            hasInitialized = true;
            
            if (!CanAdd())
                return;
            
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            whitelist._AddUsers(players);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!hasInitialized || !CanAdd())
                return;
            
            whitelist._AddUser(player.displayName);
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            if (whitelist?._IsSynced() == true)
                onlyAddIfOwner = true;
        }
#endif
    }
}
