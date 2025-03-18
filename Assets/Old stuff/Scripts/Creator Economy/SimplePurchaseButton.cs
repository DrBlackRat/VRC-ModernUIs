using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SimplePurchaseButton : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [SerializeField] public bool owned = false;
        [SerializeField] private UdonProduct product;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip purchaseSound;
        
        [Header("Internals:")]
        [SerializeField] private GameObject purchaseObj;
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
