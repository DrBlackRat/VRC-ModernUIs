
using System;
using DrBlackRat.VRC.ModernUIs.Notifications;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SpawnableMenuNotification : UdonSharpBehaviour
    {
        [SerializeField] private SpawnableMenu spawnableMenu;
        [SerializeField] private NotificationManager notificationManager;
        
        [Tooltip("If enabled the notification will be shown in the notification history screen.")]
        [SerializeField] private bool showInHistory = true;
        [Tooltip("Name of the source of the notification. If left empty, none will be displayed.")]
        [SerializeField] private string sourceName = "";
        
        [Tooltip("If enabled a player will only see the notification the first time they join the world.")]
        [SerializeField] private bool onlyShowOnce = false;
        [Tooltip("Key used for storing if they have already seen it.")]
        [SerializeField] private string persistenceKey = "hasSeenMenuNotification";
        
        [Range(0, 10)][Tooltip("How long to wait before showing the notification after joining.")]
        [SerializeField] private float notificationDelay = 2f;
        
        [Tooltip("Header / Title of the notification.")]
        public string header = "Menu Guide";
        [TextArea(1, 3)][Tooltip("Text / Message / Body of the notification. \"{menuInfo}\" will be replaced by the options below.")]
        public string message = "You can open the menu by {menuInfo}!";

        [TextArea(1, 3)][Tooltip("Desktop menu info message. \"{key}\" will be replaced by the selected key.")]
        public string desktopInfo = "pressing the <b>{key}</b> on your keyboard";
        [TextArea(1, 3)][Tooltip("Mobile / Touch Device info message.")]
        public string mobileInfo = "pressing on the <b>menu button</b> in the <b>top left corner of your screen</b>";
        [TextArea(1, 3)][Tooltip("VR down and up info message.")]
        public string vrDownUpInfo = "moving your <b>right thumb stick</b> quickly <b>down and up</b>";
        [TextArea(1, 3)][Tooltip("VR holding up info message.")]
        public string vrHoldUpInfo = "holding your <b>right thumb stick</b> up for <b>{holdTime}s</b>";
        [TextArea(1, 3)][Tooltip("VR holding down info message.")]
        public string vrHoldDownInfo = "holding your <b>right thumb stick</b> down for <b>{holdTime}s</b>";
        
        [Tooltip("Image / Sprite used by the notification, if not provided the image will be disabled.")]
        public Sprite image;
        
        [Tooltip("If a notification sound should be played.")]
        public bool playSound = true;
        [Tooltip("Audio Clip used for the notification, if not provided the default will be used.")]
        public AudioClip soundClip;
        [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        public float soundVolume = 0.25f; 
        
        [Range(0f, 60f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 15f;
        [Range(50f, 150f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 100f;

        private VRCPlayerApi localPlayer;
        private bool hasSeenNotification;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;

            if (!onlyShowOnce)
            {
                SendCustomEventDelayedSeconds(nameof(_ShowNotification), notificationDelay);
            }
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!onlyShowOnce || player != localPlayer) return;
            
            if (!PlayerData.GetBool(localPlayer, persistenceKey))
                SendCustomEventDelayedSeconds(nameof(_ShowNotification), notificationDelay);
        }

        public void _ShowNotification()
        {
            if (!enabled || !gameObject.activeInHierarchy || hasSeenNotification) return;
            if (onlyShowOnce) 
                PlayerData.SetBool(persistenceKey, true);

            notificationManager._AddNotification(header, GenerateMessage(), image, playSound, soundClip, soundVolume,
                duration, width, showInHistory, sourceName);
            hasSeenNotification = true;
        }

        private string GenerateMessage()
        {
            string info = "ERROR";
            if (localPlayer.IsUserInVR())
            {
                switch (spawnableMenu.VROpenMethod)
                {
                    case VRMenuOpenMethod.HoldUp:
                        info = vrHoldUpInfo.Replace("{holdTime}", spawnableMenu.VRHoldTime.ToString());
                        break;
                    case VRMenuOpenMethod.HoldDown:
                        info = vrHoldDownInfo.Replace("{holdTime}", spawnableMenu.VRHoldTime.ToString());
                        break;
                    case VRMenuOpenMethod.DownAndUp:
                        info = vrDownUpInfo;
                        break;
                }
            }
            else if (InputManager.GetLastUsedInputMethod() == VRCInputMethod.Touch)
            {
                info = mobileInfo;
            }
            else
            {
                info = desktopInfo.Replace("{key}", spawnableMenu.OpenKey.ToString());
            }

            return message.Replace("{menuInfo}", info);
        }
    }
}
