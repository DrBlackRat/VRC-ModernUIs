
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using DrBlackRat.VRC.ModernUIs.SelectorUI;
using DrBlackRat.VRC.ModernUIs.SliderUI;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CustomNetNotificationSender : UdonSharpBehaviour
    {
        [Tooltip("Net Notification Relay used to actually send the notification to everyone.")]
        [SerializeField] private NetNotificationRelay netNotificationRelay;
        [Tooltip("If provided only players who are on this whitelist will receive the notification. If left empty it will be send to everyone.")]
        [SerializeField] private WhitelistGetterBase receivingWhitelist;

        [Range(0, 1)] [Tooltip("Volume of the notification sound. Default is 0.25, clamped between 0 and 1.")]
        [SerializeField] private float soundVolume = 0.25f;
        [Range(100f, 200f)] [Tooltip("How wide the notification pop up will be, measured in pixel of the UI canvas.")]
        [SerializeField] private float width = 120f;
        
        [SerializeField] private TMP_InputField headerInputField;
        [SerializeField] private TMP_InputField messageInputField;
        [SerializeField] private SelectorUI.SelectorUI iconSelector;
        [SerializeField] private SelectorUI.SelectorUI soundSelector;
        [SerializeField] private SliderUI.SliderUI durationSlider;

        [HideInInspector] public int selectorIconId;
        [HideInInspector] public int selectorSoundId;
        [HideInInspector] public float sliderDuration;

        public void _ApplyValues(string header, string message, int iconId, int soundId, float duration)
        {
            headerInputField.text = header;
            messageInputField.text = message;
            iconSelector._ButtonSelected(iconId);
            soundSelector._ButtonSelected(soundId);
            durationSlider._UpdateValue(duration);
        }
        
        public void _SendNotification()
        {
            DataList players = null;
            if (receivingWhitelist != null)
            {
                players = receivingWhitelist._GetUsersAsList();
            }

            netNotificationRelay._SendNetNotification(players, headerInputField.text, messageInputField.text, 
                selectorIconId - 1, selectorSoundId != 0, selectorSoundId - 1, 
                soundVolume, sliderDuration, width);
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            if (netNotificationRelay != null)
                return;
            
            netNotificationRelay = FindObjectOfType<NetNotificationRelay>();
        }
#endif
    }
}
