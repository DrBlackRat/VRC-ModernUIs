
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Components;
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
        
        [Tooltip("Optional | Can be used to show notifications in a history.")]
        [SerializeField] private NotificationHistory notificationHistory;
        [Tooltip("If enabled, the notification will be added to the history directly, otherwise it will only be added once the toast shows.\nRemoving a Notification after it was added will not remove it from the history if enabled.")]
        [SerializeField] private bool directlyAddToHistory = false;
        
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
        [Tooltip("Curve used for scaling the notification based on a desktop players FOV.\nLeft: 50°, Right 100°")]
        [SerializeField] private AnimationCurve fovScaleCurve = AnimationCurve.Linear(0f,0.8f, 1f, 1.8f);
        
        private float currentShownDuration;
        private int currentShownId = -1;

        private VRCTweenHandle sliderTweenHandle;
        private VRCTweenHandle posTweenHandle;
        private VRCTweenHandle scaleTweenHandle;
        private VRCTweenHandle eventTweenHandle;
        
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
        private readonly DataToken SaveHistoryKey = new DataToken("saveHistory"); // bool | if the notification should be saved / shown in the history
        private readonly DataToken SourceKey      = new DataToken("source");      // string | name of the system that sent the notification
        
        private VRCPlayerApi localPlayer;
        private float rootCanvasSizeY;
        
        private const float MinDesktopFov = 50f;
        private const float MaxDesktopFov = 100f;
        
        #region Unity & VRChat Events
        private void OnDisable()
        {
            _ClearNotifications();
            
            PauseAllTween();
            currentShownId = -1;
            notificationObject.SetActive(false);
        }

        private void Start()
        {
            notificationObject.SetActive(false);
            localPlayer = Networking.LocalPlayer;
            rootCanvasSizeY = canvasRootTransform.sizeDelta.y;
            
            notificationHeader = notificationHeaderObj.GetComponent<TextMeshProUGUI>();
            notificationText = notificationTextObj.GetComponent<TextMeshProUGUI>();
            SetupTween();
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
        /// <param name="image">Image shown next to the header and text. If not provided, none will be shown.</param>
        /// <param name="playSound">If a notification sound should be played.</param>
        /// <param name="soundClip">Audio Clip used for the notification, if not provided, the default will be used.</param>
        /// <param name="soundVolume">Volume of the notification sound. The default is 0.25, clamped between 0 and 1.</param>
        /// <param name="duration">How long the notification should be shown for in seconds. Default is 3, clamped between 0 and 60.</param>
        /// <param name="width">How wide the canvas should be. Default is 100, clamped between 100 and 200.</param>
        /// <param name="source">Where this notification comes from, usually the name of the system that sends it.</param>
        /// <param name="showInHistory">If the notification should be displayed in the notification history.</param>
        /// <returns>-1 if the notification could not be added, otherwise id of the notification.</returns>
        public int _AddNotification(string header, string text, Sprite image = null, 
            bool playSound = true, AudioClip soundClip = null, float soundVolume = 0.25f, 
            float duration = 3f, float width = 100f, bool showInHistory = true, string source = "")
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
            notificationDict.SetValue(WidthKey, Mathf.Clamp(width, 100, 200));
            notificationDict.SetValue(SaveHistoryKey, showInHistory);
            notificationDict.SetValue(SourceKey, source);
            
            notifications.Add(lastNotificationId,notificationDict);
            notificationIds.Add(lastNotificationId);
            
            NotificationQueueChanged();
            
            if (directlyAddToHistory) 
                TryAddNotificationToHistory(notificationDict);
            
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
            if (removeWhileShown && currentShownId == id)
            {
                _StartHideAnimation();
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
        /// Removes all notifications from the queue. Does NOT hide the notification that is currently being shown.
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
            if (currentShownId == -1) 
                return false;

            _StartHideAnimation();
            return true;
        }
        #endregion

        private bool TryAddNotificationToHistory(DataDictionary notificationDict)
        {
            if (!notificationHistory || notificationDict == null || !notificationDict[SaveHistoryKey].Boolean)
                return false;
                
            notificationHistory._AddNotification(notificationDict);
            return true;
        }
        
        private void NotificationQueueChanged()
        {
            PlayNextNotification();
        }
        
        private void PlayNextNotification()
        {
            if (notifications.Count == 0 ||  currentShownId != -1) 
                return;

            currentShownId = notificationIds[0].Int;
            var notificationDict = notifications[currentShownId].DataDictionary;
            
            // History
            if (!directlyAddToHistory) 
                TryAddNotificationToHistory(notificationDict);

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
            
            StartAnimation();
            _RemoveFirstNotification();
        }

        private void UpdatePlayerHeadData()
        {
            if (currentShownId == -1) 
                return;
            
            transform.position = VRCCameraSettings.ScreenCamera.Position;
            transform.rotation = VRCCameraSettings.ScreenCamera.Rotation;
        }

        #region Animation
        private void SetupTween()
        {
            sliderTweenHandle = countdownSlider.TweenValue(0f, 1f, VRCTweenEase.Linear)
                .Pause();
            
            posTweenHandle = notificationTransform.gameObject.TweenLocalPosition(new Vector3(), 1f, VRCTweenEase.None)
                .SetEase(smoothingCurve)
                .Pause();
            
            scaleTweenHandle = notificationTransform.gameObject.TweenScale(new Vector3(), 1f, VRCTweenEase.None)
                .SetEase(smoothingCurve)
                .Pause();
            
            eventTweenHandle = VRCTween.DelayedCall(this, "", 1f)
                .Pause();
        }

        private void PauseAllTween()
        {
            sliderTweenHandle.Pause();
            posTweenHandle.Pause();
            scaleTweenHandle.Pause();
            eventTweenHandle.Kill();
        }
        
        private Vector3 GetCurrentShownScale()
        {
            if (localPlayer.IsUserInVR())
                return shownScale;
            
            var normalizedFov = Mathf.InverseLerp(
                MinDesktopFov,
                MaxDesktopFov,
                VRCCameraSettings.ScreenCamera.FieldOfView);

            return shownScale * fovScaleCurve.Evaluate(normalizedFov);
        }
        
        private void StartAnimation()
        {
            notificationObject.SetActive(true);
            
            countdownSlider.value = 1;
            sliderTweenHandle.SetDuration(currentShownDuration + startAnimDuration + endAnimDuration)
                .Restart();
            
            notificationTransform.localPosition = hiddenPos;
            posTweenHandle.SetDuration(startAnimDuration)
                .ChangeEndValue(shownPos, true)
                .Restart();

            notificationTransform.localScale = hiddenScale;
            scaleTweenHandle.SetDuration(startAnimDuration)
                .ChangeEndValue(GetCurrentShownScale(), true)
                .Restart();
            
            eventTweenHandle = VRCTween.DelayedCall(this, nameof(_StartHideAnimation), currentShownDuration + startAnimDuration);
        }
        
        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Starts the hiding animation for the notification.
        /// </summary>
        public void _StartHideAnimation()
        {
            posTweenHandle.SetDuration(endAnimDuration)
                .ChangeEndValue(hiddenPos, true)
                .Restart();
            scaleTweenHandle.SetDuration(endAnimDuration)
                .ChangeEndValue(hiddenScale, true)
                .Restart();
            
            eventTweenHandle.Kill();
            eventTweenHandle = VRCTween.DelayedCall(this, nameof(_HideAnimationFinished), endAnimDuration);
        }

        /// <summary>
        /// INTERNAL | DO NOT CALL MANUALLY
        /// Ends the hiding animation for the notification and triggers the next notification.
        /// </summary>
        public void _HideAnimationFinished()
        {
            PauseAllTween();
            
            currentShownId = -1;
            notificationObject.SetActive(false);
            
            PlayNextNotification();
        }
        #endregion
    }
}

