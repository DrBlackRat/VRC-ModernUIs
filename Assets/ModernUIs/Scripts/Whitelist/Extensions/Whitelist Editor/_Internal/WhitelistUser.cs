using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistUser : UdonSharpBehaviour
    {
        [SerializeField] protected TextMeshProUGUI usernameTMP;
        [SerializeField] protected GameObject buttonObj;
        
        protected bool isRemover;

        protected IWhitelistEditor whitelistEditor;
        
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
        
        public void _Setup(IWhitelistEditor newWhitelistEditor, string newDisplayname, bool newIsRemover, bool newHasAccess)
        {
            whitelistEditor = newWhitelistEditor;
            DisplayName = newDisplayname;
            isRemover = newIsRemover;
            HasAccess = newHasAccess;
        }

        public void _ButtonPressed()
        {
            if (!hasAccess) return;
            if (isRemover)
            {
                whitelistEditor._Remove(displayName);
            }
            else
            {
                whitelistEditor._Add(displayName);
            }
        }

        public void _Destroy()
        {
            Destroy(gameObject);
        }
    }
}
