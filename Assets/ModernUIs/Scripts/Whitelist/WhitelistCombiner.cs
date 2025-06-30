using System;
using System.Linq;
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
        [Tooltip("Remove duplicates from the combined whitelist. This is slow, so only use it when really necessary!")]
        [SerializeField] protected bool removeDuplicates;
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

            if (removeDuplicates)
            {
                whitelist = DataListExtensions.RemoveDuplicates(whitelist);
            }
            
            WhitelistUpdated();
        }


    }
}
