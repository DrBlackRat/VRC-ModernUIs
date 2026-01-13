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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistCombiner : WhitelistGetterBase
    {
        [Tooltip("Remove duplicates from the combined whitelist. This is slow, so only use it when really necessary!")]
        [SerializeField] protected bool removeDuplicates;
        [FormerlySerializedAs("whitelistManagers")]
        [Tooltip("Whitelists that you want to combine into one. Useful for combining a local and synced whitelist.")]
        [SerializeField] protected WhitelistGetterBase[] whitelists;
        
        protected override void Start()
        {
            base.Start();
            
            foreach (var whitelist in whitelists)
            {
                whitelist._SetUpConnection((IUdonEventReceiver)this);
            }
        }

        public void _WhitelistUpdated()
        {
            if (whitelists == null || whitelists.Length == 0) return;
            whitelist.Clear();

            for (int i = 0; i < whitelists.Length; i++)
            {
                var users = whitelists[i]._GetUsersAsList();
                whitelist.AddRange(users);
            }
            
            if (removeDuplicates)
            {
                whitelist = whitelist.RemoveDuplicates();
            }

            WhitelistUpdated();
        }

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
            if (whitelist.Contains(username))
                return localName;
            return null;
        }
    }
}
