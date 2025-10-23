
using DrBlackRat.VRC.ModernUIs;
using DrBlackRat.VRC.ModernUIs.Notifications;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NotificationTest : UdonSharpBehaviour
{
    public string header;
    [TextArea(3, 3)]
    public string text;
    public bool playSound;
    public Sprite image;
    public float duration;
    public float width;
    [Space(10)] 
    public NotificationManager notificationManager;

    public void Notify()
    {
        notificationManager._AddNotification(header, text, playSound, image, duration, width);
    }
}
