using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistObjectToggle : UdonSharpBehaviour
    {
        [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist Manager that is storing info on which user is whitelisted.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("Objects that should only be enabled for whitelisted users.")]
        [SerializeField] private GameObject[] whitelistedObjs;
        [Tooltip("Objects that should be enabled for everyone, but disabled for whitlisted users.")]
        [SerializeField] private GameObject[] notWhitelistedObjs;
        
        private void Start()
        {
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            var isWhitelisted = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);

            if (whitelistedObjs != null && whitelistedObjs.Length != 0)
            {
                foreach (var obj in whitelistedObjs)
                {
                    if (obj == null) continue;
                    obj.SetActive(isWhitelisted);
                }
            }

            if (notWhitelistedObjs != null && notWhitelistedObjs.Length != 0)
            {
                foreach (var obj in notWhitelistedObjs)
                {
                    if (obj == null) continue;
                    obj.SetActive(!isWhitelisted);
                }  
            }

        }
    }
}


