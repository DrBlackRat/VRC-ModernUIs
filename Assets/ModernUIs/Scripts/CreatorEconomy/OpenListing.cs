
using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OpenListing : UdonSharpBehaviour
    {
        [SerializeField] private string listingId = "";
        
        public void _OpenListing()
        {
            Store.OpenListing(listingId);
        }
    }
}
