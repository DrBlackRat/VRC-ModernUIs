using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistObjectToggle : UdonSharpBehaviour
    {
        [Tooltip("Whitelist Manager that is storing info on which user is whitelisted.")]
        [SerializeField] private WhitelistManager whitelistManager;
        
        [Tooltip("Objects that should only be enabled for whitelisted users.")]
        [SerializeField] private GameObject[] whitelistedObjs;
        [Tooltip("Objects that should be enabled for everyone, but disabled for whitlisted users.")]
        [SerializeField] private GameObject[] notWhitelistedObjs;
        
        private void Start()
        {
            whitelistManager._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            var isWhitelisted = whitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer);

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


