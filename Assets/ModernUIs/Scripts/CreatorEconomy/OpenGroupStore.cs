using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Utils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OpenGroupStore : UdonSharpBehaviour
    {
        [SerializeField] private string groupId = "CHANGE THIS";
        
        public void _OpenStore()
        {
            Store.OpenGroupStorePage(groupId);
        }
    }
}