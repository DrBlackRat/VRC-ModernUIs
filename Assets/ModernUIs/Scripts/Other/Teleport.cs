
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Teleport : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [SerializeField] private Transform teleportTransform;
        
        public void _Teleport()
        {
            Networking.LocalPlayer.TeleportTo(teleportTransform.position, teleportTransform.rotation);
        }
    }
}

