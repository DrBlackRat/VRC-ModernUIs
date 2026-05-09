
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NotificationHistory : UdonSharpBehaviour
    {
        [Range(5f, 100f)][Tooltip("How many notifications should be kept in the history.")]
        [SerializeField] private int maxHistorySize = 25;
        [Tooltip("Text that will be used as the header if no header was provided.")]
        [SerializeField] private string noHeaderText = "Empty Header";
        [Tooltip("Text that will be used as the message if no message was provided.")]
        [SerializeField] private string noMessageText = "No message was provided.";
        
        [Tooltip("If enabled the source of the notification will be shown in the history below the header.")]
        [SerializeField] private bool showSource = true;
        [Tooltip("Text that will be used as the prefix for the source of the notification.")]
        [SerializeField] private string sourcePrefix = "from ";
        
        [SerializeField] private GameObject emptyHistoryInfo;
        [SerializeField] private TextMeshProUGUI[] countDisplays;
        
        [SerializeField] private Transform historyTransform;
        [SerializeField] private GameObject notificationPrefab;
        
        private readonly DataToken HeaderKey      = new DataToken("header");      // string | title of the notification
        private readonly DataToken TextKey        = new DataToken("text");        // string | message of the notification
        private readonly DataToken ImageKey       = new DataToken("image");       // reference (Sprite) | the image to use
        private readonly DataToken SourceKey      = new DataToken("source");      // string | name of the system that sent the notification
        
        private DataList notifications = new DataList();

        private void Start()
        {
            NotificationsChanged();
        }

        public void _ClearHistory()
        {
            for (int i = 0; i < notifications.Count; i++)
            {
                ((NotificationHistoryObject)notifications[i].Reference)._Delete();
            }
            
            notifications.Clear();
            NotificationsChanged();
        }
        
        public void _AddNotification(DataDictionary data)
        {
            var newNotificationObj = Instantiate(notificationPrefab, historyTransform);
            newNotificationObj.transform.localScale = Vector3.one;
            newNotificationObj.transform.SetSiblingIndex(1);
            newNotificationObj.SetActive(true);

            var header = data[HeaderKey].String;
            header = string.IsNullOrEmpty(header) ? noHeaderText : header;
            
            var source = data[SourceKey].String;
            if (showSource && !string.IsNullOrEmpty(source)) source = sourcePrefix + source;
            
            var text = data[TextKey].String;
            text = string.IsNullOrEmpty(text) ? noMessageText : text;
            
            var image = (Sprite)data[ImageKey].Reference;
            var time = DateTime.Now.ToShortTimeString();
            
            var newNotificationScript = newNotificationObj.GetComponent<NotificationHistoryObject>();
            newNotificationScript._Setup(header, text, time, image, source);
            
            notifications.Add(newNotificationScript);
            RemoveLastIfFull();
            NotificationsChanged();
        }

        private void NotificationsChanged()
        {
            var isEmpty = notifications.Count == 0;
            emptyHistoryInfo.SetActive(isEmpty);

            if (countDisplays == null || countDisplays.Length == 0)
                return;

            foreach (var countDisplay in countDisplays)
            {
                if (!countDisplay)
                    continue;

                countDisplay.text = isEmpty ? string.Empty : $"({notifications.Count.ToString()})";
            } 
        }


        private void RemoveLastIfFull()
        {
            if (notifications.Count <= maxHistorySize) return;
         
            var last = notifications[0].Reference;
            notifications.RemoveAt(0);
            ((NotificationHistoryObject)last)._Delete();
        }
    }
}
