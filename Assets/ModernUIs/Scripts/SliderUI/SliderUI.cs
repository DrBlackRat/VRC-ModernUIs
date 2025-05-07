using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Utils
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


        private void Start()
        {
            slider.SetValueWithoutNotify(value);
            UpdateExternalSlider(value);
        }

        public void _ValueUpdated()
        {
            value = slider.value;
            UpdateExternalSlider(value);
            if (usePersistence) PlayerData.SetFloat(dataKey, value);
        }

        private void UpdateExternalSlider(float newValue)
        {
            if (postProcessVolume != null) postProcessVolume.weight = newValue;
            
            if (sliderBehaviour == null) return;
            sliderBehaviour.SetProgramVariable(variableName, newValue);
            sliderBehaviour.SendCustomEvent(updateEventName);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;
            if (PlayerData.TryGetFloat(player, dataKey, out float newValue))
            {
                value = newValue;
                UpdateExternalSlider(value);
                slider.SetValueWithoutNotify(value);
            }

        }
    }
}
