
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
        [Tooltip("Notification Manager / System used to actually display the notification.")] 
        [SerializeField] private NotificationManager notificationManager;
        [Tooltip("If enabled the notification will be shown in the notification history screen.")]
        [SerializeField] private bool showInHistory = true;
        [Tooltip("Name of the source of the notification. If left empty, none will be displayed.")]
        [SerializeField] private string sourceName = "";

        [Tooltip("Whitelist to send add / remove notifications for.")] 
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("If provided the notifications will only be shown to members of this whitelist.")]
        [SerializeField] private WhitelistGetterBase showToWhitelist;
        [Tooltip("If enabled notifications will be send to everyone, BUT members on the provided whitelist.")]
        [SerializeField] private bool invertShowToWhitelist;
        
        [Tooltip("If whitelist add should be shown.")]
        [SerializeField] private bool showAddNotification = true;
        [Tooltip("If enabled, the user who added the user to the whitelist will be shown in the notification, this is done by replacing \"{adder}\" in the addes message")]
        [SerializeField] private bool showAdder = true;
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
        
        [Tooltip("If whitelist remove should be shown.")]
        [SerializeField] private bool showRemoveNotification = true;
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
        [Range(50f, 150f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 100f;
        
        private bool canSeeNotification;
        private VRCPlayerApi localPlayer;

        private DataList previousWhitelist = new DataList();
        private DataList currentWhitelist = new DataList();
        
        // Cooldown system to prevent spamming notifications on join;
        private float startCooldown = 10f; // in seconds
        private bool cooldownFinished;
        
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            SendCustomEventDelayedSeconds(nameof(_FinishCooldown), startCooldown);

            whitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_WhitelistUpdated));
            if (showToWhitelist != null)
            {
                showToWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_ShowToWhitelistUpdated));
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
        
        public void _ShowToWhitelistUpdated()
        {
            var isWhitelisted = showToWhitelist._IsPlayerWhitelisted(localPlayer);
            canSeeNotification = invertShowToWhitelist ? !isWhitelisted : isWhitelisted ;
        }

        public void _WhitelistUpdated()
        {
            previousWhitelist = currentWhitelist;
            currentWhitelist = whitelist._GetUsersAsList();
            
            CheckForChanges();
        }

        private void CheckForChanges()
        {
            if (!cooldownFinished || !canSeeNotification) 
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

        private void ShowAddNotification(string username)
        {
            var newMessage = messageAdded.Replace("{user}", username);
            
            if (showAdder)
                newMessage = newMessage.Replace("{adder}", whitelist._GetWhoAdded(username));
            
            notificationManager._AddNotification(headerAdded, newMessage, imageAdded, playSoundAdded, soundClipAdded, soundVolumeAdded, duration, width, showInHistory, sourceName);
        }
        
        private void ShowRemoveNotification(string username)
        {
            var newMessage = messageRemoved.Replace("{user}", username);
            notificationManager._AddNotification(headerRemoved, newMessage, imageRemoved, playSoundRemoved, soundClipRemoved, soundVolumeRemoved, duration, width, showInHistory, sourceName);
        }
        
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            notificationManager = FindObjectOfType<NotificationManager>();
        }
#endif
    }
}
