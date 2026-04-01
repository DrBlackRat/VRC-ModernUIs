
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UdonSharpEditor;
#endif
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistAutoAddAll : UdonSharpBehaviour
    {
        [Tooltip("Whitelist Setter to add all users to.")] 
        [SerializeField] private WhitelistSetterBase whitelist;
        
        [Tooltip("If provided you have to be the owner of the whitelist and be on the admin whitelist to add all players to it.")] 
        [SerializeField] private WhitelistGetterBase adminWhitelist;

        [Range(0, 20)] [Tooltip("Initial delay before the Whitelist is filled with all players. Useful if you want to wait for persistence to finish loading.")]
        [SerializeField] private float initialDelay = 5;
        
        private bool hasInitialized;
        private bool isFillQueued;
        
        private bool whitelistIsSynced;
        private bool isAdmin;

        private void Start()
        {
            if (adminWhitelist != null)
            {
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_AdminWhitelistUpdated));
            }
            else
            {
                isAdmin = true;
            }
        }

        public void _AdminWhitelistUpdated()
        {
            isAdmin = adminWhitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
        }
        
        private void OnEnable()
        {
            if (isFillQueued) return;
            isFillQueued = true;
            SendCustomEventDelayedSeconds(nameof(_FillEntireWhitelist), hasInitialized ? 0 : initialDelay);
        }

        private bool CanAdd()
        {
            if (!enabled || !gameObject.activeInHierarchy)
                return false;

            return isAdmin && (!whitelistIsSynced || Networking.IsOwner(whitelist.gameObject));
        }

        public void _FillEntireWhitelist()
        {
            isFillQueued = false;
            hasInitialized = true;
            
            if (!CanAdd())
                return;
            
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            whitelist._ClearWhitelist(null, null, false);
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
                whitelistIsSynced = true;
            UdonSharpEditorUtility.CopyProxyToUdon(this);
        }
#endif
    }
}
