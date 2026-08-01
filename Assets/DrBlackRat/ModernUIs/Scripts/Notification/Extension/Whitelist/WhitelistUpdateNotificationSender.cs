
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistUpdateNotificationSender : UdonSharpBehaviour
    {
        //General
        [SerializeField] 
        [Tooltip("Notification Manager / System used to actually display the notification.")] 
        private NotificationManager notificationManager;
        [SerializeField] 
        [Tooltip("If enabled the notification will be shown in the notification history screen.")]
        private bool showInHistory = true;
        [SerializeField] 
        [Tooltip("Name of the source of the notification. If left empty, none will be displayed.")]
        private string sourceName = "";

        // Whitelist
        [SerializeField] 
        [Tooltip("Whitelist to send add / remove notifications for.")] 
        private WhitelistGetterBase whitelist;
        [SerializeField]
        [Range(0f, 60f)]
        [Tooltip("Time in seconds before the first change notifications will be send. Used to prevent receiving huge amount of notifications when first joining.")]
        private float startDelayTime = 15f;
        
        // Show To Whitelist
        [SerializeField] 
        [Tooltip("If provided the notifications will only be shown to members of this whitelist.")]
        private WhitelistGetterBase showToWhitelist;
        [SerializeField] 
        [Tooltip("If enabled notifications will be send to everyone, BUT members on the provided whitelist.")]
        private bool invertShowToWhitelist;
        
        // Show Notification
        [SerializeField] 
        [Tooltip("If whitelist add should be shown.")]
        private bool showAddNotification = true;
        [Tooltip("Header / Title of the add notification.")]
        public string headerAdded = "Whitelist User Added";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the add notification. \nThe \"{user}\" will be replaced with the user name and \"{adder}\" with who added them.")]
        public string messageAdded = "<b>{user}</b> was added to the whitelist by <b>{adder}</b>!";
        [Tooltip("Image / Sprite used by the add notification, if not provided the image will be disabled.")]
        public Sprite imageAdded;
        [Tooltip("If a add notification sound should be played.")]
        public bool playSoundAdded = false;
        [Tooltip("Audio Clip used for the add notification, if not provided the default will be used.")]
        public AudioClip soundClipAdded;
        [Tooltip("Volume of the add notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolumeAdded = 0.25f;
        
        // Remove Notification
        [SerializeField] 
        [Tooltip("If whitelist remove should be shown.")]
        private bool showRemoveNotification = true;
        [Tooltip("Header / Title of the remove notification.")]
        public string headerRemoved = "Whitelist User Removed";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the remove notification. \nThe \"{user}\" will be replaced with the user name.")]
        public string messageRemoved = "<b>{user}</b> was removed from the whitelist!";
        [Tooltip("Image / Sprite used by the remove notification, if not provided the image will be disabled.")]
        public Sprite imageRemoved;
        [Tooltip("If a remove notification sound should be played.")]
        public bool playSoundRemoved = false;
        [Tooltip("Audio Clip used for the remove notification, if not provided the default will be used.")]
        public AudioClip soundClipRemoved;
        [Tooltip("Volume of the remove notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolumeRemoved = 0.25f;
        
        [Range(0f, 10f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 5f;
        [Range(100f, 200f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 120f;
        
        private bool canSeeNotification;
        private VRCPlayerApi localPlayer;

        private DataList previousWhitelist = new DataList();
        private DataList currentWhitelist = new DataList();
        
        private bool startDelayFinished;

        #region Unity, VRChat & Modern UI Events
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            
            whitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_WhitelistUpdated));
            if (showToWhitelist != null)
            {
                showToWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_ShowToWhitelistUpdated));
            }
            else
            {
                canSeeNotification = true;
            }
            
            SendCustomEventDelayedSeconds(nameof(_FinishStartDelay), startDelayTime);
        }
        
        public void _FinishStartDelay()
        {
            startDelayFinished = true;
        }
        
        public void _ShowToWhitelistUpdated()
        {
            var pevCanSeeNotification = canSeeNotification;
            
            var isWhitelisted = showToWhitelist._IsPlayerWhitelisted(localPlayer);
            canSeeNotification = invertShowToWhitelist ? !isWhitelisted : isWhitelisted ;

            if (!pevCanSeeNotification && canSeeNotification)
                ResetHistory();
        }

        public void _WhitelistUpdated()
        {
            if (!canSeeNotification)
                return;
            
            previousWhitelist = currentWhitelist;
            currentWhitelist = whitelist._GetUsersAsList();
            
            CheckForChanges();
        }
        #endregion

        #region Checking Changes, Show To, etc,
        private void CheckForChanges()
        {
            if (!startDelayFinished) 
                return;

            if (showRemoveNotification)
            {
                for (var i = 0; i < previousWhitelist.Count; i++)
                {
                    if (currentWhitelist.Contains(previousWhitelist[i])) continue;
                    ShowRemoveNotification(previousWhitelist[i].String);
                }
            }

            if (showAddNotification)
            {
                for (var j = 0; j < currentWhitelist.Count; j++)
                {
                    if (previousWhitelist.Contains(currentWhitelist[j])) continue;
                    ShowAddNotification(currentWhitelist[j].String);
                }
            }
        }

        private void ResetHistory()
        {
            var currentList =  whitelist._GetUsersAsList();
            previousWhitelist = currentList;
            currentWhitelist = currentList;
        }
        #endregion


        #region Showing Notifications
        private void ShowAddNotification(string username)
        {
            var newMessage = messageAdded.Replace("{user}", username);
            newMessage = newMessage.Replace("{adder}", whitelist._GetWhoAdded(username));
            
            notificationManager._AddNotification(headerAdded, newMessage, imageAdded, playSoundAdded, soundClipAdded, soundVolumeAdded, duration, width, showInHistory, sourceName);
        }
        
        private void ShowRemoveNotification(string username)
        {
            var newMessage = messageRemoved.Replace("{user}", username);
            notificationManager._AddNotification(headerRemoved, newMessage, imageRemoved, playSoundRemoved, soundClipRemoved, soundVolumeRemoved, duration, width, showInHistory, sourceName);
        }
        #endregion
        
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
