using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Economy;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EconomySelectorUIButton : SelectorUIButton
    {
        [Header("Creator Economy:")] 
        [Tooltip("If enabled, a user will be treated as if they own the product, useful for testing.")]
        [SerializeField] protected bool owned;
        [Tooltip("Product a user needs to own to be able to select this button.")]
        [SerializeField] protected UdonProduct product;

        public override void _Setup(Color newNormalColor, Color newSelectedColor, AnimationCurve newSmoothingCurve,
            float newMovementDuration, SelectorUI newSelectorUI, int newButtonId)
        {
            base._Setup(newNormalColor, newSelectedColor, newSmoothingCurve, newMovementDuration, newSelectorUI, newButtonId);
            _UpdateInteractable(owned);
        }

        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            owned = true;
            _UpdateInteractable(owned);
        }
        
        public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            owned = false;
            _UpdateInteractable(owned);
        }
    }
}
