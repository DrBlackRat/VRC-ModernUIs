
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using DrBlackRat.VRC.ModernUIs.Notifications;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NetNotificationRelay : UdonSharpBehaviour
    {
        [Tooltip("Notification Manager / System used to actually display the notification.")]
        [SerializeField] private NotificationManager notificationManager;
        [Tooltip("If provided only people on the whitelist will be able to send notifications.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("If enabled the notification will be shown in the notification history screen.")]
        [SerializeField] private bool showInHistory = true;
        [Tooltip("If enabled the player who send the notification will be shown as the  source of the notification.")]
        [SerializeField] private bool showSendingPlayerAsSource = true;
        
        [Tooltip("Images available for networked notifications, when sending provide the corresponding index that should be used.")]
        public Sprite[] images;
        [Tooltip("Audio Clips available for networked notifications, when sending provide the corresponding index that should be used.")]
        public AudioClip[] soundClips;
        
        [Tooltip("If enabled, the notification received (including who send it) will be logged. Otherwise only failures will be logged.")]
        [SerializeField] private bool logNotification = false;

        private VRCPlayerApi localPlayer;
        private string localName;
        
        private bool hasWhitelist;
        private bool isWhitelisted;
        
        // Notification Dict Keys:
        private const string IsGlobalKey = "isGlobal"; // bool | if it should be sent to everyone
        private const string PlayersKey = "players"; // Data List | containing players who should receive the notification (only used if is global is disabled)
        private const string HeaderKey = "header"; // string | Title of the notification
        private const string TextKey = "text"; // string | Message of the notification
        private const string ImageIdKey = "imageId"; // int | id of the image to use
        private const string PlaySoundKey = "playSound"; // bool | if sound should be played
        private const string SoundClipIdKey = "soundClipId"; // int | id of the sound clip to use
        private const string SoundVolumeKey = "soundVolume"; // float | volume of the notification sound
        private const string DurationKey = "duration"; // float | how long the notification stays on screen
        private const string WidthKey = "width"; // float | how wide the notification should be

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            localName = localPlayer.displayName;
            
            hasWhitelist = whitelist != null;
            if (hasWhitelist)
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
            isWhitelisted = whitelist._IsPlayerWhitelisted(localPlayer);
        }

        /// <summary>
        /// Sends a global notification to all players or to the specified targets.
        /// If a whitelist is set in the inspector, the sender must be on it to send the notification.
        /// </summary>
        /// <param name="targetPlayerNames">
        /// A <see cref="DataList"/> of player display names that should receive the notification.
        /// If null, the notification is sent to everyone.
        /// </param>
        /// <param name="header">The title of the notification.</param>
        /// <param name="text">The content of the notification.</param>
        /// <param name="imageId">
        /// Index of the image to use from the Images array configured in the Inspector.
        /// Set to -1 to omit the image.
        /// </param>
        /// <param name="playSound">Whether a notification sound should be played.</param>
        /// <param name="soundClipId">
        /// Index of the sound clip to use from the Sound Clips array configured in the Inspector.
        /// Set to -1 to use the default sound.
        /// </param>
        /// <param name="soundVolume">Volume of the notification sound. Default is 0.25, clamped between 0 and 1.</param>
        /// <param name="duration">How long the notification should be shown, in seconds. Default is 3, clamped between 0 and 60.</param>
        /// <param name="width">How wide the notification panel should be. Default is 80, clamped between 50 and 150.</param>
        /// <returns></returns>
        public bool _SendNetNotification(DataList targetPlayerNames ,string header, string text, int imageId = -1, 
            bool playSound = true, int soundClipId = -1, float soundVolume = 0.25f, 
            float duration = 3f, float width = 80f)
        {
            if (!isWhitelisted)
            {
                MUIDebug.LogWarning("Net Notification Relay | You are not allowed to call \"_SendNetworkNotification\"!");
                return false;
            }

            DataDictionary notificationDict = new DataDictionary();
            notificationDict.SetValue(IsGlobalKey, targetPlayerNames == null);
            
            if (targetPlayerNames != null)
                notificationDict.SetValue(PlayersKey, targetPlayerNames);
            
            notificationDict.SetValue(HeaderKey, header);
            notificationDict.SetValue(TextKey, text);
            notificationDict.SetValue(ImageIdKey, imageId);
            notificationDict.SetValue(PlaySoundKey, playSound);
            notificationDict.SetValue(SoundClipIdKey, soundClipId);
            notificationDict.SetValue(SoundVolumeKey, soundVolume);
            notificationDict.SetValue(DurationKey, duration);
            notificationDict.SetValue(WidthKey, width);

            if (!VRCJson.TrySerializeToJson(notificationDict, JsonExportType.Minify, out DataToken notificationData))
            {
                MUIDebug.LogError("Net Notification Relay | Could not Serialize Notification Data!");
                return false;
            }
            
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReceiveNetNotification), notificationData.String);
            return true;
        }

        [NetworkCallable(2)]
        public void ReceiveNetNotification(string notificationData)
        {
            var callingPlayerName = NetworkCalling.CallingPlayer.displayName;
            if (!NetworkCalling.InNetworkCall)
            {
                MUIDebug.LogWarning("Net Notification Relay | You are not allowed to call \"ReceiveNetNotification\" outside of a network call!");
                return;
            }
            if (hasWhitelist && !whitelist._IsPlayerWhitelisted(callingPlayerName))
            {
                MUIDebug.LogWarning($"Net Notification Relay | {callingPlayerName} tried to send a notification but isn't allowed to!");
                return;
            }

            if (!VRCJson.TryDeserializeFromJson(notificationData, out DataToken notificationToken))
            {
                MUIDebug.LogError("Net Notification Relay | Could not Deserialize Notification Data!");
                return;
            }
            
            var notificationDict = notificationToken.DataDictionary;
            
            bool isReceiver = true;
            string header = "Lorem ipsum";
            string text = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr.";
            Sprite image = null;
            bool playSound = true;
            AudioClip soundClip = null;
            float soundVolume = 0.25f;
            float duration = 3f;
            float width = 80f;
            
            if (notificationDict.TryGetValue(IsGlobalKey, TokenType.Boolean, out var isGlobalToken) 
                && !isGlobalToken.Boolean
                && notificationDict.TryGetValue(PlayersKey, TokenType.DataList, out var playersToken)
                )
            {
                isReceiver = false;
                var players = playersToken.DataList;
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].String == localPlayer.displayName)
                    {
                        isReceiver = true;
                        break;
                    }
                }
            }
            
            if (!isReceiver)
                return;
            
            if (notificationDict.TryGetValue(ImageIdKey, TokenType.Double, out var imageIdToken))
            {
                var imageId = (int)imageIdToken.Double;
                if (imageId >= 0 && imageId < images.Length)
                {
                    image = images[imageId];
                } 
            }
            
            if (notificationDict.TryGetValue(SoundClipIdKey, TokenType.Double, out var soundClipIdToken))
            {
                var soundClipId = (int)soundClipIdToken.Double;
                if (soundClipId >= 0 && soundClipId < soundClips.Length)
                {
                    soundClip = soundClips[soundClipId];
                } 
            }
            
            if (notificationDict.TryGetValue(HeaderKey, TokenType.String, out var headerToken))
                header = headerToken.String;
            if (notificationDict.TryGetValue(TextKey, TokenType.String, out var textToken))
                text = textToken.String;
            if (notificationDict.TryGetValue(PlaySoundKey, TokenType.Boolean, out var playSoundToken))
                playSound = playSoundToken.Boolean;
            if (notificationDict.TryGetValue(SoundVolumeKey, TokenType.Double, out var soundVolumeToken))
                soundVolume = (float)soundVolumeToken.Double;
            if (notificationDict.TryGetValue(DurationKey, TokenType.Double, out var durationToken))
                duration = (float)durationToken.Double;
            if (notificationDict.TryGetValue(WidthKey, TokenType.Double, out var widthToken))
                width = (float)widthToken.Double;
            
            notificationManager._AddNotification(header, text, image, playSound, soundClip, soundVolume, duration,
                width, showInHistory, showSendingPlayerAsSource ? callingPlayerName : "");
            
            if (logNotification)
                MUIDebug.Log($"Net Notification Relay | Received Notification from {callingPlayerName}: {header} - {text}");
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
