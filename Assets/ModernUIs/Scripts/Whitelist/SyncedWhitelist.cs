using System;
using BestHTTP.JSON;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Economy;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedWhitelist : WhitelistSetterBase
    {
        [Range(1, 10)][Tooltip("Hoe long to wait before retrying deserialization if the network isn't allowed to edit yet.")]
        [SerializeField] protected float deserializationRetryDelay = 5f;
        [Range(1, 10)][Tooltip("How many times to attempt deserialization if the network owner isn't allowed to edit yet.")]
        [SerializeField] protected int maxDeserializationAttempts = 4;
        
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
        
        [UdonSynced] protected string serializedWhitelist;
        [UdonSynced] protected string serializedAddedBy;
        
        protected bool canEdit;
        protected bool noneAdminAccess;
        
        protected int deserializationAttempts = 0;
        protected int scheduledDeserializations = 0;

        public override bool _IsSynced() => true;

        protected override void Start()
        {
            localName = Networking.LocalPlayer.displayName;
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
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);
            if (!Networking.LocalPlayer.IsOwner(gameObject)) return;
            var displayName = player.displayName;
            
            if (autoRemoveOnAdderLeft)
            {
                _RemoveAddedBy(displayName);
            }
            
            if (autoRemoveOnLeave)
            {
                _RemoveUser(displayName);
            }
        }

        // Only for Admin Whitelist
        public void _WhitelistUpdated()
        {
            canEdit = CanEdit(Networking.LocalPlayer);
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
                MUIDebug.LogError("Whitelist Manager: You are not allowed to edit this whitelist!");
                return false;
            }

            return true;
        }
        
        #region Deserialization
        public override void OnDeserialization()
        {
            deserializationAttempts = 0;
            HandleDeserialization();
        }
        
        public void _DelayedDeserialization()
        {
            scheduledDeserializations--;
            if (scheduledDeserializations > 0) return;
            if (deserializationAttempts == 0) return; // 0th = finished / new start
            HandleDeserialization();
        }

        private void HandleDeserialization()
        {
            deserializationAttempts++;
            
            var owner = Networking.GetOwner(gameObject);
            if (!CanEdit(owner))
            {
                if (deserializationAttempts >= maxDeserializationAttempts)
                {
                    MUIDebug.LogError($"{owner.displayName} is still not whitelisted! Aborting deserialization on {gameObject.name} after {deserializationAttempts} attempts.", gameObject);
                    deserializationAttempts = 0;
                    return;
                }
                
                MUIDebug.LogWarning($"{owner.displayName} is not whitelisted but tried to send network data on {gameObject.name}. Retrying in {deserializationRetryDelay}s, Attempt {deserializationAttempts} / {maxDeserializationAttempts}.", gameObject);
                scheduledDeserializations++;
                SendCustomEventDelayedSeconds(nameof(_DelayedDeserialization), deserializationRetryDelay);
                return;
            }
            
            if (deserializationAttempts > 1)
                MUIDebug.Log($"{owner.displayName} is now whitelisted! Running Deserialization on {gameObject.name} after {deserializationAttempts} attempts.", gameObject);
            deserializationAttempts = 0;
            ApplyNetData();
        }
        #endregion

        private void ApplyNetData()
        {
            if (VRCJson.TryDeserializeFromJson(serializedAddedBy, out DataToken addedByResult))
            {
                usersAddedBy = addedByResult.DataDictionary;
            }
            
            if (VRCJson.TryDeserializeFromJson(serializedWhitelist, out DataToken whitelistResult))
            {
                whitelist = whitelistResult.DataList;
                base.WhitelistUpdated(); // Calls base class to skip the added access checks
            }
        }
        
        public override void OnPreSerialization()
        {
            if (VRCJson.TrySerializeToJson(whitelist, JsonExportType.Minify, out DataToken whitelistResult))
            {
                serializedWhitelist = whitelistResult.String;
            }

            if (VRCJson.TrySerializeToJson(usersAddedBy, JsonExportType.Minify, out DataToken addedByResult))
            {
                serializedAddedBy = addedByResult.String;
            }
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (CanEdit(newOwner))
            {
                MUIDebug.Log($"Synced Whitelist: {requester.displayName} has made {newOwner.displayName} the new Network Owner of {gameObject.name}.", gameObject);
                return true; 
            }

            MUIDebug.LogError($"Synced Whitelist: {requester.displayName} has tried to make {newOwner.displayName} the Network Owner of {gameObject.name}, but isn't allowed to!", gameObject);
            return false;
        }
        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            CheckOwnership();
        }

        private void CheckOwnership()
        {
            canEdit = CanEdit(Networking.LocalPlayer);
            if (!Networking.LocalPlayer.isMaster) return;
            
            var owner = Networking.GetOwner(gameObject);
            if (adminWhitelist == null || CanEdit(owner)) return;
            MUIDebug.LogWarning($"{owner.displayName} is the Network Owner of {gameObject.name}, but isn't whitelisted. " +
                                $"Trying to move ownership to a whitelisted player.", gameObject);

            var newOwner = adminWhitelist._GetPlayerInInstance();
            if (newOwner == null)
            {
                MUIDebug.LogWarning($"Could not find a whitelisted player to transfer ownership of {gameObject.name} to. " +
                                    $"Keeping current owner, but synced data may get lost." , gameObject);
                return;
            }
            
            MUIDebug.Log($"Found whitelisted player \"{newOwner.displayName}\" to transfer ownership of {gameObject.name} to.", gameObject);
            Networking.SetOwner(newOwner, gameObject);
        }

        //Networked Version of update Event, if local only is needed use base instead
        protected override void WhitelistUpdated(IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            var localPlayer = Networking.LocalPlayer;
            
            if (!CanEdit(localPlayer))
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            
            if (!localPlayer.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            RequestSerialization();

            base.WhitelistUpdated(senderBehaviour, senderEvent);
        }

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

        public override void _RemoveUsers(DataList usernamesToRemove, IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (!EditCheck()) return;
            base._RemoveUsers(usernamesToRemove, senderBehaviour, senderEvent);
        }

        public override void _RemoveUsers(string[] usernamesToRemove, IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (!EditCheck()) return;
            base._RemoveUsers(usernamesToRemove, senderBehaviour, senderEvent);
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

        public override void _ClearWhitelist(IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (!EditCheck()) return;
            base._ClearWhitelist(senderBehaviour, senderEvent);
        }

        public override void _RemoveAddedBy(string username, IUdonEventReceiver senderBehaviour = null, string senderEvent = null)
        {
            if (!EditCheck()) return;
            base._RemoveAddedBy(username, senderBehaviour, senderEvent);
        }

        #endregion
        
    }
}
    