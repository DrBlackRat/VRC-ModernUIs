using System.Drawing;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistUser : UdonSharpBehaviour
    {
        [SerializeField] protected TextMeshProUGUI usernameTMP;
        
        [SerializeField] protected GameObject buttonObj;
        [SerializeField] protected GameObject declineButtonObj;
        
        protected bool isRemover;

        protected WhitelistEditorBase WhitelistEditorBase;
        
        protected string displayName;
        protected bool isRequestor;
        public string DisplayName
        {
            set
            {
                displayName = value;
                if (usernameTMP != null) usernameTMP.text = value;
            }
            get => displayName;
        }
        
        protected bool hasAccess;
        public bool HasAccess
        {
            set
            {
                hasAccess = value;
                if (buttonObj != null) buttonObj.SetActive(value);
            }
            get => hasAccess;
        }
        
        public void _Setup(WhitelistEditorBase newWhitelistEditorBase, string newDisplayname, bool newIsRemover, bool newHasAccess)
        {
            WhitelistEditorBase = newWhitelistEditorBase;
            DisplayName = newDisplayname;
            isRemover = newIsRemover;
            HasAccess = newHasAccess;

            isRequestor = WhitelistEditorBase.GetUdonTypeID() == GetUdonTypeID<WhitelistRequestor>();
            if (declineButtonObj != null) declineButtonObj.SetActive(isRequestor && !isRemover);
        }

        public void _ButtonPressed()
        {
            if (!hasAccess) return;
            if (isRemover)
            {
                WhitelistEditorBase._Remove(displayName);
            }
            else
            {
                WhitelistEditorBase._Add(displayName);
            }
        }
        
        public void _DeclinePressed()
        {
            if (!hasAccess || isRemover) return;
            var requestor = (WhitelistRequestor)WhitelistEditorBase;
            requestor._Decline(displayName);
        }

        public void _Destroy()
        {
            Destroy(gameObject);
        }
    }
}
