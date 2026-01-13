
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class NotificationManager : UdonSharpBehaviour
    {
        [Tooltip("If enabled the notification queue will be limited to the length set below.\nIf limit is reached you can not add any more notifications until one is removed or has been played.")]
        [SerializeField] private bool limitNotificationQueueLength = true;
        [Range(1, 50)][Tooltip("How long the notification queue can get before new notifications can't be added.")]
        [SerializeField] private int maxNotificationQueueLength = 10;
        
        [SerializeField] private GameObject notificationObject;
        [SerializeField] private Transform notificationTransform;
        [SerializeField] private RectTransform canvasRootTransform;
        [SerializeField] private GameObject notificationHeaderObj;
        [SerializeField] private GameObject notificationTextObj;
        [SerializeField] private GameObject imageContainer;
        [SerializeField] private Image notificationImage;
        [SerializeField] private Slider countdownSlider;
        
        private TextMeshProUGUI notificationHeader;
        private TextMeshProUGUI notificationText;
        
        [Tooltip("Audio Source used for playing the notification sound.")]
        [SerializeField] private AudioSource notificationAudioSource;
        [Tooltip("Audi Clip used if none is provided by the notification.")]
        [SerializeField] private AudioClip defaultAudioClip;
        
        [SerializeField] private AnimationCurve smoothingCurve;
        [Tooltip("Time spend animating to show the UI.")] [Range(0f, 5f)]
        [SerializeField] private float startAnimDuration = 0.5f;
        [Tooltip("Time spend animating to hide the UI.")] [Range(0f, 5f)]
        [SerializeField] private float endAnimDuration = 0.5f;
        
        [SerializeField] private Vector3 shownPos;
        [SerializeField] private Vector3 hiddenPos;
        
        [SerializeField] private Vector3 shownScale = Vector3.one;
        [SerializeField] private Vector3 hiddenScale = Vector3.zero;
        
        private bool showingNotification;
        private float currentShownDuration;
        private int currentShownId = -1;
        private float elapsedTime;
        
        // Notification Data:
        private DataDictionary notifications = new DataDictionary(); 
        private DataList notificationIds = new DataList();
        
        private int lastNotificationId = 0; // Tracks the ID of the last notification that was added.
        private readonly DataToken HeaderKey      = new DataToken("header");      // string | title of the notification
        private readonly DataToken TextKey        = new DataToken("text");        // string | message of the notification
        private readonly DataToken ImageKey       = new DataToken("image");       // reference (Sprite) | the image to use
        private readonly DataToken PlaySoundKey   = new DataToken("playSound");   // bool | if sound should be played
        private readonly DataToken SoundClipKey   = new DataToken("soundClip");   // reference (Audio Clip) | the audio clip to use
        private readonly DataToken SoundVolumeKey = new DataToken("soundVolume"); // float | volume of the notification sound
        private readonly DataToken DurationKey    = new DataToken("duration");    // float | how long the notification stays on screen
        private readonly DataToken WidthKey       = new DataToken("width");       // float | how wide the notification should be
        
        private VRCPlayerApi localPlayer;
        private float rootCanvasSizeY;
        
        #region Unity & VRChat Events
        private void OnDisable()
        {
            elapsedTime = 0f;
            showingNotification = false;
            notificationObject.SetActive(false);
            
            _ClearNotifications();
        }

        private void Start()
        {
            notificationObject.SetActive(false);
            localPlayer = Networking.LocalPlayer;
            rootCanvasSizeY = canvasRootTransform.sizeDelta.y;
            
            notificationHeader = notificationHeaderObj.GetComponent<TextMeshProUGUI>();
            notificationText = notificationTextObj.GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (showingNotification)
                Animate();
        }

        public override void PostLateUpdate()
        {
            UpdatePlayerHeadData();
        }
        #endregion
        
        # region API
        /// <summary>
        /// Adds a notification to the notification queue, if multiple are added they will play one after the other.
        /// </summary>
        /// <param name="header">Title of the notification.</param>
        /// <param name="text">Content of the notification.</param>
        /// <param name="image">Image shown next to the header and text. If not provided none will be shown.</param>
        /// <param name="playSound">If a notification sound should be played.</param>
        /// <param name="soundClip">Audio Clip used for the notification, if not provided the default will be used.</param>
        /// <param name="soundVolume">Volume of the notification sound. Default is 0.25, clamped between 0 and 1.</param>
        /// <param name="duration">How long the notification should be shown for in seconds. Default is 3, clamped between 0 and 60.</param>
        /// <param name="width">How wide the canvas should be. Default is 80, clamped between 50 and 150.</param>
        /// <returns>-1 if the notification could not be added, otherwise id of the notification.</returns>
        public int _AddNotification(string header, string text, Sprite image = null, 
            bool playSound = true, AudioClip soundClip = null, float soundVolume = 0.25f, 
            float duration = 3f, float width = 80f)
        {
            if (!enabled || !gameObject.activeInHierarchy) 
                return -1;
            if (limitNotificationQueueLength && notifications.Count >= maxNotificationQueueLength)
                return -1;
            
            DataDictionary notificationDict = new DataDictionary();
            lastNotificationId++;
            
            notificationDict.SetValue(HeaderKey, header);
            notificationDict.SetValue(TextKey, text);
            notificationDict.SetValue(ImageKey, image);
            notificationDict.SetValue(PlaySoundKey, playSound);
            notificationDict.SetValue(SoundClipKey, soundClip ? soundClip : defaultAudioClip);
            notificationDict.SetValue(SoundVolumeKey, Mathf.Clamp(soundVolume, 0, 1));
            notificationDict.SetValue(DurationKey, Mathf.Clamp(duration, 0, 60));
            notificationDict.SetValue(WidthKey, Mathf.Clamp(width, 50, 150));
            
            notifications.Add(lastNotificationId,notificationDict);
            notificationIds.Add(lastNotificationId);
            
            NotificationQueueChanged();
            
            return lastNotificationId;
        }

        /// <summary>
        /// Removes a notification from the queue.
        /// If the notification is already playing, it will not be removed.
        /// </summary>
        /// <param name="id">ID of the notification to remove.</param>
        /// <param name="removeWhileShown">If enabled, the notification will be instantly hidden if it's already being shown.</param>
        /// <returns>True if successful, false if not.</returns>
        public bool _RemoveNotification(int id, bool removeWhileShown = false)
        {
            if (id <= 0 || id >= lastNotificationId)
                return false;

            if (showingNotification && !removeWhileShown && currentShownId == id)
            {
                _HideShownNotification();
                return true;
            }

            if (!notificationIds.Contains(id))
                return false;
            
            notifications.Remove(id);
            notificationIds.Remove(id);
            
            NotificationQueueChanged();
            return true;
        }

        /// <summary>
        /// Removes the notification at the start of the queue.
        /// </summary>
        /// <returns>Bool | True if successful, false if not.</returns>
        public bool _RemoveFirstNotification()
        {
            if (notifications.Count == 0)
                return false;
            
            notifications.Remove(notificationIds[0]);
            notificationIds.RemoveAt(0);
            
            NotificationQueueChanged();
            return true;
        }

        /// <summary>
        /// Removes all notifications from the queue.
        /// </summary>
        public void _ClearNotifications()
        {
            notifications.Clear();
            notificationIds.Clear();
            
            NotificationQueueChanged();
        }

        /// <summary>
        /// Hides the currently shown notification.
        /// </summary>
        /// <returns>Bool | True if successful, false if not.</returns>
        public bool _HideShownNotification()
        {
            if (!showingNotification) 
                return false;
            
            elapsedTime = currentShownDuration + startAnimDuration;
            return true;
        }
        #endregion

        public void NotificationQueueChanged()
        {
            PlayNextNotification();
        }

        private void PlayNextNotification()
        {
            if (notifications.Count == 0 || showingNotification) 
                return;

            currentShownId = notificationIds[0].Int;
            var notificationDict = notifications[notificationIds[0]].DataDictionary;

            // Notification Sound
            if (notificationAudioSource != null && notificationDict[PlaySoundKey].Boolean)
            {
                notificationAudioSource.PlayOneShot((AudioClip)notificationDict[SoundClipKey].Reference, notificationDict[SoundVolumeKey].Float);
            }
            
            var image = (Sprite)notificationDict[ImageKey].Reference;
            if (image != null)
            {
                imageContainer.SetActive(true);
                notificationImage.sprite = image;
            }
            else
            {
                imageContainer.SetActive(false);
            }
            
            var header = notificationDict[HeaderKey].String;
            notificationHeader.text = header;
            notificationHeaderObj.SetActive(!string.IsNullOrEmpty(header));

            var text = notificationDict[TextKey].String;
            notificationText.text = text;
            notificationTextObj.SetActive(!string.IsNullOrEmpty(text));
            
            currentShownDuration = notificationDict[DurationKey].Float;
            canvasRootTransform.sizeDelta = new Vector2(notificationDict[WidthKey].Float, rootCanvasSizeY);
            
            // Reset
            notificationObject.SetActive(true);
            countdownSlider.value = 1;
            notificationTransform.localPosition = hiddenPos;
            notificationTransform.localScale = hiddenScale;
            
            // Animation
            showingNotification = true;
            elapsedTime = 0f;
            
            _RemoveFirstNotification();
        }

        private void UpdatePlayerHeadData()
        {
            if (showingNotification)
            {
                transform.position = VRCCameraSettings.ScreenCamera.Position;
                transform.rotation = VRCCameraSettings.ScreenCamera.Rotation;
            }
        }

        private void Animate()
        {
            elapsedTime += Time.deltaTime;
            var totalPercentageComplete = elapsedTime / (currentShownDuration + startAnimDuration + endAnimDuration);

            if (elapsedTime <= startAnimDuration) // Start
            {
                var percentageComplete = elapsedTime / startAnimDuration;
                ApplyTransformLerp(smoothingCurve.Evaluate(percentageComplete));
                
            } else if (elapsedTime >= currentShownDuration + startAnimDuration) // End
            {
                var percentageComplete = (elapsedTime - currentShownDuration - startAnimDuration) / endAnimDuration;
                ApplyTransformLerp(smoothingCurve.Evaluate(1f - percentageComplete));
            }
            
            countdownSlider.value = 1f - totalPercentageComplete;

            if (totalPercentageComplete >= 1)
            {
                elapsedTime = 0f;
                currentShownId = -1;
                showingNotification = false;
                notificationObject.SetActive(false);
                
                PlayNextNotification();
            }
        }

        private void ApplyTransformLerp(float percentageComplete)
        {
            notificationTransform.localPosition = Vector3.LerpUnclamped(hiddenPos, shownPos, percentageComplete);
            notificationTransform.localScale = Vector3.LerpUnclamped(hiddenScale, shownScale, percentageComplete);
        }
    }
}

