
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicNetNotificationSender : UdonSharpBehaviour
    {
        [Tooltip("Net Notification Relay used to actually send the notification to everyone.")]
        [SerializeField] private NetNotificationRelay netNotificationRelay;
        [Tooltip("If provided only players who are on this whitelist will receive the notification. If left empty it will be send to everyone.")]
        [SerializeField] private WhitelistGetterBase receivingWhitelist;
        
        [Tooltip("Header / Title of the notification.")]
        public string header = "Networked Notification";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the notification.")]
        public string message = "";
        [Tooltip("Index of the image to use from the Images array configured on the Net Notification Relay.\nSet to -1 to omit the image.")]
        public int imageId = -1;
        
        [Tooltip("If a notification sound should be played.")]
        public bool playSound = true;
        [Tooltip("Index of the sound clip to use from the Sound Clips array configured on the Net Notification Relay.\nSet to -1 to use the default.")]
        public int soundClipId = -1;
        [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolume = 0.25f;
        
        [Range(0f, 60f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 3f;
        [Range(100f, 200f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 100f;
        
        public void _SendNotification()
        {
            DataList tragetNames = null;
            if (receivingWhitelist != null)
                tragetNames = receivingWhitelist._GetUsersAsList();

            netNotificationRelay._SendNetNotification(tragetNames, header, message, imageId, playSound, soundClipId,
                soundVolume, duration, width);
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            if (netNotificationRelay != null)
                return;
            
            netNotificationRelay = FindObjectOfType<NetNotificationRelay>();
        }
#endif
    }
}
