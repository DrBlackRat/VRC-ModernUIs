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
    public class SyncedWhitelistManager : WhitelistManager
    {
        [FormerlySerializedAs("adminWhitelistManager")]
        [Tooltip("Whitelist of Admins which can change the Synced Whitelist without being on it them self.")]
        [SerializeField] protected WhitelistGetterBase adminWhitelist;
        [Tooltip("Allow none Admins, but users who are on the whitelist it self to change it. Will be set to true if no Admin Whitelist is provided.")]
        [SerializeField] protected bool noneAdminAccess;
        
        [Tooltip("Automatically remove a player when they leave the instance.")]
        [SerializeField] protected bool autoRemoveOnLeave;
        [Tooltip("Automatically removes every player if the person that added them left.")]
        [SerializeField] protected bool autoRemoveOnAdderLeft;
        
        [UdonSynced] protected string serializedWhitelist;
        protected bool canEdit;
        
        protected DataList addedBy = new DataList();
        [UdonSynced] protected string serializedAddedBy;

        protected string localName;

        protected override void Start()
        {
            localName = Networking.LocalPlayer.displayName;
            
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                foreach (var username in startWhitelist)
                {
                    if (whitelist.Contains(username)) continue;
                    addedBy.Add(localName);
                    whitelist.Add(username);
                }
                WhitelistUpdated();
            }

            if (adminWhitelist != null)
            {
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                noneAdminAccess = true;
            }
            canEdit = CanEdit(Networking.LocalPlayer);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
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
            return (noneAdminAccess && _IsPlayerWhitelisted(player)) || 
                   (adminWhitelist != null && adminWhitelist._IsPlayerWhitelisted(player)) ||
                   player.IsOwner(gameObject);
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
        
        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(serializedAddedBy, out DataToken addedByResult))
            {
                addedBy = addedByResult.DataList;
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

            if (VRCJson.TrySerializeToJson(addedBy, JsonExportType.Minify, out DataToken addedByResult))
            {
                serializedAddedBy = addedByResult.String;
            }
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (CanEdit(newOwner) || newOwner.IsOwner(gameObject))
            {
                MUIDebug.Log($"Whitelist Manager: {requester.displayName} has made {newOwner.displayName} the new Network Owner.");
                return true; 
            }
            else
            {
                MUIDebug.LogError($"Whitelist Manager: {requester.displayName} has tried to make {newOwner.displayName} the Network Owner, but isn't allowed to!");
                return false;
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            canEdit = CanEdit(Networking.LocalPlayer);
        }

        //Networked Version of update Event, if local only is needed use base instead
        protected override void WhitelistUpdated(IUdonEventReceiver senderBehaviour = null)
        {
            var localPlayer = Networking.LocalPlayer;
            
            if (!CanEdit(localPlayer) && !localPlayer.IsOwner(gameObject))
            {
                MUIDebug.LogError("Whitelist Manager: You are not whitelisted!");
                return;
            }
            
            if (!localPlayer.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            RequestSerialization();

            base.WhitelistUpdated(senderBehaviour);
        }

        #region Add & Remove Overrides
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour = null, bool notify = true)
        {
            if (!EditCheck()) return;
            addedBy.Add(localName);
            base._AddUser(username, senderBehaviour, notify);
        }

        public override void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;

            var index = whitelist.IndexOf(username);
            if (index != -1)
            {
                addedBy.RemoveAt(index);
            }
            
            base._RemoveUser(username, senderBehaviour);
        }

        public override void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            base._AddUsers(newUsernames, senderBehaviour);
        }
        
        public override void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            base._AddUsers(newUsernames, senderBehaviour);
        }

        public override void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;

            addedBy = new DataList();
            for (int i = 0; i < newUsernames.Count; i++)
            {
                addedBy.Add(localName);
            }
            
            base._ReplaceWhitelist(newUsernames, senderBehaviour);
        }

        public override void _ReplaceWhitelist(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            if (!EditCheck()) return;
            
            addedBy = new DataList();
            for (int i = 0; i < newUsernames.Length; i++)
            {
                addedBy.Add(localName);
            }
            
            base._ReplaceWhitelist(newUsernames, senderBehaviour);
        }
        #endregion

        #region Added By
        /// <summary>
        /// Allows you to check who added a specific user.
        /// </summary>
        /// <param name="username">Display Name of the user you want to check.</param>
        /// <returns>Display Name of the user who added them, null if that user is not whitelisted.</returns>
        public string _AddedBy(string username)
        {
            var index = whitelist.IndexOf(username);
            if (index != -1)
            {
                return addedBy[index].String;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns how many users a single user has added. This is slow! So please cache this if possible!
        /// </summary>
        /// <param name="username">Display Name of the user you want to check.</param>
        /// <returns>Count of how many people this user has added.</returns>
        public int _AddedCount(string username)
        {
            DataList sorted = addedBy.ShallowClone();
            sorted.Sort();

            int count = 0;
            bool prevMatched = false;
            for (int i = 0; i < addedBy.Count; i++)
            {
                if (addedBy[i].String == username)
                {
                    count++;
                    prevMatched = true;
                }
                else if (prevMatched) // Reached end of our matched user occuring
                {
                    break;
                }
            }

            return count;
        }

        /// <summary>
        /// Removed everyone that was added by the provided user.
        /// </summary>
        /// <param name="username">Display Name of the user who you want to remove ever added user of.</param>
        public void _RemoveAddedBy(string username)
        {
            if (!EditCheck()) return;

            for (int i = addedBy.Count - 1; i >= 0; i--)
            {
                if (addedBy[i].String == username)
                {
                    addedBy.RemoveAt(i);
                    whitelist.RemoveAt(i);
                }
            }
            
            WhitelistUpdated();
        }
        #endregion
        
    }
}
    