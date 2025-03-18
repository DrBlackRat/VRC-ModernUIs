using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.Hideout
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OpenGroupStore : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [SerializeField] private string groupId;
        
        public void _OpenStore()
        {
            Store.OpenGroupStorePage(groupId);
        }
    }
}