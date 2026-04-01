
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NotificationHistoryObject : UdonSharpBehaviour
    {
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI notificationHeader;
        [SerializeField] private TextMeshProUGUI notificationMessage;
        [SerializeField] private TextMeshProUGUI notificationTime;
        
        [Header("Source Elements")]

        [SerializeField] private TextMeshProUGUI notificationSource;
        [SerializeField] private GameObject notificationSourceObject;

        [Header("Image Elements")] 
        [SerializeField] private Image notificationImage;
        [SerializeField] private GameObject notificationImageObject;
        
        public void _Setup(string header, string message, string timeString, Sprite image, string source)
        {
            if (image)
            {
                notificationImageObject.SetActive(true);
                notificationImage.sprite = image;
            }
            else
            {
                notificationImageObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(source))
            {
                notificationSourceObject.SetActive(true);
                notificationSource.text = source;
            }
            else
            {
                notificationSourceObject.SetActive(false);
            }
            
            notificationHeader.text = header;
            notificationMessage.text = message;
            notificationTime.text = timeString;
        }

        public void _Delete()
        {
            Destroy(gameObject);
        }
    }
}
