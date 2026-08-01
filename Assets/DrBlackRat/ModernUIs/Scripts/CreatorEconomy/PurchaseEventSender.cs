
using System;
using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PurchaseEventSender : UdonSharpBehaviour
    {
        [Tooltip("Udon Product to test for.")]
        [SerializeField] private UdonProduct udonProduct;
        [Tooltip("If enabled, the Purchase Event will only fire if the product was just bought.\nIf disabled it will always fire, even when a player who previously bought it joins.")]
        [SerializeField] private bool onlyPurchasedNow;

        [Tooltip("Udon Behaviours to send the events to.")]
        [SerializeField] private UdonBehaviour[] udonBehaviours;
        [Tooltip("Event that will be fired if the Local Player bought the Product.")]
        [SerializeField] private string purchaseEventName = "_ProductPurchased";
        [Tooltip("Event that will be fired if the product expired for the Local Player.")]
        [SerializeField] private string expiredEventName = "_ProductExpired";

        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }

        public override void OnPurchaseConfirmed(IProduct product, VRCPlayerApi player, bool purchasedNow)
        {
            if (player != localPlayer || product.ID != udonProduct.ID) return;
            if (onlyPurchasedNow && !purchasedNow) return;

            UpdateExternalConnection(true);
        }

        public override void OnPurchaseExpired(IProduct product, VRCPlayerApi player)
        {
            if (player != localPlayer || product.ID != udonProduct.ID) return;
            
            UpdateExternalConnection(false);
        }

        private void UpdateExternalConnection(bool purchased)
        {
            var eventName = purchased ? purchaseEventName : expiredEventName;
            if (udonBehaviours != null && udonBehaviours.Length > 0)
            {
                foreach (var behaviour in udonBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SendCustomEvent(eventName);
                }
            }
        }
    }
}

