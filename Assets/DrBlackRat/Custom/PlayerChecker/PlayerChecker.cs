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
        
        [Header("Validations")]
        [SerializeField]
        [Tooltip("If enabled, checks whether the name reported by a player matches the name visible locally.")]
        private bool validatePlayerNames = true;
        [SerializeField]
        [Tooltip("If enabled, checks whether the returned response ID matches the requested one.")]
        private bool validateResponseIds = true;
        
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
        
        private DataDictionary _playerDataDict; // Key: VRCPlayerApi, Value: DataDictionary
        private readonly DataToken pendingResponsesKey = new DataToken("PendingResponses"); // Value: DataList (responseIDs)
        private readonly DataToken missedResponsesKey = new DataToken("MissedResponses"); // Value: DataList (responseIDs)
        private readonly DataToken previousResponseMissedKey = new DataToken("PreviousResponseMissed"); // Value: bool
        private readonly DataToken consecutiveMissedResponsesKey = new DataToken("ConsecutiveMissedResponses"); // Value: int (counter)
        private readonly DataToken mismatchedNameResponsesKey = new DataToken("MismatchedNameResponses"); // Value: DataList (responseIDs)
        private readonly DataToken mismatchedIdResponsesKey = new DataToken("MismatchedIdResponses"); // Value: DataList (responseIDs)
        
        private int _nextResponseId = 0;
        
        private VRCTweenHandle _checkEventHandle;
        private bool _isRunning;
        private bool _hasInitialized;
        private bool _allowedToCheck;
        
        #region Unity, VRChat & ModernUI Events
        private void Start()
        {
            if (checkingPlayers == null)
            {
                _allowedToCheck = true;
                EnableAndStartChecking();
            }
            else
            {
                checkingPlayers._SetUpConnection(this, nameof(OnWhitelistUpdated));
            }
            
            _hasInitialized = true;
        }

        private void OnEnable()
        {
            if (!_hasInitialized)
            {
                return;
            }
            
            EnableAndStartChecking();
        }
        
        private void OnDisable()
        {
            DisableAndReset();
        }

        public void OnWhitelistUpdated()
        {
            var prevAllowedToCheck = _allowedToCheck;
            _allowedToCheck = checkingPlayers._IsPlayerWhitelisted(Networking.LocalPlayer);
            
            if (prevAllowedToCheck == _allowedToCheck || 
                !isActiveAndEnabled)
            {
                return;
            }
            
            if (_allowedToCheck)
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
            if (!_isRunning) 
            {
                return;
            }
            
            TryAddPlayer(player);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!_isRunning) 
            {
                return;
            }
            
            _playerDataDict.Remove(new DataToken(player));
        }
        #endregion
        
        #region Setup & Checking Loop
        private void EnableAndStartChecking()
        {
            if (!_isRunning || 
                !_allowedToCheck) 
            {
                return;
            }
            
            var allPlayers = VRCPlayerApi.GetPlayers();
            foreach (var player in allPlayers)
            {
                TryAddPlayer(player);
            }
            
            _isRunning = true;
            _CheckPlayers();
        }

        private bool TryAddPlayer(VRCPlayerApi player)
        {
            var playerToken = new DataToken(player);
            
            if (player == Networking.LocalPlayer ||
                _playerDataDict.ContainsKey(playerToken))
            {
                return false;
            }

            var playerInfoDict = new DataDictionary();
            playerInfoDict.Add(pendingResponsesKey, new DataList());
            playerInfoDict.Add(missedResponsesKey, new DataList());
            playerInfoDict.Add(previousResponseMissedKey, false);
            playerInfoDict.Add(consecutiveMissedResponsesKey, 0);
            playerInfoDict.Add(mismatchedNameResponsesKey, new DataList());
            playerInfoDict.Add(mismatchedIdResponsesKey, new DataList());
            _playerDataDict.Add(playerToken, playerInfoDict);
            
            return true;
        }

        private void DisableAndReset()
        {
            if (!_isRunning) 
            {
                return;
            }
            
            _checkEventHandle.Kill();
            _playerDataDict.Clear();
            _isRunning = false;
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