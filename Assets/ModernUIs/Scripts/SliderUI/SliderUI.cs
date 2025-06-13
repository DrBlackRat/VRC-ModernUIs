using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SliderUI : UdonSharpBehaviour
    {
        [Tooltip("This is the default value the slider will have.")]
        [SerializeField] private float value = 0f;

        [Tooltip("Post Process Volume of which the weight will be set by the slider.")]
        [SerializeField] private PostProcessVolume postProcessVolume;
        [Tooltip("Udon Behaviour that should be updated.")]
        [SerializeField] private UdonBehaviour sliderBehaviour;
        [Tooltip("Event that will be called on the Slider Behaviour if the value changes.")]
        [SerializeField] private string updateEventName = "_SliderUpdated";
        [Tooltip("Variable that will be updated on the Slider Behaviour if the value changes.")]
        [SerializeField] private string variableName = "sliderValue";
        
        [Tooltip("Turn on if this Slider should be saved using Persistence.")]
        [SerializeField] private bool usePersistence = true;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] private string dataKey = "CHANGE THIS";
        
        [SerializeField] private Slider slider;
        
        [Tooltip("Text that the slider value should be displayed at. Formated as whole numbers from 0 - 100, so 0.5 would be 50. Can be left empty if not needed.")]
        [SerializeField] private TextMeshProUGUI sliderText;

        [Tooltip("If enabled the slider will snap to the provided Snap Interval.")]
        [SerializeField] private bool snapSlider;
        [Tooltip("Interval the slider would snap to, e.g.  0.2 means it will snap to 0, 0.2, 0.4 etc.")]
        [SerializeField] private float snapInterval = 0.1f;


        private void Start()
        {
            UpdateValue(value, false, true, true);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;
            if (PlayerData.TryGetFloat(player, dataKey, out float newValue))
            {
                UpdateValue(value, true, false, false);
            }

        }
        
        public void _ValueUpdated()
        {
            UpdateValue(slider.value, false, false, false);
        }

        protected bool UpdateValue(float newValue, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (Mathf.Approximately(newValue, value) && !skipSameCheck) return false;
            if (snapSlider)
            {
                var rounded = Mathf.Round(newValue / snapInterval) * snapInterval;
                value = rounded;
            }
            else
            {
                value = newValue;
            }

            if (usePersistence && !skipPersistence)
            {
                PlayerData.SetFloat(dataKey, value);
            }
            
            slider.SetValueWithoutNotify(value);
            UpdateExternal(value);
            UpdateSliderText(value);
            return true;
        }
        
        private void UpdateExternal(float newValue)
        {
            if (postProcessVolume != null) postProcessVolume.weight = newValue;
            
            if (sliderBehaviour == null) return;
            sliderBehaviour.SetProgramVariable(variableName, newValue);
            sliderBehaviour.SendCustomEvent(updateEventName);
            
        }

        private void UpdateSliderText(float newValue)
        {
            if (sliderText != null)
            {
                var temp = newValue * 100;
                sliderText.text = temp.ToString("0");
            } 
        }


    }
}
