
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class JoinLeaveNotificationSender : UdonSharpBehaviour
    {
        [Tooltip("Notification Manager / System used to actually display the notification.")]
        [SerializeField] private NotificationManager notificationManager;
        [Tooltip("If enabled the notification will be shown in the notification history screen.")]
        [SerializeField] private bool showInHistory = false;
        [Tooltip("Name of the source of the notification. If left empty, none will be displayed.")]
        [SerializeField] private string sourceName = "";
        
        [Tooltip("If provided only people on the whitelist will have send a notification from them.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("If enabled Join / Leave Notifications will be shown from everyone, BUT members on the provided whitelist.\nShould be enabled if Whitelist Members should have their own notification.")]
        [SerializeField] private bool invertWhitelist;
        
        [Tooltip("If provided the Join / Leave notifications will only be send to members of this whitelist.")]
        [SerializeField] private WhitelistGetterBase sendToWhitelist;
        [Tooltip("If enabled Join / Leave notifications will be send to everyone, BUT members on the provided whitelist.")]
        [SerializeField] private bool invertSendToWhitelist;

        [Tooltip("If join notifications should be shown.")]
        [SerializeField] private bool showJoinNotification = true;
        [Tooltip("Header / Title of the join notification.")]
        public string headerJoined = "Player Joined";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the join notification. The \"{user}\" will be replaced with the user name.")]
        public string messageJoined = "<b>{user}</b> has joined!";
        [Tooltip("Image / Sprite used by the join notification, if not provided the image will be disabled.")]
        public Sprite imageJoined;
        [Tooltip("If a join notification sound should be played.")]
        public bool playSoundJoined = false;
        [Tooltip("Audio Clip used for the join notification, if not provided the default will be used.")]
        public AudioClip soundClipJoined;
        [Tooltip("Volume of the join notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolumeJoined = 0.25f;
        
        [Tooltip("If leave notifications should be shown.")]
        [SerializeField] private bool showLeaveNotification = true;
        [Tooltip("Header / Title of the leave notification.")]
        public string headerLeft = "Player Left";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the leave notification. The \"{user}\" will be replaced with the user name.")]
        public string messageLeft = "<b>{user}</b> has left!";
        [Tooltip("Image / Sprite used by the leave notification, if not provided the image will be disabled.")]
        public Sprite imageLeft;
        [Tooltip("If a leave notification sound should be played.")]
        public bool playSoundLeft = false;
        [Tooltip("Audio Clip used for the leave notification, if not provided the default will be used.")]
        public AudioClip soundClipLeft;
        [Tooltip("Volume of the leave notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolumeLeft = 0.25f; 
        
        [Range(0f, 10f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 1.5f;
        [Range(100f, 200f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 100f;

        private bool hasWhitelist;
        private bool canSeeNotification;
        private VRCPlayerApi localPlayer;

        // Cooldown system to prevent spamming notifications on join;
        private float startCooldown = 5f; // in seconds
        private bool cooldownFinished;
        private bool cooldownStarted;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            hasWhitelist = whitelist != null;

            if (sendToWhitelist != null)
            {
                sendToWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(ShowToWhitelistUpdated));
            }
            else
            {
                canSeeNotification = true;
            }
        }

        public void _FinishCooldown()
        {
            cooldownFinished = true;
        }

        public void ShowToWhitelistUpdated()
        {
            var isWhitelisted = sendToWhitelist._IsPlayerWhitelisted(localPlayer);
            canSeeNotification = invertSendToWhitelist ? !isWhitelisted : isWhitelisted ;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!showJoinNotification) return;
            if (!cooldownFinished)
            {
                if (cooldownStarted) return;
                cooldownStarted = true;
                SendCustomEventDelayedSeconds(nameof(_FinishCooldown), startCooldown);
                return;
            }
            
            HandleNotification(headerJoined, messageJoined, imageJoined, playSoundJoined, soundClipJoined, soundVolumeJoined, player);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!showLeaveNotification || !cooldownFinished) return;
            HandleNotification(headerLeft, messageLeft, imageLeft, playSoundLeft, soundClipLeft, soundVolumeLeft, player);
        }

        private void HandleNotification(string header, string message, Sprite image, bool playSound, AudioClip soundClip, float soundVolume,  VRCPlayerApi player)
        {
            if (!ShowPlayerCheck(player) || !canSeeNotification) return;
            
            var newMessage = message.Replace("{user}", player.displayName);
            notificationManager._AddNotification(header, newMessage, image, playSound, soundClip, soundVolume, duration, width, showInHistory, sourceName);
        }
        
        private bool ShowPlayerCheck(VRCPlayerApi player)
        {
            if (!hasWhitelist)
                return true;

            var isWhitelisted = whitelist._IsPlayerWhitelisted(player);
            return invertWhitelist ? !isWhitelisted : isWhitelisted;
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
