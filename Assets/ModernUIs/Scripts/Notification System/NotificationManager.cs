
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
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
        private DataList durations = new DataList();
        private DataList widths = new DataList();

        private bool showingNotification;
        private float currentShownDuration;
        private float elpasedTime;

        private VRCPlayerApi localPlayer;

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
        /// <param name="header">String | Title of the notification.</param>
        /// <param name="text">String | Content of the notification.</param>
        /// <param name="playSound">Bool | True when a notification sounds should be played, false when not.</param>
        /// <param name="image">Optional Sprite | Image shown next to the header and text. If not provided none will be shown.</param>
        /// <param name="duration">Optional Float | How long the notification should be shown for in seconds, if not provided the default will be used.</param>
        /// <param name="width">Optional Float | How wide the canvas should be. Default is 80, clamped between 50 and 150.</param>
        public void AddNotification(string header, string text, bool playSound, Sprite image = null,  float duration = 3f, float width = 80f)
        {
            headers.Add(header);
            texts.Add(text);
            images.Add(image);
            playSounds.Add(playSound);
            durations.Add(duration);
            widths.Add(Mathf.Clamp(width, 50, 150));
            
            NotificationQueueChanged();
        }

        /// <summary>
        /// Removes the notification at the start of the queue.
        /// </summary>
        /// <returns>Bool | True if successful, false if not.</returns>
        public bool RemoveFirstNotification()
        {
            if (headers.Count == 0)
                return false;
            
            headers.RemoveAt(0);
            texts.RemoveAt(0);
            images.RemoveAt(0);
            playSounds.RemoveAt(0);
            durations.RemoveAt(0);
            widths.RemoveAt(0);
            
            NotificationQueueChanged();
            return true;
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
            
            if (notificationAudioSource != null && playSounds[0].Boolean == true)
                notificationAudioSource.Play();
            
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
                var headData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                transform.position = headData.position;
                transform.rotation = headData.rotation;
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

                RemoveFirstNotification();
            }
        }

        private void ApplyTransformLerp(float percentageComplete)
        {
            notificationTransform.localPosition = Vector3.LerpUnclamped(hiddenPos, shownPos, percentageComplete);
            notificationTransform.localScale = Vector3.LerpUnclamped(hiddenScale, shownScale, percentageComplete);
        }
    }
}

