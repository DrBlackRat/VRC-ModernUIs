using DrBlackRat.VRC.ModernUIs.Notifications;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class WhitelistRequestor : WhitelistEditorBase
    {
        [Tooltip("Button for Requesting Access. Will have it's interactable set.")]
        [SerializeField] protected Button requestButton;

        [Tooltip("GameObject that is enabled on the Request Button if you are Whitelisted.")] 
        [SerializeField] protected GameObject whitelistedText;
        [Tooltip("GameObject that is enabled on the Request Button if you are an Admin and \"Allow Admins To Request\" is disabled.")]
        [SerializeField] protected GameObject adminText;
        [Tooltip("GameObject that is enabled on the Request Button if you aren't whitelisted and can request.")]
        [SerializeField] protected GameObject requestText;
        [Tooltip("GameObject that is enabled on the Request Button if you aren't whitelisted and are currently requesting")]
        [SerializeField] protected GameObject removeRequestText;
        
        [Tooltip("Notification Manager that will be used to display a \"User Is Requesting\" notification to admins. Leave empty if you don't want to show one.")]
        [SerializeField] protected NotificationManager notificationManager;
        [Tooltip("Header of the notification that will be shown to admins.")]
        [SerializeField] protected string notificationHeader = "User Is Requesting Access";
        [TextArea(1, 3)] [Tooltip("Text of the notification that will be shown to admins, {user} will be replaced with the username.")]
        [SerializeField] protected string notificationText = "<b>{user}</b> is requesting access to the whitelist!";
        [Tooltip("Image of the notification that will be shown to admins, if not provided the image will be disabled.")]
        [SerializeField] private Sprite notificationImage;
        [Tooltip("If a notification sound should be played.")]
        [SerializeField] protected bool playNotificationSound = true;
        [Tooltip("Audio Clip used for the notification, if not provided the default will be used.")]
        [SerializeField] protected AudioClip notificationSoundClip;
        [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        [SerializeField] protected float notificationVolume = 0.25f;
        [Tooltip("Duration of the notification that will be shown to admins.")]
        [SerializeField] protected float notificationDuration = 5f;
        [Tooltip("Width of the notification that will be shown to admins.")]
        [SerializeField] protected float notificationWidth = 80;

        [UdonSynced]
        protected string serializedRequestingUsers;

        [HideInInspector]
        public int autoAccept = 0; // Set by Selector UI, 0 = Off, 1 = On

        #region Unity, VRChat & ModernUIs Events
        protected override void Start()
        {
            base.Start();
            UpdateRequestButton();
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            var displayName = player.displayName;
            if (Networking.LocalPlayer.isMaster)
            {
                RemoveOtherUser(displayName, true); 
                UpdateCountDisplays();
            }
        }
        
        public override void _WhitelistUpdated()
        {
            base._WhitelistUpdated();
            UpdateRequestButton();
        }

        public override void _AdminWhitelistUpdated()
        {
            base._AdminWhitelistUpdated();
            UpdateRequestButton();
        }
        
        public void _CheckAutoAdd()
        {
            if (autoAccept == 0 || !hasAccess || !canAdd)
                return;

            var usersToAdd = otherUsers.GetKeys();
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                if (!canAdd) break;
                _Add(usersToAdd[i].String);
            }
        }
        
        #endregion

        #region Networking
        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(serializedRequestingUsers, out DataToken result))
            {
                // Add if not on whitelistedUsers
                var requestors = result.DataList;
                for (var i = 0; i < requestors.Count; i++)
                {
                    if (otherUsers.ContainsKey(requestors[i])) continue;
                    AddOtherUser(requestors[i].String, true);
                }
            
                // Remove if on whitelistedUsers but not on whitelist
                var keys = otherUsers.GetKeys();
                for (var i = 0; i < keys.Count; i++)
                {
                    if (requestors.Contains(keys[i])) continue;
                    RemoveOtherUser(keys[i].String, true);
                }
            }
            UpdateRequestButton();
            UpdateCountDisplays();
            
            SendCustomEventDelayedFrames(nameof(_CheckAutoAdd), 1);
        }

        public override void OnPreSerialization()
        {
            if (VRCJson.TrySerializeToJson(otherUsers.GetKeys(), JsonExportType.Minify, out DataToken result))
            {
                serializedRequestingUsers = result.String;
            }
        }
        #endregion
        
        #region Accces Checks & Related UI Updates
        protected void UpdateRequestButton()
        {
            bool isWhitelisted = whitelistedUsers.ContainsKey(localName);
            bool isRequesting = otherUsers.ContainsKey(localName);
            bool isBlockingAdmin = hasAdminWhitelist && !showOrAllowAdmins && adminWhitelist._IsPlayerWhitelisted(localName);
            
            requestButton.interactable = !(isWhitelisted || isBlockingAdmin);
            
            whitelistedText.SetActive(isWhitelisted);
            removeRequestText.SetActive(isRequesting);
            adminText.SetActive(isBlockingAdmin && !isWhitelisted);
            requestText.SetActive(!isWhitelisted && !isRequesting && !isBlockingAdmin);
        }
        #endregion
        
        #region Adding & Removing Users
        protected override bool AddOtherUser(string username, bool sendNet)
        {
            if (!base.AddOtherUser(username, sendNet)) 
                return false;
            
            if (sendNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
            
            SendRequestNotification(username);

            return true;
        }
        
        protected override bool RemoveOtherUser(string username, bool sendNet)
        {
            if (!base.RemoveOtherUser(username, sendNet)) 
                return false;
            
            if (sendNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }

            return true;
        }
        #endregion

        #region Button API
        public override void _Add(string username)
        {
            base._Add(username);
            UpdateCountDisplays();
            if (showOrAllowAdmins)
                UpdateRequestButton();
        }
        
        public override void _Remove(string username)
        {
            base._Remove(username);
            UpdateRequestButton();
            
            SendCustomEventDelayedFrames(nameof(_CheckAutoAdd), 1);
        }

        public void _Decline(string username)
        {
            if (!hasAccess) return;
            
            RemoveOtherUser(username, true);
            
            UpdateRequestButton();
            UpdateCountDisplays();
        }

        public void _RequestAccess()
        {
            if (!otherUsers.ContainsKey(localName))
            {
                AddOtherUser(localName, true);
            }
            else
            {
                RemoveOtherUser(localName, true);
            }
            UpdateRequestButton();
            UpdateCountDisplays(); 
        }
        #endregion

        private void SendRequestNotification(string username)
        {
            if (notificationManager == null || !hasAccess)
                return;

            notificationManager._AddNotification(notificationHeader, notificationText.Replace("{user}", username),
                notificationImage, playNotificationSound, notificationSoundClip, notificationVolume,
                notificationDuration, notificationWidth);
        }

    }
}
