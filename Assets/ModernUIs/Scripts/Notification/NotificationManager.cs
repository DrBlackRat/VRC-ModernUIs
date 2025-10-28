
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
        [SerializeField] private GameObject notificationObject;
        [SerializeField] private Transform notificationTransform;
        [SerializeField] private RectTransform canvasRootTransform;
        [SerializeField] private TextMeshProUGUI notificationHeader;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private GameObject imageContainer;
        [SerializeField] private Image notificationImage;
        [SerializeField] private Slider countdownSlider;
        
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

        private DataList headers = new DataList();
        private DataList texts = new DataList();
        private DataList images = new DataList();
        
        private DataList playSounds = new DataList();
        private DataList soundVolumes = new DataList();
        private DataList soundClips = new DataList();
        
        private DataList durations = new DataList();
        private DataList widths = new DataList();

        private bool showingNotification;
        private float currentShownDuration;
        private float elpasedTime;

        private VRCPlayerApi localPlayer;

        private void OnDisable()
        {
            elpasedTime = 0f;
            showingNotification = false;
            notificationObject.SetActive(false);
            
            _ClearNotifications();
        }

        private void Start()
        {
            notificationObject.SetActive(false);
            localPlayer = Networking.LocalPlayer;
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
        /// <returns>True if successful, false if not.</returns>
        public bool _AddNotification(string header, string text, Sprite image = null, 
            bool playSound = true, AudioClip soundClip = null, float soundVolume = 0.25f, 
            float duration = 3f, float width = 80f)
        {
            if (!enabled) 
                return false;
            
            headers.Add(header);
            texts.Add(text);
            images.Add(image);
            
            playSounds.Add(playSound);
            soundClips.Add(soundClip ? soundClip : defaultAudioClip);
            soundVolumes.Add(Mathf.Clamp(soundVolume, 0, 1));
            
            durations.Add(Mathf.Clamp(duration, 0, 60));
            widths.Add(Mathf.Clamp(width, 50, 150));
            
            NotificationQueueChanged();
            return true;
        }

        /// <summary>
        /// Removes the notification at the start of the queue.
        /// </summary>
        /// <returns>Bool | True if successful, false if not.</returns>
        public bool _RemoveFirstNotification()
        {
            if (headers.Count == 0)
                return false;
            
            headers.RemoveAt(0);
            texts.RemoveAt(0);
            images.RemoveAt(0);
            
            playSounds.RemoveAt(0);
            soundClips.RemoveAt(0);
            soundVolumes.RemoveAt(0);
            
            durations.RemoveAt(0);
            widths.RemoveAt(0);
            
            NotificationQueueChanged();
            return true;
        }

        /// <summary>
        /// Removes all notifications from the queue.
        /// </summary>
        public void _ClearNotifications()
        {
            headers = new DataList();
            texts = new DataList();
            images = new DataList();
            
            playSounds = new DataList();
            soundVolumes = new DataList();
            soundClips = new DataList();
            
            durations = new DataList();
            widths = new DataList();
            
            NotificationQueueChanged();
        }

        public void NotificationQueueChanged()
        {
            PlayNextNotification();
        }

        private void PlayNextNotification()
        {
            if (headers.Count == 0 || showingNotification)
                return;

            // Get & Set Data
            notificationHeader.text = headers[0].String;
            notificationText.text = texts[0].String;
            currentShownDuration = durations[0].Float;
            canvasRootTransform.sizeDelta = new Vector2(widths[0].Float, canvasRootTransform.sizeDelta.y);
            var sprite = (Sprite)images[0].Reference;
            if (sprite != null)
            {
                imageContainer.SetActive(true);
                notificationImage.sprite = sprite;
            }
            else
            {
                imageContainer.SetActive(false);
            }
            
            if (notificationAudioSource != null && playSounds[0].Boolean)
                notificationAudioSource.PlayOneShot((AudioClip)soundClips[0].Reference, soundVolumes[0].Float);
            
            // Reset
            notificationObject.SetActive(true);
            countdownSlider.value = 1;
            notificationTransform.localPosition = hiddenPos;
            notificationTransform.localScale = hiddenScale;
            
            // Animation
            showingNotification = true;
            elpasedTime = 0f;
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
            elpasedTime += Time.deltaTime;
            var totalPercentageComplete = elpasedTime / (currentShownDuration + startAnimDuration + endAnimDuration);

            if (elpasedTime <= startAnimDuration) // Start
            {
                var percentageComplete = elpasedTime / startAnimDuration;
                ApplyTransformLerp(smoothingCurve.Evaluate(percentageComplete));
                
            } else if (elpasedTime >= currentShownDuration + startAnimDuration) // End
            {
                var percentageComplete = (elpasedTime - currentShownDuration - startAnimDuration) / endAnimDuration;
                ApplyTransformLerp(smoothingCurve.Evaluate(1f - percentageComplete));
            }
            
            countdownSlider.value = 1f - totalPercentageComplete;

            if (totalPercentageComplete >= 1)
            {
                elpasedTime = 0f;
                showingNotification = false;
                notificationObject.SetActive(false);

                _RemoveFirstNotification();
            }
        }

        private void ApplyTransformLerp(float percentageComplete)
        {
            notificationTransform.localPosition = Vector3.LerpUnclamped(hiddenPos, shownPos, percentageComplete);
            notificationTransform.localScale = Vector3.LerpUnclamped(hiddenScale, shownScale, percentageComplete);
        }
    }
}

