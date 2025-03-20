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
    [DefaultExecutionOrder(-200)]
    public class SESUButton : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [SerializeField] public bool owned = false;
        [SerializeField] private UdonProduct product;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip purchaseSound;
        
        [Header("Internals:")]
        [SerializeField] private Button button;
        [SerializeField] private GameObject purchaseObj;
        [SerializeField] private GameObject ownedObj;
        
        private int item;
        private SyncedEconomySelectorUI manager;

        private bool locked;

        private void Start()
        {
            UpdateUI();
        }

        public bool _Owned()
        {
            return owned;
        }

        public void _SetUpConnection(SyncedEconomySelectorUI newManager, int newItem)
        {
            manager = newManager;
            item = newItem;
        }
        public void _UpdateLock(bool newLocked)
        {
            locked = newLocked;
            UpdateUI();
        }
        
        public void _SelectItem()
        {
            if (locked) return;
            manager._ChangeItem(item, false, false);
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
            button.interactable = owned && !locked;
            if (purchaseObj) purchaseObj.SetActive(!owned);
            if (ownedObj) ownedObj.SetActive(owned);
        }
    }
}
