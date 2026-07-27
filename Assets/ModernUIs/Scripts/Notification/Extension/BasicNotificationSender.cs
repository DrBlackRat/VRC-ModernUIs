
using System;
using UdonSharp;
using UnityEngine;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicNotificationSender : UdonSharpBehaviour
    {
        [Tooltip("Notification Manager / System used to actually display the notification.")]
        [SerializeField] private NotificationManager notificationManager;
        [Tooltip("If enabled the notification will be shown in the notification history screen.")]
        [SerializeField] private bool showInHistory = true;
        [Tooltip("Name of the source of the notification. If left empty, none will be displayed.")]
        [SerializeField] private string sourceName = "";
        
        [Tooltip("If provided the notification will only be send to members of this whitelist.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("If enabled Join / Leave notifications will be send to everyone, BUT members on the provided whitelist.")]
        [SerializeField] private bool invertWhitelist;

        [Tooltip("Header / Title of the notification.")]
        public string header = "Notification";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the notification.")]
        public string message = "";
        [Tooltip("Image / Sprite used by the notification, if not provided the image will be disabled.")]
        public Sprite image;
        
        [Tooltip("If a notification sound should be played.")]
        public bool playSound = true;
        [Tooltip("Audio Clip used for the notification, if not provided the default will be used.")]
        public AudioClip soundClip;
        [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolume = 0.25f; 
        
        [Range(0f, 60f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 3f;
        [Range(100f, 200f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 100f;

        private bool isWhitelisted;
        public VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            if (whitelist != null)
            {
                whitelist._SetUpConnection((IUdonEventReceiver)this);
            }
            else
            {
                isWhitelisted = true;
            }
        }

        public void _WhitelistUpdated()
        {
            var newIsWhitelisted = whitelist._IsPlayerWhitelisted(localPlayer);
            isWhitelisted = invertWhitelist ? !newIsWhitelisted : newIsWhitelisted ; 
        }

        /// <summary>
        /// Sends the notification as configured.
        /// </summary>
        public void _SendNotification()
        {
            if (!isWhitelisted) return;
            notificationManager._AddNotification(header, message, image, playSound, soundClip, soundVolume, duration, width, showInHistory, sourceName);
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            if (notificationManager != null)
                return;
            
            notificationManager = FindObjectOfType<NotificationManager>();
        }
#endif
    }
}
