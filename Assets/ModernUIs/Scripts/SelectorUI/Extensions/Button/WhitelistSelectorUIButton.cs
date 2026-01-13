
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistSelectorUIButton : SelectorUIButton
    {
        [Tooltip("Whitelist used to lock this specific Selector UI Button.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("If enabled the button will be fully disabled when not whitelisted. (It wont be visible)")]
        [SerializeField] private bool disableButton;

        
        public override void _Setup(SelectorUI newSelectorUI, int newButtonId)
        {
            selectable = false;
            base._Setup(newSelectorUI, newButtonId);
            
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            selectable = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);

            if (disableButton)
            {
                gameObject.SetActive(selectable);
                selectorUI._ForceUpdateSelector();

                if (selected && !selectable && !locked)
                {
                    int id = buttonId > 0 ? buttonId - 1 : buttonId + 1;
                    selectorUI._ButtonSelected(id);
                }
            }
            
            UpdateInteractable();
        }
    }
}
