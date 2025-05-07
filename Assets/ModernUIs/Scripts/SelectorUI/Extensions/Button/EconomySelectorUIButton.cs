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
        [Tooltip("If enabled, a user will be treated as if they own the product, useful for testing.")]
        [SerializeField] protected bool owned;
        [Tooltip("Product a user needs to own to be able to select this button.")]
        [SerializeField] protected UdonProduct product;
        
        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            owned = true;
            _UpdateLocked(locked);
        }
        
        public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            owned = false;
            _UpdateLocked(locked);
        }

        public override void _ButtonPressed()
        {
            if (!owned) return;
            base._ButtonPressed();
        }

        public override void _UpdateLocked(bool newLocked)
        {
            locked = newLocked;
            button.interactable = !locked && owned;
        }
    }
}
