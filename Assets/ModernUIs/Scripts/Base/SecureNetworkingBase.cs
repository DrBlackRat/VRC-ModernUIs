using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Base
{
    /// Networking model:
    /// - Only the latest received network state is kept.
    /// - The player whose data was most recently accepted becomes the authoritative sender.
    /// - If validation is temporarily not possible (e.g. whitelist not initialized yet),
    ///   applying that latest state may be retried.
    /// - If newer network data arrives before a retry completes, the older pending state is discarded.
    /// This is intentional to ensure "latest state wins" behavior and avoid stale state application.

    /// <summary>
    /// This class is used as a base for everything that requires secure networking.
    /// Implement all abstract methods to provide the necessary functionality. Call <see cref="SendNetworkData"/> to send networked data. Call <see cref="MoveAuthoritativeSenderIfNeeded"/> when the result of CanSendNet could have changed.
    /// Either inherit from it or use it as a template. (Due to type limitations, I feel like the later will probably be the case)
    /// Currently used by: SyncedSliderUI, SyncedSelectorUI, and SyncedWhitelist
    /// </summary>
    public abstract class SecureNetworkingBase : UdonSharpBehaviour
    {
        [SerializeField]
        [Tooltip("If enabled before accepting network data, it checks if the network owner is whitelisted.\nThis is more secure, but only works if a whitelisted player is in the instance, disable if you prefer reliability over security.")]
        protected bool secureNetworking = true;

        [SerializeField]
        [Tooltip("If enabled, once the last allowed sender leaves the instance, the data will be reset to it's default state to keep it in sync with newly joined players. (Only used by Secure Networking)")]
        protected bool resetIfNoAllowedSenderPresent = true;

        [SerializeField]
        [Range(1, 10)]
        [Tooltip("How long to wait before retrying applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected float netRetryDelay = 5f;

        [SerializeField]
        [Range(1, 10)]
        [Tooltip("How many times to attempt applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected int maxNetRetryAttempts = 4;

        [SerializeField]
        [Tooltip("By default only retries or failed attempts are logged. If enabled, other things like successful attempts, updating a newly joined player, etc are logged as well.")]
        protected bool extraNetLogging = true;

        private DataDictionary netDataDict;

        private int netAttempts;
        private bool netRetryScheduled;
        private VRCPlayerApi netSendingPlayer;

        private bool netSendScheduled;
        
        
        private VRCPlayerApi localPlayer;
        /// <summary>
        /// The <see cref="VRCPlayerApi"/> of the local player.
        /// </summary>
        protected VRCPlayerApi LocalPlayer => localPlayer;

        private VRCPlayerApi authoritativeSender;
        /// <summary>
        /// The <see cref="VRCPlayerApi"/> of the player who is responsible for sending the current state to new players, usually the last sender or master. Can be null if there is currently no valid sender.
        /// </summary>
        public VRCPlayerApi AuthoritativeSender
        {
            get => authoritativeSender; 
            private set
            {
                authoritativeSender = value;
                OnAuthoritativeSenderChanged(value);
            }
        }

        private const int NET_MAX_QUEUE_SIZE = 5; // Max number of network events that can be queued at once
        private const float NET_SEND_DELAY = 0.5f; // Delay between trying to send network events if there are already ones queued up

        #region VRChat / Unity Events
        public virtual void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            SendInitialState(player);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);

            if (player == AuthoritativeSender)
                MoveAuthoritativeSenderIfNeeded();
        }

        #endregion

        #region Abstract & Virtual Methods

        /// <summary>
        /// Checks if the provided player can send networked data.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> of the player to check.</param>
        /// <returns>True if they can, false if not.</returns>
        protected abstract bool CanSendNet(VRCPlayerApi player); // Should essentially be a whitelist._IsPlayerWhitelisted(player)

        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a player in the instance that is allowed to send networked data.
        /// </summary>
        /// <returns><see cref="VRCPlayerApi"/> of a player who can send network data if none is found returns null.</returns>
        protected abstract VRCPlayerApi GetAllowedSender(); // Should essentially be a whitelist._GetPlayerInInstance()

        /// <summary>
        /// Applies the networked data locally.
        /// </summary>
        protected abstract void ApplyNetData(DataDictionary netData); // Should essentially just get the data out of the dict and apply it

        /// <summary>
        /// Called when ever network data should be sent, should return a <see cref="DataDictionary"/> containing the key value pairs that should be sent.
        /// </summary>
        /// <returns><see cref="DataDictionary"/> contating the data that should be sent.</returns>
        protected abstract DataDictionary GetDataForSending();

        /// <summary>
        /// Applies the default data and overrides the current net state. Only used if <see cref="resetIfNoAllowedSenderPresent"/> is enabled and no allowed sender is present.
        /// </summary>
        protected abstract void ApplyDefaultData();
    
        /// <summary>
        /// Triggered when the authoritative network sender changes.
        /// </summary>
        /// <param name="newOwner">The <see cref="VRCPlayerApi"/> representing the new authoritative sender.</param>
        protected virtual void OnAuthoritativeSenderChanged(VRCPlayerApi newOwner)
        {
        }

        #endregion

        #region Receiving Networked Data

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Handles the receipt of networked data sent to this object.
        /// </summary>
        /// <param name="receiverId">The player ID of the intended recipient. Use -1 for broadcast to all players.</param>
        /// <param name="netJson">The networked JSON being transmitted.</param>
        [NetworkCallable(NET_MAX_QUEUE_SIZE)]
        public virtual void OnNetworkedDataReceived(int receiverId, string netJson)
        {
            if (!NetworkCalling.InNetworkCall)
            {
                MUIDebug.LogError($"OnNetworkedDataReceived can only be called as a network event! ({gameObject.name})",
                    gameObject);
                return;
            }

            var callingPlayer = NetworkCalling.CallingPlayer;
            if (!Utilities.IsValid(callingPlayer))
            {
                MUIDebug.LogError($"OnNetworkedDataReceived called by an invalid player! ({gameObject.name})",
                    gameObject);
                return;
            }

            if (receiverId != -1 && receiverId != localPlayer.playerId)
                return;

            if (!VRCJson.TryDeserializeFromJson(netJson, out DataToken result))
            {
                MUIDebug.LogError(
                    $"OnNetworkedDataReceived could not deserialize from JSON due to {result.ToString()}! ({gameObject.name})",
                    gameObject);
                return;
            }

            if (result.TokenType != TokenType.DataDictionary)
            {
                MUIDebug.LogError($"OnNetworkedDataReceived received data is not a dictionary! ({gameObject.name})",
                    gameObject);
                return;
            }

            netDataDict = result.DataDictionary;
            netSendingPlayer = callingPlayer;

            DiscardNetAttempts();
            TryApplyNetData();
        }

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Handles receiving network data after a retry.
        /// </summary>
        public void _DelayedNetAttempt()
        {
            netRetryScheduled = false;
            if (netAttempts == 0) // 0th = finished or received new data
                return;

            TryApplyNetData();
        }

        /// <summary>
        /// Attempts to apply network data, validating sender if secure networking is enabled, will retry if the sender couldn't be validated.
        /// </summary>
        private void TryApplyNetData()
        {
            if (!secureNetworking)
            {
                AuthoritativeSender = netSendingPlayer;
                ApplyNetData(netDataDict);

                return;
            }

            netAttempts++;
            if (!CanSendNet(netSendingPlayer))
            {
                if (netAttempts >= maxNetRetryAttempts)
                {
                    MUIDebug.LogError(
                        $"{netSendingPlayer.displayName} is still not allowed to send data! Aborting applying net data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.",
                        gameObject);
                    DiscardNetAttempts();
                    return;
                }

                if (!netRetryScheduled)
                {
                    netRetryScheduled = true;
                    MUIDebug.LogWarning(
                        $"{netSendingPlayer.displayName} is not allowed to send data but tried to on {gameObject.name}. Retrying in {netRetryDelay}s, Attempt {netAttempts} / {maxNetRetryAttempts}.",
                        gameObject);
                    SendCustomEventDelayedSeconds(nameof(_DelayedNetAttempt), netRetryDelay);
                }

                return;
            }

            if (netAttempts > 1)
            {
                MUIDebug.Log(
                    $"{netSendingPlayer.displayName} is now allowed to send data! Applying networked data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.",
                    gameObject);
            }
            else if (extraNetLogging)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} has sent network data on {gameObject.name}.", gameObject);
            }

            DiscardNetAttempts();

            AuthoritativeSender = netSendingPlayer;
            ApplyNetData(netDataDict);
        }

        /// <summary>
        /// Resets the current retry state.
        /// Already scheduled retry callbacks will be disregarded when they fire.
        /// </summary>
        private void DiscardNetAttempts()
        {
            netAttempts = 0;
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
                MUIDebug.LogError($"Failed to serialize data for sending! No data will be sent on ({gameObject.name})",
                    gameObject);
                return null;
            }

            return result.String;
        }

        /// <summary>
        /// Attempts to send networked data immediately, or schedules a retry if sending is currently blocked.
        /// </summary>
        protected void SendNetworkData()
        {
            if (netSendScheduled)
                return;

            if (NetworkCalling.GetQueuedEvents(this, nameof(OnNetworkedDataReceived)) != 0)
            {
                netSendScheduled = true;
                SendCustomEventDelayedSeconds(nameof(_SendNetDelayed), NET_SEND_DELAY);
                return;
            }

            TrySendNet();
        }

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Handles delayed network data transmission retry.
        /// </summary>
        public void _SendNetDelayed()
        {
            netSendScheduled = false;
            SendNetworkData();
        }

        /// <summary>
        /// Attempts to send network data to other players if the local player is allowed to send.
        /// Usually only used internally, use <see cref="SendNetworkData"/> when possible.
        /// </summary>
        /// <returns>True if the network data was successfully sent; otherwise, false.</returns>
        private bool TrySendNet()
        {
            if (!CanSendNet(localPlayer))
                return false;

            var data = GetSerializedData();
            if (string.IsNullOrEmpty(data))
                return false;

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), -1, data);
            AuthoritativeSender = localPlayer;

            return true;
        }

        #endregion

        #region Ownership, Joining & Leaving
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
            if (secureNetworking && !CanSendNet(localPlayer))
                return;

            var data = GetSerializedData();
            if (string.IsNullOrEmpty(data))
                return;

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), player.playerId, data);

            if (extraNetLogging)
                MUIDebug.Log($"Send initial net data to {player.displayName} on {gameObject.name}.", gameObject);
        }


        /// <summary>
        /// Verifies the validity and permissions of the current authoritative sender and reassigns it to a new eligible sender if necessary.
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
                MUIDebug.LogWarning($"Authoritative Sender of {gameObject.name} is null.", gameObject);
                FindNewAuthoritativeSender();
                return;
            }

            if (!AuthoritativeSender.IsValid())
            {
                MUIDebug.LogWarning($"Authoritative Sender of {gameObject.name} is invalid.", gameObject);
                FindNewAuthoritativeSender();
                return;
            }

            if (secureNetworking && !CanSendNet(AuthoritativeSender))
            {
                MUIDebug.LogWarning($"{AuthoritativeSender.displayName} is the Authoritative Sender of {gameObject.name}, but isn't allowed to send data.", gameObject);
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
                MUIDebug.Log($"Transferring Authoritative Sender of {gameObject.name} to the Master.", gameObject);
                AuthoritativeSender = Networking.Master;
                return;
            }

            var newOwner = GetAllowedSender();
            if (!Utilities.IsValid(newOwner) || !newOwner.IsValid())
            {
                MUIDebug.LogWarning($"Could not find a new player to transfer Authoritative Sender of {gameObject.name} to. Removing current Authoritative Sender to prevent syncing issues.", gameObject);
                AuthoritativeSender = null;

                if (resetIfNoAllowedSenderPresent)
                {
                    MUIDebug.Log($"Resetting {gameObject.name} to default state.", gameObject);
                    ApplyDefaultData();
                }
                return;
            }

            MUIDebug.Log($"Found allowed player \"{newOwner.displayName}\" to transfer Authoritative Sender of {gameObject.name} to.", gameObject);
            AuthoritativeSender = newOwner;
        }
        #endregion
    }
}