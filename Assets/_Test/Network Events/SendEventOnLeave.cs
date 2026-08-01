using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class SendEventOnLeave : UdonSharpBehaviour
{
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Debug.LogWarning($"{player.displayName} left the room! Is Valid: {player.IsValid()}.");
            
        if (player.isLocal)    
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReceiveNetworkEvent));
    }
    
    [NetworkCallable]
    public void ReceiveNetworkEvent()
    {
        var sender = NetworkCalling.CallingPlayer;
        Debug.LogError($"Network Event Received!\nSender: {sender.displayName}, Sender Valid: {sender.IsValid()}.");
    }
}
