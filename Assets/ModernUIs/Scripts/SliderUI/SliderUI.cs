﻿using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.Helpers;

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SliderUI : UdonSharpBehaviour
    {
        [Tooltip("This is the default value the slider will have.")]
        [SerializeField] protected float value = 0f;

        [Tooltip("Post Process Volume of which the weight will be set by the slider.")]
        [SerializeField] protected PostProcessVolume postProcessVolume;
        [Tooltip("Udon Behaviours that should be updated.")]
        [SerializeField] protected UdonBehaviour[] externalBehaviours;
        [Tooltip("Event that will be called on the Slider Behaviour if the value changes.")]
        [SerializeField] protected string updateEventName = "_SliderUpdated";
        [Tooltip("Variable that will be updated on the Slider Behaviour if the value changes.")]
        [SerializeField] protected string variableName = "sliderValue";
        
        [Tooltip("Turn on if this Slider should be saved using Persistence.")]
        [SerializeField] protected bool usePersistence;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] protected string dataKey = "CHANGE THIS";
        
        [SerializeField] protected Slider slider;
        
        [Tooltip("Text that the slider value should be displayed at. Formated as whole numbers from 0 - 100, so 0.5 would be 50. Can be left empty if not needed.")]
        [SerializeField] protected TextMeshProUGUI sliderText;
        [Tooltip("Changes what the slider number will be multiplied by for display.\n- 1 would keep 0.5 as 0.5\n- 100 would turn 0.5 to 50")]
        [SerializeField] protected float sliderDisplayMultiplier = 100f;
        [Tooltip("What the slider value will be formated as.\n- 0 means it will always at least show one digit\n- 00 means it will fill always be formated as two digits")]
        [SerializeField] protected string sliderDisplayFormat = "0";
        [Tooltip("Text that would be placed at the end of the slider display value, useful for things like \"%\" or \"°\".")]
        [SerializeField] protected string sliderDisplaySuffix;
        
        [Tooltip("Interval the slider would snap to, e.g.  0.2 means it will snap to 0, 0.2, 0.4 etc.")]
        [Range(0.001f, 1f)]
        [SerializeField] protected float snapInterval = 0.001f;


        protected virtual void Start()
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

        protected virtual bool UpdateValue(float newValue, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            newValue = Mathf.Round(newValue / snapInterval) * snapInterval;
            slider.SetValueWithoutNotify(newValue); // Is done before the same check as slider snapping requires it
            
            if (Mathf.Approximately(newValue, value) && !skipSameCheck) return false;
            value = newValue;
            
            if (usePersistence && !skipPersistence)
            {
                PlayerData.SetFloat(dataKey, value);
            }
            UpdateExternal(value);
            UpdateSliderText(value);
            return true;
        }
        
        protected void UpdateExternal(float newValue)
        {
            if (postProcessVolume != null) postProcessVolume.weight = newValue;

            if (externalBehaviours != null && externalBehaviours.Length > 0)
            {
                foreach (var behaviour in externalBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SetProgramVariable(variableName, newValue);
                    behaviour.SendCustomEvent(updateEventName);
                }
            }
        }

        protected void UpdateSliderText(float newValue)
        {
            if (sliderText != null)
            {
                var temp = newValue * sliderDisplayMultiplier;
                sliderText.text = temp.ToString(sliderDisplayFormat) + sliderDisplaySuffix;
            } 
        }


    }
}
