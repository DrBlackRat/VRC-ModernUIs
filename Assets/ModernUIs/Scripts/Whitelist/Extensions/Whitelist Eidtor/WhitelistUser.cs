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
        [SerializeField] private TextMeshProUGUI usernameTMP;
        [SerializeField] private GameObject buttonObj;
        
        private bool isRemover;

        private WhitelistEditor whitelistEditor;
        
        private string displayName;
        public string DisplayName
        {
            set
            {
                displayName = value;
                usernameTMP.text = value;
            } 
        }
        
        private bool hasAccess;
        public bool HasAccess
        {
            set
            {
                hasAccess = value;
                buttonObj.SetActive(value);
            }
        }
        
        public void _Setup(WhitelistEditor newWhitelistEditor, string newDisplayname, bool newIsRemover, bool newHasAccess)
        {
            whitelistEditor = newWhitelistEditor;
            DisplayName = newDisplayname;
            isRemover = newIsRemover;
            HasAccess = newHasAccess;
        }

        public void _ButtonPressed()
        {
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
