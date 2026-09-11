
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.SelectorUI;
using DrBlackRat.VRC.ModernUIs.SliderUI;
using UnityEngine.Serialization;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-1000)]
    public class ResetUiToDefault : UdonSharpBehaviour
    {
        [Tooltip("Selector UIs which should be reset when pressing the Reset button.")]
        [SerializeField] private SelectorUI.SelectorUI[] selectorUIs;
        private int[] selectorUIDefaultValues;
        
        [Tooltip("Slider UIs which should be reset when pressing the Reset button.")]
        [SerializeField] private SliderUI.SliderUI[] sliderUIs;
        private float[] sliderUIDefaultValues;

        private void Start()
        {
            selectorUIDefaultValues = new int[selectorUIs.Length];
            for (int i = 0; i < selectorUIs.Length; i++)
            {
                selectorUIDefaultValues[i] = selectorUIs[i].SelectedState;
            }
            
            sliderUIDefaultValues = new float[sliderUIs.Length];
            for (int i = 0; i < sliderUIs.Length; i++)
            {
                sliderUIDefaultValues[i] = sliderUIs[i].Value;
            }
        }

        public void _Reset()
        {
            for (int i = 0; i < selectorUIs.Length; i++)
            {
                selectorUIs[i]._ButtonSelected(selectorUIDefaultValues[i]);
            }
            
            for (int i = 0; i < sliderUIs.Length; i++)
            {
                sliderUIs[i]._UpdateValue(sliderUIDefaultValues[i]);
            }
        }
    }
}

