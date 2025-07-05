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
    public class WhitelistManager : WhitelistSetterBase
    {
        [Tooltip("Display Name of each User you would want to be on the whitelist. Only correct on start!")]
        [SerializeField] protected string[] startWhitelist;

        protected virtual void Start()
        {
            foreach (var username in startWhitelist)
            {
                if (whitelist.Contains(username)) continue;
                whitelist.Add(username);
            }
            WhitelistUpdated();
        }
        
    }
}
