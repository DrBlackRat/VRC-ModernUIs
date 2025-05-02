using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(900)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class WhitelistCombiner : WhitelistManager
    {
        [Tooltip("Whitelists that you want to combine into one. Useful for combining a local and synced whitelist.")]
        [SerializeField] protected WhitelistManager[] whitelistManagers;
        
        protected override void Start()
        {
            base.Start();
            foreach (var whitelistManager in whitelistManagers)
            {
                whitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
            }
        }

        public void _WhitelistUpdated()
        {
            if (whitelistManagers == null || whitelistManagers.Length == 0) return;
            var users = new string[0];
            foreach (var whitelistManager in whitelistManagers)
            {
                users = ArrayExtensions.Combine(users, whitelistManager._GetNames());
            }
            users = users.Distinct();
            ChangeWhitelist(users, false);
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.
        /// </summary>
        public override void _AddUser(string username)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be removed from a whitelist combiner. Remove them from the original whitelist instead.
        /// </summary>
        public override void _RemoveUser(string username)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be removed from a whitelist combiner. Remove them from the original whitelist instead.");
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.
        /// </summary>
        public override void _AddUsers(string[] newUsernames)
        {
            MUIDebug.LogError("Whitelist Combiner: Users can't directly be added to a whitelist combiner. Add another whitelist to the combiner instead.");
        }

        /// <summary>
        /// DISABLED ON COMBINER
        /// Whitelist can't directly be replaced on a whitelist combiner.
        /// </summary>
        public override void _ReplaceWhitelist(string[] newUsernames)
        {
            MUIDebug.LogError("Whitelist Combiner: Whitelist can't directly be replaced on a whitelist combiner.");
        }
    }
}
