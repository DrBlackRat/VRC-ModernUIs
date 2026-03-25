using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Base
{
    /// <summary>
    /// This class is used as a base for everything that requires secure networking.
    /// Either inherit from it or use it as a template. (Due to type limitations, I feel like the later will probably be the case)
    /// Currently used by: SyncedSliderUI, SyncedSelectorUI, and SyncedWhitelist
    /// </summary>
    public abstract class SecureNetworkingBase : UdonSharpBehaviour
    {
        [SerializeField] [Tooltip("If enabled before accepting network data, it checks if the network owner is whitelisted.\nThis is more secure, but only works if a whitelisted player is in the instance, disable if you prefer reliability over security.")]
        protected bool secureNetworking = true;
        [SerializeField] [Range(1, 10)][Tooltip("Hoe long to wait before retrying applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected float netRetryDelay = 5f;
        [SerializeField] [Range(1, 10)][Tooltip("How many times to attempt applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected int maxNetRetryAttempts = 4;
        [SerializeField] [Tooltip("By default only retries or failed attempts are logged. If enabled, other things like successful attempts, updating a newly joined player, etc are logged as well.")]
        protected bool extraLogging = true;

        protected string data; // Local example data
        protected string netData; // Networked example data
        
        protected int netAttempts;
        protected int scheduledNetAttempts;
        protected VRCPlayerApi netSendingPlayer;
        
        protected const int NET_MAX_QUEUE_SIZE = 2;
        protected bool netSendScheduled;

        protected VRCPlayerApi localPlayer;
        

        #region VRChat / Unity Events
        public void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

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
        #endregion

        #region Abstract Methods
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
        protected abstract void ApplyNetData(); // Should set the localData to be the netData and call whatever method needed to propagate that change
        #endregion
        
        #region Receiving Networked Data
        /// <summary>
        /// Handles the receipt of networked data sent to this object.
        /// </summary>
        /// <param name="receiverId">The player ID of the intended recipient. Use -1 for broadcast to all players.</param>
        /// <param name="newData">The networked data being transmitted.</param>
        [NetworkCallable(NET_MAX_QUEUE_SIZE)]
        public virtual void OnNetworkedDataReceived(int receiverId, string newData)
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
            ResetNetAttempts();
            TryApplyNetData();
        }

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// </summary>
        public void _DelayedNetAttempt()
        {
            scheduledNetAttempts--;
            if (scheduledNetAttempts > 0) return; // Prevent scheduling multiple delayed attempts
            if (netAttempts == 0) return; // 0th = finished or received new data
            TryApplyNetData();
        }

        /// <summary>
        /// Attempts to apply network data, validating sender if secure networking is enabled, will retry if the sender couldn't be validated.
        /// </summary>
        protected virtual void TryApplyNetData()
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
                    ResetNetAttempts();
                    return;
                }
                
                MUIDebug.LogWarning($"{netSendingPlayer.displayName} is not allowed to send data but tried to on {gameObject.name}. Retrying in {netRetryDelay}s, Attempt {netAttempts} / {maxNetRetryAttempts}.", gameObject);
                scheduledNetAttempts++;
                SendCustomEventDelayedSeconds(nameof(_DelayedNetAttempt), netRetryDelay);
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
            
            ResetNetAttempts();
            ApplyNetData();
        }
        
        /// <summary>
        /// Resets the number of attempts to apply network data which in return cancels all scheduled attempts.
        /// </summary>
        protected void ResetNetAttempts()
        {
            netAttempts = 0;
        }
        #endregion
        
        #region Sending Networked Data
        /// <summary>
        /// Sends networked data to other players in the instance.
        /// </summary>
        protected void ScheduleSendNetworkedData()
        {
            if (netSendScheduled)
                return;
            
            if (NetworkCalling.GetQueuedEvents((IUdonEventReceiver)this, nameof(OnNetworkedDataReceived)) != 0)
            {
                netSendScheduled = true;
                SendCustomEventDelayedSeconds(nameof(_SendNetDelayed), 1);
                return;
            }

            TrySendNet();
        }

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Checks if there are any queued events and if not, tries to send the data, otherwise schedules a retry.
        /// </summary>
        public void _SendNetDelayed()
        {
            netSendScheduled = false;
            
            if (NetworkCalling.GetQueuedEvents((IUdonEventReceiver)this, nameof(OnNetworkedDataReceived)) != 0)
            {
                netSendScheduled = true;
                SendCustomEventDelayedSeconds(nameof(_SendNetDelayed), 1);
                return;
            }

            TrySendNet();
        }

        /// <summary>
        /// Attempts to send network data to other players if the local player is allowed to send.
        /// Usually only used internally, use <see cref="ScheduleSendNetworkedData"/> when possible.
        /// </summary>
        /// <returns>True if the network data was successfully sent; otherwise, false.</returns>
        protected virtual bool TrySendNet()
        {
            if (!CanSendNet(localPlayer))
                return false;
            
            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), -1, data);
            return true;
        }
        #endregion

        #region Ownership & Joining
        /// <summary>
        /// Sends the current networked state to a newly joined player.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> representing the newly joined player.</param>
        protected virtual void SendInitialState(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player))
                return;
            if (player.isLocal)
                return;
            if (!localPlayer.IsOwner(gameObject))
                return;
            if (!CanSendNet(localPlayer))
                return;

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), player.playerId, data);
            
            if (extraLogging)
                MUIDebug.Log($"Send initial net data to {player.displayName} on {gameObject.name}.", gameObject);
        }

        /// <summary>
        /// Checks if the network owner is allowed to send data, if not, it tries to transfer ownership to a player who is.
        /// </summary>
        protected virtual void TransferOwnershipIfNeeded()
        {
            if (!localPlayer.isMaster) return;
            var owner = Networking.GetOwner(gameObject);
            if (CanSendNet(owner))
                return;
            
            MUIDebug.LogWarning($"{owner.displayName} is the Network Owner of {gameObject.name}, but isn't allowed to send data. " +
                                $"Trying to move ownership to a player who can.", gameObject);
            
            var newOwner = GetAllowedSender();
            if (newOwner == null)
            {
                MUIDebug.LogWarning($"Could not find a new player to transfer ownership of {gameObject.name} to. " +
                                    $"Keeping current owner, but new players wont receive the current state." , gameObject);
                return;
            }
            
            MUIDebug.Log($"Found allowed player \"{newOwner.displayName}\" to transfer ownership of {gameObject.name} to.", gameObject);
            Networking.SetOwner(newOwner, gameObject);
        }
        #endregion
    }
    
}