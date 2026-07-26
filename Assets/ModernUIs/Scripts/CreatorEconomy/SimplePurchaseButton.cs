using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SimplePurchaseButton : UdonSharpBehaviour
    {
        // Setup
        [SerializeField, FormerlySerializedAs("owned")]
        [Tooltip("If enabled, the button will act as if the products are owned by the local player.\nShould only be used for debugging.")]
        private bool forceOwned = false;
        
        [SerializeField]
        [Tooltip("If enabled, the player needs to own all products (including the whitelist if provided) to be counted as owning.")]
        private bool requireAllProducts = false;
        [SerializeField, FormerlySerializedAs("product")]
        [Tooltip("Udon Products the player needs to own.")]
        private UdonProduct[] products;
        [SerializeField]
        [Tooltip("Whitelist that can be used as a \"Product\", being on it is similar to owning it.")]
        private WhitelistGetterBase whitelist;
        
        // Sound Effects
        [SerializeField]
        [Tooltip("Audio Source that will be used to play the Purchase Sound.")]
        private AudioSource audioSource;
        [SerializeField]
        [Tooltip("Sound that will be played if a player buys the product.")]
        private AudioClip purchaseSound;
        [SerializeField]
        [Tooltip("Sound that will be played if a player buys the product.")]
        private AudioClip expiredSound;
        
        // External Connection
        [SerializeField, FormerlySerializedAs("purchaseObj")]
        [Tooltip("Objects that will be enabled if the player does not own the products.")]
        private GameObject[] notOwnedObjects;
        [SerializeField]
        [FormerlySerializedAs("ownedObj")]
        [Tooltip("Objects that will be enabled if the player owns the products.")]
        private GameObject[] ownedObjects;

        [SerializeField]
        private UdonBehaviour[] eventReceivers;
        [SerializeField]
        [Tooltip("Event to be called on the Event Receivers if one of the products was purchased.")]
        private string onPurchaseEventName = "_OnPurchased";
        [SerializeField]
        [Tooltip("Event to be called on the Event Receivers if the all products have expired.")]
        private string onExpiredEventName = "_OnExpired";
        
        private bool isWhitelisted = false;
        
        private int totalProductsCount = 0;
        private int productsOwnedCount = 0;
        private bool ownsProducts = false;

        #region Unity, VRChat & ModernUI Events
        private void Start()
        {
            totalProductsCount = products.Length;
            ownsProducts = forceOwned;
            UpdateExternal();
            
            if (forceOwned || whitelist == null)
            {
                return;
            }
            totalProductsCount++;
            whitelist._SetUpConnection(this, nameof(_OnWhitelistUpdated));
        }

        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (!player.isLocal || forceOwned) 
            {
                return;
            }

            foreach (var product in products)
            {
                if (product == null || product.ID != result.ID)
                {
                    continue;
                }
                
                productsOwnedCount++;
                UpdateOwnershipStateAndPlaySound(purchased);
                break;
            }
        }

        public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
        {
            if (!player.isLocal || forceOwned)
            {
                return;
            }

            foreach (var product in products)
            {
                if (product == null || product.ID != result.ID)
                {
                    continue;
                }
                
                productsOwnedCount--;
                UpdateOwnershipStateAndPlaySound(true);
                break;
            }
        }
        
        public void _OnWhitelistUpdated()
        {
            var prevIsWhitelisted = isWhitelisted;
            isWhitelisted = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);

            if (prevIsWhitelisted == isWhitelisted)
            {
                return;
            }

            productsOwnedCount += isWhitelisted ? 1 : -1;
            UpdateOwnershipStateAndPlaySound(true);
        }
        #endregion

        private void UpdateOwnershipStateAndPlaySound(bool playSound)
        {
            var newOwnsProducts = requireAllProducts ? productsOwnedCount == totalProductsCount : productsOwnedCount > 0;
            
            if (newOwnsProducts == ownsProducts)
            {
                return;
            }
            
            ownsProducts = newOwnsProducts;
            UpdateExternal();
            
            if (!playSound)
            {
                return;
            }
            TryPlaySound(ownsProducts ? purchaseSound : expiredSound);
        }
        
        private void UpdateExternal()
        {
            var eventName = ownsProducts ? onPurchaseEventName : onExpiredEventName;
            foreach (var receiver in eventReceivers)
            {
                if (receiver == null)
                    continue;
                
                receiver.SendCustomEvent(eventName);
            }
            
            foreach (var obj in ownedObjects)
            {
                if (obj == null) continue;
                obj.SetActive(ownsProducts);
            }

            foreach (var obj in notOwnedObjects)
            {
                if (obj == null) continue;
                obj.SetActive(!ownsProducts);
            }
        }

        private bool TryPlaySound(AudioClip clip)
        {
            if (audioSource == null || clip == null)
            {
                return false;
            }
            
            audioSource.PlayOneShot(clip);
            return true;
        }
    }
}
