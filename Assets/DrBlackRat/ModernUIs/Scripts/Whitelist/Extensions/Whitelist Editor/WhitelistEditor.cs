using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using TMPro;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistEditor : WhitelistEditorBase
    {
        protected DataList allUsers = new DataList();
        protected DataList prevAdmins = new DataList();

        #region Unity, VRChat & ModernUIs Events
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var username = player.displayName;
            allUsers.Add(username);
            AddOtherUser(username, false);
            UpdateCountDisplays();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            
            allUsers.Remove(displayName);
            RemoveOtherUser(displayName, false);
            UpdateCountDisplays();
        }

        public override void _AdminWhitelistUpdated()
        {
            UpdateAccess(adminWhitelist._IsPlayerWhitelisted(localName));
            
            if (hasAdminWhitelist && !showOrAllowAdmins)
            {
                var admins = adminWhitelist._GetUsersAsList();
                
                for (var i = 0; i < admins.Count; i++)
                {
                    RemoveOtherUser(admins[i].String, false); 
                }
                for (var i = 0; i < prevAdmins.Count; i++)
                {
                    if (admins.Contains(prevAdmins[i])) continue;
                    
                    AddOtherUser(prevAdmins[i].String, false); 
                }
                prevAdmins = admins;
            }
            
            UpdateCountDisplays();
            
            addedList = this.whitelist._GetUsersAddedBy(localName);
            CheckMaxUsersAdded();
        }

        #endregion
        
        #region Adding & Removing Users
        protected override bool RemoveWhitelistUser(string username)
        {
            if (!base.RemoveWhitelistUser(username))
                return false;
            
            AddOtherUser(username, false);
            
            return true;
        }
        
        protected override bool AddOtherUser(string username, bool sendNet)
        {
            if (allUsers.Contains(username))
            {
                return base.AddOtherUser(username, sendNet);
            }
                
            return false;
        }
        #endregion
    }
}

