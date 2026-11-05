using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharpEditor;
#endif

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [RequireComponent(typeof(CustomNetNotificationSenderPresetSelectorInstance))]
    public class CustomNetNotificationSenderPresetSelector : MonoBehaviour
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [Tooltip("Custom Net Notification Sender that should have the presets applied to it.")]
        [SerializeField] private CustomNetNotificationSender customNetNotificationSender;
        [Tooltip("Whitelist the local player must be on to be able to set a preset.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("Presets that can be selected from the Selector UI.")]
        [SerializeField] private Preset[] presets = new Preset[1];
        
        [Serializable]
        private class Preset
        {
            public Preset()
            {
                header = "";
                message = "";
                iconId = 1;
                soundId = 1;
                duration = 3f;
            }
            
            [Tooltip("Header / Title of the notification.")]
            public string header;
            [TextArea(1, 3)] [Tooltip("Text / Message / Body of the notification.")]
            public string message;
            [Tooltip("Index of the image same order as on the Selector UI is used.\nSet to 0 to omit the image.")]
            public int iconId;
            [Tooltip("Index of the same order as on the Selector UI is used.\nSet to 0 to use the default.")]
            public int soundId;
            [Range(0f, 30f)] [Tooltip("How long to show the notification for in seconds, this does NOT include the start / end animation.")]
            public float duration;
        }

        /// <summary>
        /// Finds the <see cref="CustomNetNotificationSenderPresetSelectorInstance"/> and transfers the presets over to it.
        /// </summary>
        public void TransferValues()
        {
            var instance = GetComponent<CustomNetNotificationSenderPresetSelectorInstance>();
            if (instance == null)
            {
                MUIDebug.LogError("Custom Net Notification Preset Selector | No Custom Net Notification Preset Selector Instance found!", gameObject);
                return;
            }
            
            string[] headers = new string[presets.Length];
            string[] messages = new string[presets.Length];
            int[] iconIds = new int[presets.Length];
            int[] soundIds = new int[presets.Length];
            float[] durations = new float[presets.Length];
            
            for (int i = 0; i < presets.Length; i++)
            {
                var preset = presets[i];
                headers[i] = preset.header;
                messages[i] = preset.message;
                iconIds[i] = preset.iconId;
                soundIds[i] = preset.soundId;
                durations[i] = preset.duration;
            }
            
            instance.customNetNotificationSender = customNetNotificationSender;
            instance.whitelist = whitelist;
            instance.headers = headers;
            instance.messages = messages;
            instance.iconIds = iconIds;
            instance.soundIds = soundIds;
            instance.durations = durations;
            
            UdonSharpEditorUtility.CopyProxyToUdon(instance);
        }
#endif
    }
}
