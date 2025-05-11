using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistRequestingUser : UdonSharpBehaviour
    {
        [SerializeField] protected TextMeshProUGUI usernameTMP;
        [SerializeField] protected GameObject buttonObj;
        
        protected WhitelistRequestor whitelistRequestor;
        
        protected string displayName;
        public string DisplayName
        {
            set
            {
                displayName = value;
                if (usernameTMP != null) usernameTMP.text = value;
            } 
        }
        
        protected bool hasAccess;
        public bool HasAccess
        {
            set
            {
                hasAccess = value;
                if (buttonObj != null) buttonObj.SetActive(value);
            }
        }
        
        public void _Setup(WhitelistRequestor newWhitelistRequestor, string newDisplayname, bool newHasAccess)
        {
            whitelistRequestor = newWhitelistRequestor;
            DisplayName = newDisplayname;
            HasAccess = newHasAccess;
        }

        public void _AddPressed()
        {
            whitelistRequestor._Add(displayName);
        }
        
        public void _DeclinePressed()
        {
            whitelistRequestor._Decline(displayName);
        }

        public void _Destroy()
        {
            Destroy(gameObject);
        }
    }
}
