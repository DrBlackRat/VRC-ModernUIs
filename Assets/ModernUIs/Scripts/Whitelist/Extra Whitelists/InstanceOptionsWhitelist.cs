
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
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InstanceOptionsWhitelist : WhitelistGetterBase
    {
        [Tooltip("Automatically adds the Instance Owner to the whitelist.\nAn Instance Owner only exists in Invite, Invite+, Friends and Friends+ Instances")]
        [SerializeField] private bool allowInstanceOwner;
        [Tooltip("Automatically adds the current Instance Master to the whitelist.")]
        [SerializeField] private bool allowInstanceMaster;

        private string instanceOwnerName;
        private string currentMasterName;
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            
            if (allowInstanceOwner && player.isInstanceOwner)
            {
                instanceOwnerName = displayName;
                
                if (!whitelist.Contains(displayName))
                {
                    whitelist.Add(displayName);
                    WhitelistUpdated();
                }
            }
            
            if (allowInstanceMaster && player.isMaster)
            {
                currentMasterName = displayName;
                
                if (!whitelist.Contains(displayName))
                {
                    whitelist.Add(displayName);
                    WhitelistUpdated();
                }
            }
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            if (!allowInstanceMaster) return;
            var displayName = newMaster.displayName;

            if (currentMasterName != null && !currentMasterName.Equals(instanceOwnerName))
            {
                whitelist.Remove(currentMasterName);
            }
            
            if (!whitelist.Contains(displayName))
            {
                whitelist.Add(newMaster.displayName);
            }
            
            WhitelistUpdated();
        }
    }
}
