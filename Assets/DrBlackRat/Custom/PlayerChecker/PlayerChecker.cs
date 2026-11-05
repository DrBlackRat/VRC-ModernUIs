using System;
using DrBlackRat.VRC.ModernUIs.Notifications;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

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
        
        [FormerlySerializedAs("checkingPlayers")]
        [Header("Whitelists")]
        [SerializeField]
        [Tooltip("Whitelist of players who should perform checks and receive notifications about players who don't respond or respond incorrectly. Leave empty if every player should do so.")]
        private WhitelistGetterBase checkingPlayersWhitelist;
        [SerializeField]
        [Tooltip("Whitelist of players who are not responding or responding incorrectly.")]
        private WhitelistSetterBase flaggedPlayers;
        
        [Header("Notification")]
        [SerializeField]
        [Tooltip("Manages notifications for players who are not responding or responding incorrectly.")]
        private NotificationManager notificationManager;
        [Space(10)]
        [SerializeField]
        [Tooltip("Header / Title of the notification.")]
        private string notificationHeader = "Player Checker";
        [SerializeField]
        [Tooltip("Image / Sprite used by the notification, if not provided the image will be disabled.")]
        private Sprite notificationImage;

        [Space(10)]
        [SerializeField]
        [TextArea(1, 3)] 
        [Tooltip("Message shown when a player has consecutively not responded to the max missed responses limit.")]
        private string missedResponsesNotificationMessage = "{playerName} has not responded to {missedResponses} consecutive checks!" +
                                                            "\nThey may be using a client or have disconnected.";
        [SerializeField]
        [TextArea(1, 3)]
        [Tooltip("Message shown when a player has responded to a check but the name does not match the local player's name.")]
        private string nameMismatchNotificationMessage = "{playerName} has responded to a check with \"{receivedName}\" as their name!" +
                                                         "\nThey may be using a client to spoof their name!";
        [SerializeField]
        [TextArea(1, 3)]
        [Tooltip("Message shown when a player has responded to a check with an incorrect response ID.")]
        private string responseIdMismatchNotificationMessage = "{playerName} has responded to a check with an invalid response ID!" +
                                                         "\nThey may be using a client!";
        
        [Space(10)]
        [SerializeField]
        [Tooltip("If a notification sound should be played.")]
        private bool playNotificationSound = true;
        [SerializeField]
        [Tooltip("Audio Clip used for the notification, if not provided the default will be used.")]
        private AudioClip notificationSoundClip;
        [SerializeField]
        [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        private float notificationSoundVolume = 0.25f;
        
        [Space(10)]
        [SerializeField]
        [Range(0f, 60f)] 
        [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        private float duration = 5f;
        [SerializeField]
        [Range(100f, 200f)] 
        [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        private float width = 150f;
        
        [Header("Other")]
        [SerializeField]
        [Tooltip("If enabled, sending out requests, responding to them, etc. will be logged.")]
        private bool extraLogging = false;
        
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
        
        private VRCPlayerApi _localPlayer;
        
        #region Unity, VRChat & ModernUI Events
        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            
            if (checkingPlayersWhitelist == null)
            {
                _allowedToCheck = true;
                EnableAndStartChecking();
            }
            else
            {
                checkingPlayersWhitelist._SetUpConnection(this, nameof(OnWhitelistUpdated));
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
            _allowedToCheck = checkingPlayersWhitelist._IsPlayerWhitelisted(_localPlayer);
            
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
        
        #region Setup, Enable & Disable
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
            _CheckPlayersLoop();
        }

        private bool TryAddPlayer(VRCPlayerApi player)
        {
            var playerToken = new DataToken(player);
            
            if (player == _localPlayer ||
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
        #endregion
        
        #region Checking Players Loop
        public void _CheckPlayersLoop()
        {
            var playerKeys = _playerDataDict.GetKeys();
            for (int i = 0; i < playerKeys.Count; i++)
            {
                var playerToken = playerKeys[i];
                CheckForMissedResponses(playerToken);
                SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(RequestPlayerCheck), _nextResponseId);
            }
            
            _nextResponseId++;
            _checkEventHandle = VRCTween.DelayedCall(this, nameof(_CheckPlayersLoop), checkInterval);
        }

        private void CheckForMissedResponses(DataToken playerToken)
        {
            
        }
        #endregion
        
        #region Network Events
        [NetworkCallable]
        public void RequestPlayerCheck(int responseId)
        {
            if (!NetworkCalling.InNetworkCall)
            {
                return;
            }

            var callingPlayerId = NetworkCalling.CallingPlayer.playerId;
            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(RespondToPlayerCheck), callingPlayerId, responseId, _localPlayer.displayName);
        }

        [NetworkCallable]
        public void RespondToPlayerCheck(int receiverId, int responseId, string playerName)
        {
            if (!NetworkCalling.InNetworkCall ||
                _localPlayer.playerId != receiverId)
            {
                return;
            }
            
            
        }
        #endregion
    }
}