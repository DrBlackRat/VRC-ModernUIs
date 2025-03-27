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
        [Header("Whitelist Combiner:")]
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
    }
}
