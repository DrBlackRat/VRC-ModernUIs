
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistTeleport : UdonSharpBehaviour
    {
        [Header("Settings:")] 
        [SerializeField] private Transform teleportTransform;
        [SerializeField] private WhitelistManager whitelistManager;
        
        public void _Teleport()
        {
            var localPlayer = Networking.LocalPlayer;
            if (!whitelistManager._IsPlayerWhitelisted(localPlayer)) return;
            localPlayer.TeleportTo(teleportTransform.position, teleportTransform.rotation);
        }
    }
}
