using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;


namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelist : WhitelistSetterBase
    {
        [SerializeField] [Tooltip("If enabled before accepting network data, it checks if the network owner is whitelisted.\nThis is more secure, but only works if a whitelisted player is in the instance, disable if you prefer reliability over security.")]
        protected bool secureNetworking = true;
        [SerializeField] [Range(1, 10)][Tooltip("Hoe long to wait before retrying applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected float netRetryDelay = 5f;
        [SerializeField] [Range(1, 10)][Tooltip("How many times to attempt applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected int maxNetRetryAttempts = 4;
        [SerializeField] [Tooltip("By default only retries or failed attempts are logged. If enabled, other things like successful attempts, updating a newly joined player, etc are logged as well.")]
        protected bool extraLogging = true;
        
        [Tooltip("Display Name of each User you would want to be whitelisted by default.")]
        [SerializeField] protected string[] startWhitelist;
        
        [FormerlySerializedAs("adminWhitelistManager")]
        [Tooltip("Whitelist of Admins which can change the Synced Whitelist without being on it them self.")]
        [SerializeField] protected WhitelistGetterBase adminWhitelist;
        [Tooltip("If enabled everyone can edit. This reduces security! So only enable if you really need to.")]
        [SerializeField] protected bool allowEveryoneToEdit;
        
        [Tooltip("Automatically remove a player when they leave the instance.")]
        [SerializeField] protected bool autoRemoveOnLeave;
        [Tooltip("Automatically removes every player if the person that added them left.")]
        [SerializeField] protected bool autoRemoveOnAdderLeft;
        
        protected string serializedNetWhitelist;
        protected string serializedNetAddedBy;
        
        protected bool canEdit;
        protected bool noneAdminAccess;
        
        protected int netAttempts;
        protected bool netRetryScheduled;
        protected VRCPlayerApi netSendingPlayer;
        
        protected bool netSendScheduled;
        
        protected const int NET_MAX_QUEUE_SIZE = 5; // Max number of network events that can be queued at once
        protected const float NET_SEND_DELAY = 0.5f; // Delay between trying to send network events if there are already ones queued up

        public override bool _IsSynced() => true;

        protected override void Start()
        {
            base.Start();
            canEdit = CanEdit(Networking.LocalPlayer);

            var startList = startWhitelist.ToDataList();
            whitelist = startList;
            usersAddedBy.SetValue(Networking.GetOwner(gameObject).displayName, startList.ShallowClone());
            base.WhitelistUpdated();

            if (adminWhitelist != null)
            {
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                noneAdminAccess = true;
            }
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

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);
            var displayName = player.displayName;
            
            if (autoRemoveOnAdderLeft && usersAddedBy.TryGetValue(displayName, out DataToken added))
            {
                var addedList = added.DataList.ShallowClone();
                for (int i = 0; i < addedList.Count; i++)
                {
                    base._RemoveUser(addedList[i].String, null, null, false);
                }
            }
            
            if (autoRemoveOnLeave)
            {
                base._RemoveUser(displayName, null, null, false);
            }

            if (autoRemoveOnAdderLeft || autoRemoveOnLeave)
            {
                if (Networking.IsOwner(gameObject) && canEdit)
                {
                    WhitelistUpdated();
                }
                else
                {
                    base.WhitelistUpdated();
                }
            }
        }

        // Only for Admin Whitelist
        public void _WhitelistUpdated()
        {
            canEdit = CanEdit(Networking.LocalPlayer);
        }
        
        //Networked Version of update Event, if local only is needed use base instead
        protected override void WhitelistUpdated(IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (!CanEdit(localPlayer))
            {
                MUIDebug.LogError("Synced Whitelist: You are not whitelisted!", gameObject);
                return;
            }
            
            DiscardNetAttempts();
            ScheduleSendingNetworkedData();

            base.WhitelistUpdated(senderBehaviour, senderEvent);
        }
        
        /// <summary>
        /// Checks if the provided player can edit the whitelist or not.
        /// </summary>
        /// <param name="player">VRCPlayerApi of which the edit access should be checked.</param>
        /// <returns>A bool depending on a player can edit or not.</returns>
        private bool CanEdit(VRCPlayerApi player)
        {
            return allowEveryoneToEdit || 
                   (noneAdminAccess && _IsPlayerWhitelisted(player)) || 
                   (adminWhitelist != null && adminWhitelist._IsPlayerWhitelisted(player));
        }

        /// <summary>
        /// Checks if the local player can edit the whitelist and writes an error message if they don't.
        /// </summary>
        /// <returns>A bool depending on a player can edit or not.</returns>
        private bool EditCheck()
        {
            if (!canEdit)
            {
                MUIDebug.LogError($"Synced Whitelist | You are not allowed to edit {gameObject.name}!", gameObject);
                return false;
            }

            return true;
        }
        
        private string GetSerializedWhitelist()
        {
            if (VRCJson.TrySerializeToJson(whitelist, JsonExportType.Minify, out DataToken whitelistResult))
                return whitelistResult.String;
            return String.Empty;
        }
        
        private string GetSerializedAddedBy()
        {
            if (VRCJson.TrySerializeToJson(usersAddedBy, JsonExportType.Minify, out DataToken addedByResult))
                return addedByResult.String;
            return String.Empty;
        }
        
        #region Abstract Methods
        /// <summary>
        /// Checks if the provided player can send networked data.
        /// </summary>
        /// <param name="player"><see cref="VRCPlayerApi"/> of the player to check.</param>
        /// <returns>True if they can, false if not.</returns>
        protected bool CanSendNet(VRCPlayerApi player)
        {
            return CanEdit(player);
        }
        
        /// <summary>
        /// Returns the <see cref="VRCPlayerApi"/> of a player in the instance that is allowed to send networked data.
        /// </summary>
        /// <returns><see cref="VRCPlayerApi"/> of a player who can send network data if none is found returns null.</returns>
        protected VRCPlayerApi GetAllowedSender()
        {
            return adminWhitelist._GetPlayerInInstance();
        }

        /// <summary>
        /// Applies the networked data locally.
        /// </summary>
        protected void ApplyNetData()
        {
            if (VRCJson.TryDeserializeFromJson(serializedNetAddedBy, out DataToken addedByResult))
            {
                usersAddedBy = addedByResult.DataDictionary;
            }
            
            if (VRCJson.TryDeserializeFromJson(serializedNetWhitelist, out DataToken whitelistResult))
            {
                whitelist = whitelistResult.DataList;
                base.WhitelistUpdated(); // Calls base class to skip the added access checks
            }
        }
        #endregion
        
        #region Receiving Networked Data

        /// <summary>
        /// Handles the receipt of networked data sent to this object.
        /// </summary>
        /// <param name="receiverId">The player ID of the intended recipient. Use -1 for broadcast to all players.</param>
        /// <param name="newWhitelistData">Serialized JSON string containing the updated whitelist data.</param>
        /// <param name="newAddedByData">Serialized JSON string containing the data tracking which users were added by whom.</param>
        [NetworkCallable(NET_MAX_QUEUE_SIZE)]
        public void OnNetworkedDataReceived(int receiverId, string newWhitelistData, string newAddedByData)
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
            
            serializedNetWhitelist = newWhitelistData;
            serializedNetAddedBy = newAddedByData;
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

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), -1 , GetSerializedWhitelist(), GetSerializedAddedBy());
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

            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetworkedDataReceived), player.playerId, GetSerializedWhitelist(), GetSerializedAddedBy());
            
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
        
        #region Overrides
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour = null,
            string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._AddUser(username, senderBehaviour, senderEvent, notify);
        }
        
        public override void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._AddUsers(newUsernames, senderBehaviour, senderEvent);
        }
        
        public override void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._AddUsers(newUsernames, senderBehaviour, senderEvent, notify);
        }
        
        public override void _AddUsers(VRCPlayerApi[] newPlayers, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._AddUsers(newPlayers, senderBehaviour, senderEvent, notify);
        }

        public override void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._RemoveUser(username, senderBehaviour, senderEvent, notify);
        }

        public override void _RemoveUsers(DataList usernamesToRemove, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._RemoveUsers(usernamesToRemove, senderBehaviour, senderEvent, notify);
        }

        public override void _RemoveUsers(string[] usernamesToRemove, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._RemoveUsers(usernamesToRemove, senderBehaviour, senderEvent, notify);
        }

        public override void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool directReplacement = false)
        {
            if (!EditCheck()) return;
            base._ReplaceWhitelist(newUsernames, senderBehaviour, senderEvent, directReplacement);
        }

        public override void _ReplaceWhitelist(string[] newUsernames, IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (!EditCheck()) return;
            base._ReplaceWhitelist(newUsernames, senderBehaviour, senderEvent);
        }

        public override void _ClearWhitelist(IUdonEventReceiver senderBehaviour = null, string senderEvent = null,
            bool notify = true)
        {
            if (!EditCheck()) return;
            base._ClearWhitelist(senderBehaviour, senderEvent, notify);
        }

        public override void _RemoveAddedBy(string username, IUdonEventReceiver senderBehaviour = null, string senderEvent = null, bool notify = true)
        {
            if (!EditCheck()) return;
            base._RemoveAddedBy(username, senderBehaviour, senderEvent, notify);
        }

        #endregion
    }
}
    