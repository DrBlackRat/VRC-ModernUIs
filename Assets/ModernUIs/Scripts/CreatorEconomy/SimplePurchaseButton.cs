using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SimplePurchaseButton : UdonSharpBehaviour
    {
        [Tooltip("Enabled if a player owns the product, should be left as disabled other than for testing.")]
        [SerializeField] public bool owned = false;
        [Tooltip("Udon Product the player needs to own.")]
        [SerializeField] private UdonProduct product;
        
        [Tooltip("Audio Source that will be used to play the Purchase Sound.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Sound that will be played if a player buys the product.")]
        [SerializeField] private AudioClip purchaseSound;
        
        [Tooltip("Object that will be enabled when player DOESN'T own the product.")]
        [SerializeField] private GameObject purchaseObj;
        [Tooltip("Object that will be enabled when the player DOES own the product.")]
        [SerializeField] private GameObject ownedObj;

        private void Start()
        {
            UpdateUI();
        }

        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            owned = true;
            UpdateUI();
            
            if (purchased && audioSource != null) audioSource.PlayOneShot(purchaseSound);
        }

        public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            
            if (product == null) return;
            if (result.ID != product.ID) return;

            owned = false;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (purchaseObj) purchaseObj.SetActive(!owned);
            if (ownedObj) ownedObj.SetActive(owned);
        }
    }
}
