
using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OpenGroup : UdonSharpBehaviour
    {
        [SerializeField] private string groupId = "CHANGE THIS";
        
        public void _OpenGroup()
        {
            Store.OpenGroupPage(groupId);
        }
    }
}
