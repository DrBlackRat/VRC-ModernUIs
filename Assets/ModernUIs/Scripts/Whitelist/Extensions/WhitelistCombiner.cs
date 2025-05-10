using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(900)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistCombiner : WhitelistManager
    {
        
        [Tooltip("Whitelists that you want to combine into one. Useful for combining a local and synced whitelist.")]
        [SerializeField] protected WhitelistManager[] whitelistManagers;
        
        protected override void Start()
        {
            foreach (var whitelistManager in whitelistManagers)
            {
                whitelistManager._SetUpConnection((IUdonEventReceiver)this);
            }
            WhitelistUpdated(true);
        }

        public void _WhitelistUpdated()
        {
            if (whitelistManagers == null || whitelistManagers.Length == 0) return;
            
            whitelist.Clear();
            foreach (var whitelistManager in whitelistManagers)
            {
                whitelist.AddRange(whitelistManager._GetUsersAsList());
            }
            WhitelistUpdated( false);
        }

        #region Overrides 
        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.
        /// </summary>
        public override void _AddUser(string username, IUdonEventReceiver senderBehaviour)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be removed from a whitelist combiner. Remove them from the original whitelist instead.
        /// </summary>
        public override void _RemoveUser(string username, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be removed from a whitelist combiner. Remove them from the original whitelist instead.");
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.
        /// </summary>
        public override void _AddUsers(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }
        
        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.
        /// </summary>
        public override void _AddUsers(string[] newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Whitelist can't directly be replaced on a whitelist combiner.
        /// </summary>
        public override void _ReplaceWhitelist(DataList newUsernames, IUdonEventReceiver senderBehaviour = null)
        {
            MUIDebug.LogError("Whitelist Combiner: Whitelist can't directly be replaced on a whitelist combiner.");
        }
        #endregion
        
    }
}
