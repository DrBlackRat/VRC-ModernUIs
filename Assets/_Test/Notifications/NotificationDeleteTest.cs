using DrBlackRat.VRC.ModernUIs.Notifications;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Testing
{
    public class NotificationDeleteTest : UdonSharpBehaviour
    {
        [Header("Setup:")]
        public NotificationManager notificationManager;
        public bool deleteWhileShowing = true;

        [Header("Message:")] 
        public string header = "Test Notification";
        public string message = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr.";
        public float duration = 10f;
        
        private int notificationId;
        
        public void SendNotification()
        {
            notificationId = notificationManager._AddNotification(header, message, null, true, null, duration);
            Debug.Log("Send Notification with the id: " + notificationId);
        }

        public void DeleteNotification()
        {
            if (notificationManager._RemoveNotification(notificationId, deleteWhileShowing))
            {
                Debug.Log("Delete Notification with the id: " + notificationId);
            }
            else
            {
                Debug.Log("Failed to Delete Notification with the id: " + notificationId);
            }
            
            
        }
    }
}
