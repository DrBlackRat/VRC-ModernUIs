using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedSliderUI : WhitelistSliderUI
    {
        [HideInInspector] public int LockButtonIdentifier;
        [HideInInspector] public VRCPlayerApi LockingPlayer;
        [HideInInspector] public bool IsLocked;
        
        #region VRChat / Unity Event Overrides
        protected override void Start()
        {
            base.Start();
            NetStart();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            NetOnPlayerJoined(player);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);
            NetOnPlayerLeft(player);
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.IsOwner(gameObject)) return;
            base.OnPlayerRestored(player);
        }

        public override void _WhitelistUpdated()
        {
            base._WhitelistUpdated();
            MoveAuthoritativeSenderIfNeeded();
        }

        #endregion
        
        #region Locking
        public void _OnLockStateChanged()
        {
            hasAccess = CheckAccess();
            UpdateInteractable();
        }

        protected override bool CheckAccess()
        {
            var whitelisted = whitelist == null || whitelist._IsPlayerWhitelisted(localPlayerApi);
            var lockingAccess = !IsLocked || localPlayerApi == LockingPlayer;
            return whitelisted && lockingAccess;
        }
        #endregion
        
        protected override bool UpdateValue(float newValue, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!base.UpdateValue(newValue, skipPersistence, skipSameCheck, fromNet)) return false;

            if (!fromNet)
            {
                SendNetworkData();
            }
            
            return true;
        }
        
        #region Networking Implementation
        private const string NetStateKey = "Value";
        
        /// <summary>
        /// Checks if the provided player can send networked data.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> of the player to check.</param>
        /// <returns>True if they can, false if not.</returns>
        protected bool IsAllowedSender(VRCPlayerApi player)
        {
            if (player == localPlayer)
                return hasAccess;
            
            var whitelisted = whitelist == null || whitelist._IsPlayerWhitelisted(player);
            var lockingAccess = !IsLocked || LockingPlayer == player;
            return whitelisted && lockingAccess;
        }

        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a player in the instance that is allowed to send networked data.
        /// </summary>
        /// <returns><see cref="VRCPlayerApi"/> of a player who can send network data if none is found returns null.</returns>
        protected VRCPlayerApi GetAllowedSender()
        {
            if (whitelist == null)
                return Networking.Master;
            return whitelist._GetPlayerInInstance(leavingPlayer);
        }

        /// <summary>
        /// Applies the networked data locally.
        /// </summary>
        protected void ApplyNetData(DataDictionary netData)
        {
            if (!netData.TryGetValue(NetStateKey, TokenType.Double, out DataToken stateResult))
            {
                MUIDebug.LogError($"Failed to deserialize state data on {gameObject.name}! Data may be malformed, skipping applying net data.", gameObject);
                return;
            }
            
            UpdateValue((float)stateResult.Double, true, false, true);
        }

        /// <summary>
        /// Called when ever network data should be sent, should return a <see cref="DataDictionary"/> containing the key value pairs that should be sent.
        /// </summary>
        /// <returns><see cref="DataDictionary"/> contating the data that should be sent.</returns>
        protected DataDictionary GetDataForSending()
        {
            var netDict = new DataDictionary();
            netDict.SetValue(NetStateKey, value);
            return netDict;
        }

        /// <summary>
        /// Applies the default data and overrides the current net state. Only used if <see cref="resetIfNoAllowedSenderPresent"/> is enabled and no allowed sender is present.
        /// </summary>
        protected void ApplyDefaultData()
        {
            UpdateValue(defaultValue, true, false, true);
        }
    
        /// <summary>
        /// Triggered when the authoritative network sender changes.
        /// </summary>
        /// <param name="newOwner">The <see cref="VRCPlayerApi"/> representing the new authoritative sender.</param>
        protected virtual void OnAuthoritativeSenderChanged(VRCPlayerApi newOwner)
        {
            
        }
        #endregion

        ////////////////////////////
        // Secure Networking Copy //
        ////////////////////////////
        
        #region Variables
        // General Settings
        [SerializeField]
        [Tooltip("If enabled before accepting network data, it checks if the network owner is whitelisted.\nThis is more secure, but only works if a whitelisted player is in the instance, disable if you prefer reliability over security.")]
        protected bool secureNetworking = true;
        [SerializeField]
        [Tooltip("If enabled, once the last allowed sender leaves the instance, the data will be reset to it's default state to keep it in sync with newly joined players. (Only used by Secure Networking)")]
        protected bool resetIfNoAllowedSenderPresent = true;
        
        [SerializeField]
        [Tooltip("If enabled, the data will be reset to it's default state if the person who sent data left. Even if another allowed sender would be present.")]
        protected bool resetIfSenderLeaves = false;

        // Retry Settings
        [SerializeField]
        [Range(1, 10)]
        [Tooltip("How long to wait before retrying applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected float netRetryDelay = 5f;
        [SerializeField]
        [Range(0, 10)]
        [Tooltip("How many times to retry applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected int maxNetRetryAttempts = 3;

        // Logging
        [SerializeField]
        [Tooltip("If enabled, things like successful attempts, updating a newly joined player, etc are logged in addition to retries or failed attempts.")]
        protected bool extraNetLogging = true;

        // Receiving Networked Data
        private DataDictionary netReceiveDataDict;
        private VRCPlayerApi netSendingPlayer;
        private VRCTweenHandle netReceiveRetryEvent;
        private int netAttempts;
        
        // Sending Networked Data
        private string netSendJson;
        private VRCTweenHandle netSendRetryEvent;
        
        // Local Stuff
        private VRCPlayerApi localPlayer;
        /// <summary>
        /// The <see cref="VRCPlayerApi"/> of the local player.
        /// </summary>
        protected VRCPlayerApi LocalPlayer => localPlayer;

        // Authoritative Sender
        private VRCPlayerApi authoritativeSender;
        /// <summary>
        /// The <see cref="VRCPlayerApi"/> of the player who is responsible for sending the current state to new players, usually the last sender or master. Can be null if there is currently no valid sender and secure networking is enabled.
        /// </summary>
        public VRCPlayerApi AuthoritativeSender
        {
            get => authoritativeSender; 
            private set
            {
                if (authoritativeSender == value)
                    return;
                
                authoritativeSender = value;
                OnAuthoritativeSenderChanged(value);
            }
        }
        private bool hasAuthoritativeSenderBeenReset = true;
        
        private VRCPlayerApi leavingPlayer;
        public VRCPlayerApi LeavingPlayer => leavingPlayer;


        private const int NetMaxQueueSize = 5; // Max number of network events that can be queued at once
        private const float NetSendDelay = 0.5f; // Delay between trying to send network events if there are already ones queued up
        #endregion
        
        #region Net VRChat / Unity Event Overrides
        /// <summary>
        /// Unity's <see cref="Start"/> Event but for secure networking.
        /// </summary>
        private void NetStart()
        {
            localPlayer = Networking.LocalPlayer; 
        }
        
        /// <summary>
        /// VRChat's <see cref="OnPlayerJoined"/> Event but for secure networking.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> of the player that joined.</param>
        private void NetOnPlayerJoined(VRCPlayerApi player)
        {
            SendInitialState(player);
        }
        
        /// <summary>
        /// VRChat's <see cref="OnPlayerLeft"/> Event but for secure networking.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> of the player that left.</param>
        private void NetOnPlayerLeft(VRCPlayerApi player)
        {
            leavingPlayer = player;
            CheckPlayerOnLeave();
            leavingPlayer = null;
        }

        /// <summary>
        /// Handles player leaving: checks if they're the authoritative sender, then reassigns and resets data if needed.
        /// </summary>
        private void CheckPlayerOnLeave()
        {
            if (leavingPlayer != authoritativeSender)
                return;
            
            MoveAuthoritativeSenderIfNeeded();
            ResetDataOnLeave();
        }
        #endregion

        #region Receiving Networked Data
        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Handles the receipt of networked data sent to this object.
        /// </summary>
        /// <param name="receiverId">The player ID of the intended recipient. Use -1 for broadcast to all players.</param>
        /// <param name="netJson">The networked JSON being transmitted.</param>
        [NetworkCallable(NetMaxQueueSize)]
        public void OnNetworkedDataReceived(int receiverId, string netJson)
        {
            if (!NetworkCalling.InNetworkCall)
            {
                MUIDebug.LogError($"OnNetworkedDataReceived can only be called as a network event! ({gameObject.name})", gameObject);
                return;
            }

            var callingPlayer = NetworkCalling.CallingPlayer;
            if (!Utilities.IsValid(callingPlayer))
            {
                MUIDebug.LogError($"OnNetworkedDataReceived called by an invalid player! ({gameObject.name})", gameObject);
                return;
            }

            if (receiverId != -1 && receiverId != localPlayer.playerId)
                return;

            if (!VRCJson.TryDeserializeFromJson(netJson, out DataToken result))
            {
                MUIDebug.LogError($"OnNetworkedDataReceived could not deserialize from JSON due to {result.ToString()}! ({gameObject.name})", gameObject);
                return;
            }

            if (result.TokenType != TokenType.DataDictionary)
            {
                MUIDebug.LogError($"OnNetworkedDataReceived received data is not a dictionary! ({gameObject.name})", gameObject);
                return;
            }

            netReceiveDataDict = result.DataDictionary;
            netSendingPlayer = callingPlayer;

            netAttempts = 0;
            netReceiveRetryEvent.Kill();
            
            _TryApplyNetData();
        }

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Attempts to apply network data, validating sender if secure networking is enabled, will retry if the sender couldn't be validated.
        /// </summary>
        public void _TryApplyNetData()
        {
            if (!secureNetworking)
            {
                AuthoritativeSender = netSendingPlayer;
                ApplyNetData(netReceiveDataDict);

                return;
            }
            
            if (!IsAllowedSender(netSendingPlayer))
            {
                if (netAttempts >= maxNetRetryAttempts)
                {
                    MUIDebug.LogError($"{netSendingPlayer.displayName} is still not allowed to send data! Aborting applying net data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
                    return;
                }
                
                MUIDebug.LogWarning($"{netSendingPlayer.displayName} is not allowed to send data but tried to on {gameObject.name}. Retrying in {netRetryDelay}s, Attempt {netAttempts} / {maxNetRetryAttempts}.", gameObject);
                netReceiveRetryEvent = VRCTween.DelayedCall(this, nameof(_TryApplyNetData), netRetryDelay);
                netAttempts++;
                return;
            }

            if (netAttempts > 1)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} is now allowed to send data! Applying networked data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
            }
            else if (extraNetLogging)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} has sent network data on {gameObject.name}.", gameObject);
            }
            
            AuthoritativeSender = netSendingPlayer;
            ApplyNetData(netReceiveDataDict);
        }
        #endregion

        #region Sending Networked Data
        /// <summary>
        /// Serializes data into a JSON string for network transmission.
        /// The data to be serialized is obtained from the abstract method <see cref="GetDataForSending"/>.
        /// </summary>
        /// <returns>A serialized JSON string if successful; otherwise, null if serialization fails.</returns>
        private string GetSerializedData()
        {
            if (!VRCJson.TrySerializeToJson(GetDataForSending(), JsonExportType.Minify, out DataToken result))
            {
                MUIDebug.LogError($"Failed to serialize data for sending! No data will be sent on ({gameObject.name})", gameObject);
                return null;
            }

            return result.String;
        }

        /// <summary>
        /// Attempts to send networked data immediately, or schedules a retry if sending is currently blocked.
        /// </summary>
        protected void SendNetworkData()
        {
            netSendRetryEvent.Kill();
            
            var data = GetSerializedData();
            if (string.IsNullOrEmpty(data))
                return;
            netSendJson = data;

            if (NetworkCalling.GetQueuedEvents(this, nameof(OnNetworkedDataReceived)) != 0)
            {
                netSendRetryEvent = VRCTween.DelayedCall(this, nameof(_TrySendNet), NetSendDelay);
                return;
            }

            _TrySendNet();
        }
        
        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Attempts to send network data to other players if the local player is allowed to send.
        /// </summary>
        public void _TrySendNet()
        {
            if (!IsAllowedSender(localPlayer))
            {
                MUIDebug.LogError($"You are trying to send network data on {gameObject.name} but you are not allowed to!", gameObject);
                return;
            }

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), -1, netSendJson);
            AuthoritativeSender = localPlayer;
            
            if (extraNetLogging)
                MUIDebug.Log($"Sending network data on {gameObject.name}.", gameObject);
        }
        
        /// <summary>
        /// Sends the current networked state to a newly joined player.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> representing the newly joined player.</param>
        private void SendInitialState(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player))
                return;
            if (player.isLocal)
                return;
            if (localPlayer != AuthoritativeSender)
                return;
            if (secureNetworking && !IsAllowedSender(localPlayer))
                return;

            var data = GetSerializedData();
            if (string.IsNullOrEmpty(data))
                return;

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), player.playerId, data);

            if (extraNetLogging)
                MUIDebug.Log($"Sending initial network data on {gameObject.name} to {player.displayName}.", gameObject);
        }
        #endregion

        #region Authoritative Sender, Joining & Leaving
        /// <summary>
        /// Verifies the validity and permissions of the current authoritative sender and reassigns it to a new eligible sender if necessary.
        /// Should be called whenever there are changes to the allowed senders
        /// </summary>
        /// <remarks>
        /// This method checks whether the current authoritative sender is valid, meets security requirements when secure networking is enabled,
        /// and has the permission to send networked data. If any of these conditions are not satisfied, the authoritative sender is transferred
        /// to a new player who meets the necessary criteria.
        /// </remarks>
        protected void MoveAuthoritativeSenderIfNeeded()
        {
            if (!Utilities.IsValid(AuthoritativeSender))
            {
                if (extraNetLogging)
                {
                    MUIDebug.Log($"Authoritative Sender of {gameObject.name} is null.", gameObject);
                }
                FindNewAuthoritativeSender();
                return;
            }

            if (!AuthoritativeSender.IsValid())
            {
                if (extraNetLogging)
                {
                    MUIDebug.Log($"Authoritative Sender of {gameObject.name} is invalid.", gameObject);
                }
                FindNewAuthoritativeSender();
                return;
            }

            if (AuthoritativeSender == leavingPlayer)
            {
                if (extraNetLogging)
                {
                    MUIDebug.Log($"Authoritative Sender of {gameObject.name} is the player is leaving the instance.", gameObject);
                }
                FindNewAuthoritativeSender();
                return;
            }

            if (secureNetworking && !IsAllowedSender(AuthoritativeSender))
            {
                if (extraNetLogging)
                {
                    MUIDebug.Log($"{AuthoritativeSender.displayName} is the Authoritative Sender of {gameObject.name}, but isn't allowed to send data.", gameObject);
                }
                FindNewAuthoritativeSender();
                return;
            }
        }

        /// <summary>
        /// Attempts to find and assign a new authoritative sender for the networked object.
        /// </summary>
        /// <remarks>
        /// When secure networking is enabled, the method searches for an allowed player to assume network ownership.
        /// If no allowed player is found, the current authoritative sender is set to null to prevent sync issues, and the object may reset
        /// to its default state if configured to do so.
        /// If secure networking is disabled, the network ownership is assigned to the Master player by default.
        /// </remarks>
        private void FindNewAuthoritativeSender()
        {
            if (!secureNetworking)
            {
                var master = Networking.Master;
                MUIDebug.Log($"Transferring Authoritative Sender of {gameObject.name} to the Master ({master.displayName}).", gameObject);
                AuthoritativeSender = master;
                hasAuthoritativeSenderBeenReset = false;
                return;
            }

            var newOwner = GetAllowedSender();
            if (!Utilities.IsValid(newOwner) || 
                !newOwner.IsValid() ||
                newOwner == leavingPlayer)
            {
                if (hasAuthoritativeSenderBeenReset) // Prevent resetting to if it's already reset
                {
                    return;
                }
                
                MUIDebug.Log($"Could not find a new player to transfer Authoritative Sender of {gameObject.name} to. Removing current Authoritative Sender to prevent syncing issues.", gameObject);
                AuthoritativeSender = null;
                hasAuthoritativeSenderBeenReset = true;
                ResetIfNoAllowedSenderPresent();
                return;
            }

            MUIDebug.Log($"Transferring Authoritative Sender of {gameObject.name} to {newOwner.displayName}.", gameObject);
            hasAuthoritativeSenderBeenReset = false;
            AuthoritativeSender = newOwner;
        }


        /// <summary>
        /// Resets the instance's data to its default state based on certain conditions when players leave the instance.
        /// This should only be called if the player leaving the instance is the current Authoritative Sender.
        /// </summary>
        private void ResetDataOnLeave()
        {
            if (resetIfSenderLeaves)
            {
                MUIDebug.Log($"Resetting {gameObject.name} to its default state because the sender left the instance.", gameObject);
                ApplyDefaultData();
                return;
            }
            
            ResetIfNoAllowedSenderPresent();
        }

        /// <summary>
        /// Resets the instance's data to its default state if there is no valid Authoritative Sender.
        /// Only used if <see cref="resetIfNoAllowedSenderPresent"/> is enabled.
        /// </summary>
        private void ResetIfNoAllowedSenderPresent()
        {
            if (!resetIfNoAllowedSenderPresent || authoritativeSender != null)
            {
                return;
            }
            
            MUIDebug.Log($"Resetting {gameObject.name} to its default state as there is no valid Authoritative Sender.", gameObject);
            ApplyDefaultData();
        }
        #endregion
    }
}
