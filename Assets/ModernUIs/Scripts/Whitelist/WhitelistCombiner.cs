using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(800)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistCombiner : WhitelistGetterBase
    {
        
        [FormerlySerializedAs("whitelistManagers")]
        [Tooltip("Whitelists that you want to combine into one. Useful for combining a local and synced whitelist.")]
        [SerializeField] protected WhitelistGetterBase[] whitelists;
        
        private void Start()
        {
            foreach (var whitelist in whitelists)
            {
                whitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            WhitelistUpdated();
        }

        public void _WhitelistUpdated()
        {
            if (whitelists == null || whitelists.Length == 0) return;
            
            whitelist.Clear();
            foreach (var whitelistManager in whitelists)
            {
                var users = whitelistManager._GetUsersAsList();
                if (users == null) continue;
                whitelist.AddRange(users);
            }
            WhitelistUpdated( );
        }
    }
}
