
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InstanceOptionsWhitelist : WhitelistGetterBase
    {
        [Tooltip("Automatically adds the Instance Owner to the whitelist.\nAn Instance Owner only exists in Invite, Invite+, Friends and Friends+ Instances")]
        [SerializeField] private bool allowInstanceOwner;
        [Tooltip("Automatically adds the current Instance Master to the whitelist.")]
        [SerializeField] private bool allowInstanceMaster;
        [Tooltip("Automatically adds the first player who joins to the whitelist. \nUseful for when you want the Instance Owner, but need it to work in Group and Public instances.")]
        [SerializeField] private bool allowFirstPlayer;

        private string instanceOwnerName;
        private string currentMasterName;
        [UdonSynced] private string firstPlayer;

        protected override void Start()
        {
            base.Start();
            if (allowFirstPlayer && Networking.LocalPlayer.IsOwner(gameObject))
            {
                firstPlayer = localName;
                Add(firstPlayer);
                
                RequestSerialization();
            }
        }

        public override void OnDeserialization()
        {
            Add(firstPlayer);
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            
            if (allowInstanceOwner && player.isInstanceOwner)
            {
                instanceOwnerName = displayName;
                Add(displayName);
            }
            
            if (allowInstanceMaster && player.isMaster)
            {
                currentMasterName = displayName;
                Add(displayName);
            }
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            if (!allowInstanceMaster) return;
            var displayName = newMaster.displayName;

            if (currentMasterName != null && instanceOwnerName != currentMasterName && firstPlayer != currentMasterName)
            {
                if (whitelist.Contains(currentMasterName))
                    whitelist.Remove(currentMasterName);
            }
            
            Add(displayName);
        }
        
        /// <summary>
        /// Adds a player if they aren't already on the list.
        /// </summary>
        /// <param name="displayName"> Display Name of the player you want to add.</param>
        private void Add(string displayName)
        {
            if (!whitelist.Contains(displayName))
            {
                whitelist.Add(displayName);
                WhitelistUpdated();
            }
        }

        /// <summary>
        /// Adds a player if they are on the list.
        /// </summary>
        /// <param name="displayName">Display Name of the player you want to remove.</param>
        private void Remove(string displayName)
        {
            if (whitelist.Contains(displayName))
            {
                whitelist.Remove(displayName);
                WhitelistUpdated();
            }
        }

        #region Get Added By Overrides
        public override DataList _GetUsersAddedBy(string username)
        {
            if (username == localName) 
                return whitelist.DeepClone();
            return new DataList();
        }

        public override int _GetUsersAddedByCount(string username)
        {
            if (username == localName) 
                return whitelist.Count;
            return 0;
        }

        public override string _GetWhoAdded(string username)
        {
            return localName;
        }
        #endregion

    }
}
