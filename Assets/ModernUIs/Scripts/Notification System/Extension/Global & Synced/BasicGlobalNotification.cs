
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicGlobalNotification : UdonSharpBehaviour
    {
        [Tooltip("Global Notification Sender used to actually send the notification to everyone.")]
        [SerializeField] private GlobalNotificationSender globalNotificationSender;
        
        [Tooltip("Header / Title of the notification.")]
        public string header = "Notification";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the notification.")]
        public string message = "";
        
        [Tooltip("If a notification sound should be played.")]
        public bool playSound = true;
        
        [Range(0f, 10f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 3f;
        [Range(50f, 150f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 80f;
        
        public void _SendNotification()
        {
            globalNotificationSender._SendNetworkNotification(null, header, message, playSound, duration, width);
        }
    }
}
