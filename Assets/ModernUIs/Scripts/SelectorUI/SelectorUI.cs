using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUI : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [Tooltip("This is the default state the multi select will be in.")]
        [SerializeField] protected int selectedState;
        [Space(10)] 
        [Tooltip("When enabled, clicking on an activated button again will make the selection go back to the default state.")]
        [SerializeField] protected bool doubleClickToDefault;
        [Tooltip("Default State that is being set when clicking on an activated again. Only used if Double Click To Default is enabled. ")]
        [SerializeField] protected int doubleClickDefaultState;
        
        [Header("Toggles:")]
        [Tooltip("Objects that should be turned on / off. The same order as buttons will be used.")]
        [SerializeField] protected GameObject[] itemObjs;
        [Header("External Connection:")]
        [Tooltip("Udon Behaviours that will the connection data will be send to.")]
        [SerializeField] protected UdonBehaviour[] externalBehaviours;
        [Tooltip("Name of the Integer Variable that will be set when the selection changes.")]
        [SerializeField] protected string idValueName = "selectionId";
        [Tooltip("Name of Event that will be fired if the selection changes.")]
        [SerializeField] protected string changeEventName = "_SelectionChanged";

        [Header("Persistence:")]
        [Tooltip("Turn on if this SelectorUI should be saved using Persistence.")]
        [SerializeField] protected bool usePersistence;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] protected string dataKey = "CHANGE THIS";
        
        [Header("Internals:")]
        [SerializeField] protected SelectorUIButton[] selectorUIButtons;
        [SerializeField] protected GameObject selector;
        
        // UI Stuff
        [Header("Text Colors:")]
        [SerializeField] protected Color normalColor;
        [SerializeField] protected Color selectedColor;
        
        [Header("UI Animation:")]
        [SerializeField] protected AnimationCurve animationCurve;
        [SerializeField] protected float movementDuration;

        protected RectTransform selectorTransform;
        
        protected bool animateUI;
        protected float movementElapsedTime;
        protected Vector3 oldSelectorPos;

        protected int prevSelectedState;
        
        protected virtual void Start()
        {
            selectorTransform = selector.GetComponent<RectTransform>();

            for (int i = 0; i < selectorUIButtons.Length; i++)
            {
                selectorUIButtons[i].Setup(normalColor, selectedColor, animationCurve, movementDuration, this, i);
            }
            
            _UpdateSelection(selectedState, true, true, false);
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;
            if (PlayerData.TryGetInt(player, dataKey, out int value))
            {
                _UpdateSelection(value, true, false, false);
            }
        }

        protected void ChangeExternalSelection(int id, bool skipBehaviour)
        {
            if (itemObjs != null && itemObjs.Length > 0)
            {
                foreach (var item in itemObjs)
                {
                    if (item == null) continue;
                    item.SetActive(false);
                }
                if (itemObjs[id] != null) itemObjs[id].SetActive(true);
            }

            // skipBehaviour is currently only used by the Mirror UI to prevent the seperator from moving
            if (externalBehaviours != null && externalBehaviours.Length > 0 && !skipBehaviour)
            {
                foreach (var behaviour in externalBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SetProgramVariable(idValueName, id);
                    behaviour.SendCustomEvent(changeEventName);
                }
            }
        }

        public void _ButtonSelected(int buttonId)
        {
            if (doubleClickToDefault && buttonId == selectedState)
            {
                if (buttonId == doubleClickDefaultState) return;
                _UpdateSelection(doubleClickDefaultState, false, false, false);
            }
            else
            {
                _UpdateSelection(buttonId, false, false, false);
            }
        }
        
        public void _CustomUpdate()
        {
            if (!animateUI) return;
            AnimateUI(oldSelectorPos, selectorUIButtons[selectedState].Position());
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }

        protected virtual bool _UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (newState == selectedState && !skipSameCheck) return false;
            prevSelectedState = selectedState;
            selectedState = newState;

            ChangeExternalSelection(selectedState, false);
            
            if (usePersistence && !skipPersistence) PlayerData.SetInt(dataKey, selectedState);
            
            // Button Transition
            selectorUIButtons[prevSelectedState]._UpdateSelected(false);
            selectorUIButtons[selectedState]._UpdateSelected(true);
            
            // UI Animation
            animateUI = true;
            movementElapsedTime = 0f;
            oldSelectorPos = selectorTransform.position;
            _CustomUpdate();
            return true;
        }
        
        
        protected void AnimateUI(Vector3 startPos, Vector3 endPos)
        {
            movementElapsedTime += Time.deltaTime;
            var percentageComplete = movementElapsedTime / movementDuration;
            var smoothPercentageComplete = animationCurve.Evaluate(percentageComplete);
            // Set Selector Position
            selectorTransform.position = Vector3.LerpUnclamped(startPos, endPos, smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                movementElapsedTime = 0f;
                animateUI = false;
            }
        }
    }
}
