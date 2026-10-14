using System;
using DrBlackRat.VRC.ModernUIs.Notifications;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC
{
    public class PlayerChecker : UdonSharpBehaviour
    {
        [Header("Timings & Limits")]
        [SerializeField]
        [Range(5f, 60f)]
        [Tooltip("Time in seconds between each check. If a player doesn't respond before the next check, they will be marked as not responding.")]
        private float checkInterval = 10f;
        [SerializeField]
        [Min(1)]
        [Tooltip("Maximum number of consecutive missed responses before marking a player as not responding.")]
        private int maxMissedResponses = 3;
        [Space(10)]
        [SerializeField]
        [Tooltip("If enabled, checks whether the name reported by a player matches the name visible locally.")]
        private bool validatePlayerNames = true;
        
        [Header("Whitelists")]
        [SerializeField]
        [Tooltip("Whitelist of players who should perform checks and receive notifications about players who don't respond or respond incorrectly. Leave empty if every player should do so.")]
        private WhitelistGetterBase checkingPlayers;
        [SerializeField]
        [Tooltip("Whitelist of players who are not responding or responding incorrectly.")]
        private WhitelistSetterBase flaggedPlayers;
        
        [Header("Notification")]
        [SerializeField]
        [Tooltip("Manages notifications for players who are not responding or responding incorrectly.")]
        private NotificationManager notificationManager;
        
        [Header("Other")]
        [SerializeField]
        [Tooltip("If enabled, players who are not responding or responding incorrectly will also be logged in the console.")]
        private bool logging = false;

        private DataDictionary _playerDataDict; // Key: VRCPlayerApi, Value: DataDictionary (Key: missedResponses, Value: DataList (MissedResponseIDs); Key: nameMismatch, Value: bool)
        private int _nextResponseId = 0;
        
        private VRCTweenHandle _checkEventHandle;
        private bool _hasInitialized;
        private bool _shouldCheckPlayers;
        
        #region Unity, VRChat & ModernUI Events
        private void Start()
        {
            
        }

        private void OnEnable()
        {
            
        }
        
        private void OnDisable()
        {
            
        }

        public void OnWhitelistUpdated()
        {
            var prevShouldCheck = _shouldCheckPlayers;
            _shouldCheckPlayers = checkingPlayers._IsPlayerWhitelisted(Networking.LocalPlayer);
            
            if (prevShouldCheck == _shouldCheckPlayers)
            {
                return;
            }
            
            if (_shouldCheckPlayers)
            {
                EnableAndStartChecking();
            }
            else
            {
                DisableAndReset();
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            
        }
        
        #endregion
        
        #region Setup & Checking Loop

        private void EnableAndStartChecking()
        {
            
        }

        private void DisableAndReset()
        {
            
        }
        
        public void _CheckPlayers()
        {
            _checkEventHandle = VRCTween.DelayedCall(this, nameof(_CheckPlayers), checkInterval);
        }
        #endregion
        
        #region Network Events
        [NetworkCallable]
        public void RequestPlayerCheck(int responseId)
        {
            
        }

        [NetworkCallable]
        public void RespondToPlayerCheck(int responseId, string playerName)
        {
            
        }
        #endregion
    }
}