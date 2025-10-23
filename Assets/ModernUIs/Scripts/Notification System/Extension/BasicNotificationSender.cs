
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicNotificationSender : UdonSharpBehaviour
    {
        [Tooltip("Notification Manager / System used to actually display the notification.")]
        [SerializeField] private NotificationManager notificationManager;

        [Tooltip("Header / Title of the notification.")]
        public string header = "Notification";
        [TextArea(1, 3)] [Tooltip("Text / Message / Body of the notification.")]
        public string message = "";
        [Tooltip("Image / Sprite used by the notification, if not provided the image will be disabled.")]
        public Sprite image;
        [Tooltip("If a notification sounds should be played.")]
        public bool playSound = true;
        [Range(0f, 10f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
        public float duration = 3f;
        [Range(50f, 150f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        public float width = 80f;

        /// <summary>
        /// Sends the notification as configured.
        /// </summary>
        public void _SendNotification()
        {
            notificationManager._AddNotification(header, message, playSound, image, duration, width);
        }
    }
}
