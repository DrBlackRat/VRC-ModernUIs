using System;
using System.Linq;
using System.Text;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistManager : WhitelistSetterBase
    {
        [Tooltip("Display Name of each User you would want to be on the whitelist. Only correct on start!")]
        [SerializeField] protected string[] startWhitelist;
        [Tooltip("Automatically adds the Instance Owner to the whitelist.\nAn Instance Owner only exists in Invite, Invite+, Friends and Friends+ Instances")]
        [SerializeField] protected bool allowInstanceOwner;
        [Tooltip("Automatically adds the Instance Master to the whitelist. \nIf the master changes the new master will be added, but the old one wont be removed. (This is due to technical limitations)")]
        [SerializeField] protected bool allowInstanceMaster;
        [FormerlySerializedAs("productAccess")]
        [Tooltip("Automatically adds everyone in the instance who owns one of these Udon Product to the Whitelist. \nIf the product expires while the user is in the instance they wont be removed unless they rejoin.")]
        [SerializeField] protected UdonProduct[] products;

        protected virtual void Start()
        {
            foreach (var username in startWhitelist)
            {
                if (whitelist.Contains(username)) continue;
                whitelist.Add(username);
            }
            WhitelistUpdated();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            if (whitelist.Contains(displayName)) return;
            
            if (allowInstanceOwner && player.isInstanceOwner)
            {
                whitelist.Add(displayName);
                WhitelistUpdated();
            }

            if (allowInstanceMaster && player.isMaster)
            {
                whitelist.Add(displayName);
                WhitelistUpdated();
            }
        }
        
        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            foreach (var product in products)
            {
                if (result.ID != product.ID) return;
            }
            _AddUser(player.displayName);
        }
        
    }
}
