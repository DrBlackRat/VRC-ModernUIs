using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedSelectorUI : WhitelistSelectorUI
    {
        [SerializeField] [Tooltip("If enabled before accepting network data, it checks if the network owner is whitelisted.\nThis is more secure, but only works if a whitelisted player is in the instance, disable if you prefer reliability over security.")]
        protected bool secureNetworking = true;
        [SerializeField] [Range(1, 10)][Tooltip("How long to wait before retrying applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected float netRetryDelay = 5f;
        [SerializeField] [Range(1, 10)][Tooltip("How many times to attempt applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected int maxNetRetryAttempts = 4;
        [SerializeField] [Tooltip("By default only retries or failed attempts are logged. If enabled, other things like successful attempts, updating a newly joined player, etc are logged as well.")]
        protected bool extraLogging = true;
        
        protected int netData;
        
        protected int netAttempts;
        protected bool netRetryScheduled;
        protected VRCPlayerApi netSendingPlayer;
        
        protected bool netSendScheduled;
        
        protected const int NET_MAX_QUEUE_SIZE = 5; // Max number of network events that can be queued at once
        protected const float NET_SEND_DELAY = 0.5f; // Delay between trying to send network events if there are already ones queued up
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            SendInitialState(player);
        }
        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            base.OnOwnershipTransferred(player);
            TransferOwnershipIfNeeded();
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.IsOwner(gameObject)) return;
            base.OnPlayerRestored(player);
        }

        protected override bool UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!base.UpdateSelection(newState, skipPersistence, skipSameCheck, fromNet)) return false;

            if (!fromNet)
            {
                DiscardNetAttempts();
                ScheduleSendingNetworkedData();
            }
            
            return true;
        }
        
        #region Abstract Methods
        /// <summary>
        /// Checks if the provided player can send networked data.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> of the player to check.</param>
        /// <returns>True if they can, false if not.</returns>
        protected bool CanSendNet(VRCPlayerApi player)
        {
            if (whitelist == null)
                return true;

            if (player == localPlayer)
                return hasAccess;
            
            return whitelist._IsPlayerWhitelisted(player);
        }
        
        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a player in the instance that is allowed to send networked data.
        /// </summary>
        /// <returns><see cref="VRCPlayerApi"/> of a player who can send network data if none is found returns null.</returns>
        protected VRCPlayerApi GetAllowedSender()
        {
            return whitelist._GetPlayerInInstance();
        }

        /// <summary>
        /// Applies the networked data locally.
        /// </summary>
        protected void ApplyNetData()
        {
            UpdateSelection(netData, true, false, true);
        }
        #endregion
        
        #region Receiving Networked Data
        /// <summary>
        /// Handles the receipt of networked data sent to this object.
        /// </summary>
        /// <param name="receiverId">The player ID of the intended recipient. Use -1 for broadcast to all players.</param>
        /// <param name="newData">The networked data being transmitted.</param>
        [NetworkCallable(NET_MAX_QUEUE_SIZE)]
        public void OnNetworkedDataReceived(int receiverId, int newData)
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
            
            if (receiverId != -1 && receiverId!= localPlayer.playerId) 
                return;
            
            netData = newData;
            netSendingPlayer = NetworkCalling.CallingPlayer;
            
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
        protected void TryApplyNetData()
        {
            if (!secureNetworking)
            {
                ApplyNetData();
                return;
            }

            netAttempts++;
            if (!CanSendNet(netSendingPlayer))
            {
                if (netAttempts >= maxNetRetryAttempts)
                {
                    MUIDebug.LogError($"{netSendingPlayer.displayName} is still not allowed to send data! Aborting applying net data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
                    DiscardNetAttempts();
                    return;
                }

                if (!netRetryScheduled)
                {
                    netRetryScheduled = true;
                    MUIDebug.LogWarning($"{netSendingPlayer.displayName} is not allowed to send data but tried to on {gameObject.name}. Retrying in {netRetryDelay}s, Attempt {netAttempts} / {maxNetRetryAttempts}.", gameObject);
                    SendCustomEventDelayedSeconds(nameof(_DelayedNetAttempt), netRetryDelay);
                }
                
                return;
            }
            
            if (netAttempts > 1)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} is now allowed to send data! Applying networked data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
            } 
            else if (extraLogging)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} has send network data on {gameObject.name}.", gameObject);
            }
            
            DiscardNetAttempts();
            ApplyNetData();
        }
        
        /// <summary>
        /// Resets the current retry state.
        /// Already scheduled retry callbacks will be disregarded when they fire.
        /// </summary>
        protected void DiscardNetAttempts()
        {
            netAttempts = 0;
        }
        #endregion
        
        #region Sending Networked Data
        /// <summary>
        /// Attempts to send networked data immediately, or schedules a retry if sending is currently blocked.
        /// </summary>
        protected void ScheduleSendingNetworkedData()
        {
            if (netSendScheduled)
                return;
            
            if (NetworkCalling.GetQueuedEvents((IUdonEventReceiver)this, nameof(OnNetworkedDataReceived)) != 0)
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
            ScheduleSendingNetworkedData();
        }

        /// <summary>
        /// Attempts to send network data to other players if the local player is allowed to send.
        /// Usually only used internally, use <see cref="ScheduleSendingNetworkedData"/> when possible.
        /// </summary>
        /// <returns>True if the network data was successfully sent; otherwise, false.</returns>
        protected bool TrySendNet()
        {
            if (!CanSendNet(localPlayer))
                return false;
            
            if (!localPlayer.IsOwner(gameObject)) 
                Networking.SetOwner(localPlayer, gameObject);

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), -1, selectedState);
            return true;
        }  
        #endregion
        
        #region Ownership & Joining
        /// <summary>
        /// Sends the current networked state to a newly joined player.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> representing the newly joined player.</param>
        protected void SendInitialState(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player))
                return;
            if (player.isLocal)
                return;
            if (!localPlayer.IsOwner(gameObject))
                return;
            if (secureNetworking && !CanSendNet(localPlayer))
                return;

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), player.playerId, selectedState);
            
            if (extraLogging)
                MUIDebug.Log($"Send initial net data to {player.displayName} on {gameObject.name}.", gameObject);
        }

        /// <summary>
        /// Checks if the network owner is allowed to send data, if not, it tries to transfer ownership to a player who is.
        /// </summary>
        protected void TransferOwnershipIfNeeded()
        {
            if (!localPlayer.isMaster) return;
            
            var owner = Networking.GetOwner(gameObject);
            if (!Utilities.IsValid(owner))
                return;
            if (CanSendNet(owner))
                return;
            
            MUIDebug.LogWarning($"{owner.displayName} is the Network Owner of {gameObject.name}, but isn't allowed to send data. " +
                                $"Trying to move ownership to a player who can.", gameObject);
            
            var newOwner = GetAllowedSender();
            if (newOwner == null)
            {
                MUIDebug.LogWarning($"Could not find a new player to transfer ownership of {gameObject.name} to. " +
                                    $"Keeping current owner, but new players may not receive the current state." , gameObject);
                return;
            }
            
            MUIDebug.Log($"Found allowed player \"{newOwner.displayName}\" to transfer ownership of {gameObject.name} to.", gameObject);
            Networking.SetOwner(newOwner, gameObject);
        }
        #endregion
    }
}
