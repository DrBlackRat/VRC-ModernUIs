using System.Data;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GroupOpener : UdonSharpBehaviour
    {
        [Tooltip("ID of the VRChat Group to open.")]
        public string groupID;
        
        public override void Interact()
        {
            Store.OpenGroupPage(groupID);
        }
    }
}

