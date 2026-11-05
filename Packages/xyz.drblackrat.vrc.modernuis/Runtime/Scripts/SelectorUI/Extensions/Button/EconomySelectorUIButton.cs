using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Economy;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EconomySelectorUIButton : SelectorUIButton
    {
        [Tooltip("Product a user needs to own to be able to select this button.")]
        [SerializeField] protected UdonProduct product;

        public override void _Setup(SelectorUI newSelectorUI, int newButtonId)
        {
            selectable = false;
            base._Setup(newSelectorUI, newButtonId);
        }

        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            selectable = true;
            UpdateInteractable();
        }
        
        public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            selectable = false;
            UpdateInteractable();
        }

        public override void _ButtonPressed()
        {
            if (!selectable) return;
            base._ButtonPressed();
        }
    }
}
