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

        public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
        {
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
            var displayName = player.displayName;
            if (whitelist.Contains(displayName))
            {
                whitelist.Remove(displayName);
                WhitelistUpdated();
            }
        }
        
        #region Get Added By Overrides
        public override DataList _GetUsersAddedBy(string username)
        {
            return username == localName ? whitelist.DeepClone() : new DataList();
        }

        public override int _GetUsersAddedByCount(string username)
        {
            return username == localName ? whitelist.Count : 0;
        }

        public override string _GetWhoAdded(string username)
        {
            return whitelist.Contains(username) ? localName : null;
        }
        #endregion
    }
    

}
