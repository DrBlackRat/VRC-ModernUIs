using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using Texel;


namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessTXLWhitelistBridge : WhitelistManager
    {
        public AccessControlUserList accessTxlList;
        public string[] txlNames;

        protected override void Start()
        {
            //accessTxlList._Register(AccessControlUserSource.EVENT_REVALIDATE, this, nameof(_UpdateAccessList));
            _UpdateAccessList();
        }

        public void _UpdateAccessList()
        {
            Debug.LogError("LIST UPDATEDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            //txlNames = accessTxlList.UserList;

            // add names to list
            foreach (var displayname in txlNames)
            {
                Debug.LogWarning(displayname);
                if (displayname == null || whitelist.Contains(displayname)) continue;
                whitelist.Add(displayname);
            }
            
            // remove names from list
            var copyList = whitelist.ShallowClone();
            for (int i = 0; i < copyList.Count; i++)
            {
                if (txlNames.Contains((string)copyList[i])) continue;
                whitelist.Remove(copyList[i]);
            }
            
            WhitelistUpdated(false);
        }
        
        #region Overrides 
        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }

        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be removed from a whitelist combiner. Remove them from the original whitelist instead.");
        }

        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }
        
        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }

        /// <summary>
        /// DISABLED ON AccessTXLWhitelistBridge
        /// </summary>
        public override void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Whitelist can't directly be replaced on a whitelist combiner.");
        }
        #endregion
    }

}
