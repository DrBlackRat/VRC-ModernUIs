
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

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GlobalNotificationSender : UdonSharpBehaviour
    {
        [Tooltip("Notification Manager / System used to actually display the notification.")]
        [SerializeField] private NotificationManager notificationManager;
        
        [Tooltip("If provided only people on the whitelist will be able to send notifications.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("Image / Sprite used by the notification, if not provided the image will be disabled.")]
        public Sprite image;
        [Tooltip("Audio Clip used for the notification, if not provided the default will be used.")]
        public AudioClip soundClip;
        [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolume = 0.25f;

        private VRCPlayerApi localPlayer;
        private string localName;
        
        private bool hasWhitelist;
        private bool isWhitelisted;

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
        /// If a whitelist is provided, the sender must be on it to send the notification.
        /// </summary>
        /// <param name="targetNames">
        /// A <see cref="VRCDataList"/> of player display names that should receive the notification.
        /// If null, the notification is sent to everyone.
        /// </param>
        /// <param name="header">The title of the notification.</param>
        /// <param name="text">The content of the notification.</param>
        /// <param name="playSound">
        /// Whether a notification sound should be played (uses the sound clip and volume set up in the inspector).
        /// </param>
        /// <param name="duration">
        /// How long the notification should be shown, in seconds. Default is 3, clamped between 0 and 60.
        /// </param>
        /// <param name="width">
        /// How wide the notification panel should be. Default is 80, clamped between 50 and 150.
        /// </param>
        /// <returns>True if the notification was sent successfully, otherwise false.</returns>
        public bool _SendNetworkNotification(DataList targetNames ,string header, string text, bool playSound = true, float duration = 3f, float width = 80f)
        {
            if (!isWhitelisted)
            {
                MUIDebug.LogWarning("Global Notification Sender | You are not allowed to call \"_SendNetworkNotification\"!");
                return false;
            }
            
            string playersJson = null;
            if (targetNames != null)
            {
                if (!VRCJson.TrySerializeToJson(targetNames, JsonExportType.Minify, out DataToken result))
                    return false;

                playersJson = result.String;
            }
            
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ReceiveGlobalNotification), playersJson, header, text, playSound, duration, width);
            return true;
        }

        [NetworkCallable]
        public void ReceiveGlobalNotification(string playersJson, string header, string text, bool playSound, float duration, float width)
        {
            if (!NetworkCalling.InNetworkCall)
            {
                MUIDebug.LogWarning("Global Notification Sender | You are not allowed to call \"ReceiveGlobalNotification\" outside of a network call!");
                return;
            }

            if (hasWhitelist && !whitelist._IsPlayerWhitelisted(NetworkCalling.CallingPlayer))
            {
                MUIDebug.LogWarning($"Global Notification Sender | {NetworkCalling.CallingPlayer} tried to send you a notification but isn't allowed to!");
                return;
            }

            if (playersJson != null)
            {
                if (!VRCJson.TryDeserializeFromJson(playersJson, out DataToken result) || result.TokenType != TokenType.DataList)
                {
                    MUIDebug.LogError($"Global Notification Sender | Could not Deserialize Player List Json into a Data List!");
                    return;
                }

                var playerList = result.DataList;
                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i].String == localName)
                    {
                        notificationManager._AddNotification(header, text, image, playSound, soundClip, soundVolume, duration,
                            width);
                        return;
                    }
                }
            }
            else
            {
                notificationManager._AddNotification(header, text, image, playSound, soundClip, soundVolume, duration,
                    width);
            }
        }
    }
}
