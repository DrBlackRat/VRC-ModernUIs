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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Whitelist : WhitelistSetterBase
    {
        [Tooltip("Display Name of each User you would want to be on the whitelist. Only correct on start!")]
        [SerializeField] protected string[] startWhitelist;
        
        [Tooltip("Automatically remove a player when they leave the instance.")]
        [SerializeField] protected bool autoRemoveOnLeave;

        protected override void Start()
        {
            localName = Networking.LocalPlayer.displayName;
            _AddUsers(startWhitelist);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);
            if (autoRemoveOnLeave)
                _RemoveUser(player.displayName);
        }
    }
}
