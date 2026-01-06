using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Economy;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CreatorEconomyWhitelist : WhitelistGetterBase
    {
        [Tooltip("List of Udon Products a player must at least own one of to be added to the whitelist.")]
        [SerializeField] protected UdonProduct[] products;
        
        [Tooltip("If enabled the Whitelist will be filled with all players that own at least one of the products. Otherwise only players in the instance will be added.")]
        [SerializeField] protected bool getAllProductOwners;
        [Range(5, 60)][Tooltip("How often the whitelist should be refreshed in Get All Mode in minutes.")]
        [SerializeField] protected float allProductOwnersRefreshRate = 10;
        
        protected override void Start()
        {
            base.Start();
            
            if (getAllProductOwners)
                _FillWithAllProductOwners();
        }

        #region Normal Mode
        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
            if (getAllProductOwners) return;
            var username = player.displayName;
            if (whitelist.Contains(username)) return;

            bool ownsProduct = false;
            foreach (var product in products)
            {
                if (result.ID == product.ID)
                {
                    ownsProduct = true;
                    break;
                }
            }

            if (ownsProduct)
            {
                whitelist.Add(username);
                WhitelistUpdated();
            }
        }

        public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
        {
            if (getAllProductOwners) return;
            var username = player.displayName;
            if (!whitelist.Contains(username)) return;
            
            foreach (var product in products)
            {
                // If they still own other products that grant them access don't remove them
                if (Store.DoesPlayerOwnProduct(player, product)) return;
            }

            whitelist.Remove(username);
            WhitelistUpdated();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);
            if (getAllProductOwners) return;
            var displayName = player.displayName;
            if (whitelist.Contains(displayName))
            {
                whitelist.Remove(displayName);
                WhitelistUpdated();
            }
        }
        #endregion

        #region Get All Mode
        public void _FillWithAllProductOwners()
        {
            whitelist.Clear();

            foreach (var product in products)
            {
                if (product == null) continue;
                Store.ListProductOwners((IUdonEventReceiver)this, product);
            }
            WhitelistUpdated();
            
            SendCustomEventDelayedSeconds(nameof(_FillWithAllProductOwners), allProductOwnersRefreshRate * 60);
            if (debugLogging)
                MUIDebug.Log($"{gameObject.name} CE Whitelist | Finished grabbing all Supporters. Next update in {allProductOwnersRefreshRate} minutes.", gameObject);
        }

        public override void OnListProductOwners(IProduct product, string[] owners)
        {
            foreach (var owner in owners)
            {
                if (whitelist.Contains(owner)) continue;
                whitelist.Add(owner);
            }
        }
        #endregion

        
        #region Get Added By Overrides
        public override DataList _GetUsersAddedBy(string username)
        {
            if (username == localName) 
                return whitelist.DeepClone();
            return new DataList();
        }

        public override int _GetUsersAddedByCount(string username)
        {
            if (username == localName) 
                return whitelist.Count;
            return 0;
        }

        public override string _GetWhoAdded(string username)
        {
            if (whitelist.Contains(username))
                return localName;
            return null;
        }
        #endregion
    }
    

}
