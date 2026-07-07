using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUI : UdonSharpBehaviour
    {
        [Tooltip("This is the default state the multi select will be in.")]
        [SerializeField] protected int selectedState;
        public int SelectedState => selectedState;

        [Tooltip("When enabled, clicking on an activated button again will make the selection go back to the default state.")]
        [SerializeField] protected bool secondClick;
        [Tooltip("State that is being set when clicking on an activated again. Only used if Double Click To Default is enabled. ")]
        [SerializeField] protected int secondClickState;
        
        [Tooltip("Turn on if this SelectorUI should be saved using Persistence. For Synced Selector UIs it will only try to load for the first player.")]
        [SerializeField] protected bool usePersistence;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] protected string dataKey = "CHANGE THIS";
        
        [Tooltip("Objects that should be turned on / off. The same order as buttons in the children of the Selector UI will be used (top to bottom).")]
        [SerializeField] protected GameObject[] itemObjs;
        
        [Tooltip("Animators that should have an int with the given parameter name set for the selected button.")]
        [SerializeField] protected Animator[] animators;
        [Tooltip("Parameter Name of the int that should be set.")]
        [SerializeField] protected string animParamName = "selectionId";
        
        [Tooltip("Udon Behaviours that the connection data will be send to.")]
        [SerializeField] protected UdonBehaviour[] externalBehaviours;
        [Tooltip("Name of the Integer Variable that will be set when the selection changes.")]
        [SerializeField] protected string idValueName = "selectionId";
        [Tooltip("Name of Event that will be fired if the selection changes.")]
        [SerializeField] protected string changeEventName = "_SelectionChanged";
        
        [SerializeField] protected Vector2 buttonNormalScale;
        [SerializeField] protected Vector2 buttonSelectedScale;
        [SerializeField] protected float buttonNormalPixelPerUnit;
        [SerializeField] protected float buttonSelectedPixelPerUnit;

        public Vector2 ButtonNormalScale => buttonNormalScale;
        public Vector2 ButtonSelectedScale => buttonSelectedScale;
        public float ButtonNormalPixelPerUnit => buttonNormalPixelPerUnit;
        public float ButtonSelectedPixelPerUnit => buttonSelectedPixelPerUnit;
        public Vector2 IconNormalPos => iconNormalPos;
        public Vector2 IconSelectedPos => iconSelectedPos;
        public Vector2 TextNormalPos => textNormalPos;
        public Vector2 TextSelectedPos => textSelectedPos;

        [SerializeField] protected Color normalColor;

        public Color NormalColor
        {
            get => normalColor;
            set => normalColor = value;
        }

        public Color SelectedColor
        {
            get => selectedColor;
            set => selectedColor = value;
        }

        [SerializeField] protected Color selectedColor;
        
        [SerializeField] protected Vector2 iconNormalPos;
        [SerializeField] protected Vector2 iconSelectedPos;
        
        [SerializeField] protected Vector2 textNormalPos;
        [SerializeField] protected Vector2 textSelectedPos;
        
        [FormerlySerializedAs("animationCurve")]
        [SerializeField] protected AnimationCurve smoothingCurve;
        [SerializeField] protected float movementDuration;
        
        [HideInInspector] public SelectorUIButton[] selectorUIButtons;
        [HideInInspector] public Selector selector;

        protected int prevSelectedState;
        protected bool persistenceLoaded; // Currently only used by language selector UI to apply default language if it hasn't
        
        protected VRCPlayerApi localPlayerApi;
        protected int defaultState;
        
        protected virtual void Start()
        {
            defaultState = selectedState;
            localPlayerApi = Networking.LocalPlayer;
            
            for (int i = 0; i < selectorUIButtons.Length; i++)
            {
                if (!selectorUIButtons[i].overrideDefaults) selectorUIButtons[i]._SetDefaults(
                    buttonNormalScale, 
                    buttonSelectedScale, 
                    buttonNormalPixelPerUnit, 
                    buttonSelectedPixelPerUnit, 
                    normalColor, 
                    selectedColor, 
                    iconNormalPos, 
                    iconSelectedPos, 
                    textNormalPos, 
                    textSelectedPos, 
                    smoothingCurve, 
                    movementDuration);
                selectorUIButtons[i]._Setup(this, i);
            }            
            
            if (!selector.overrideDefaults) selector._SetDefaults(smoothingCurve, movementDuration);
            selector._Setup(this);

            
            UpdateSelection(selectedState, true, true, true);
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;

            int storedSelected = 0;
            if (PlayerData.TryGetBool(player, dataKey, out bool boolValue))
            {
                storedSelected = boolValue ? 1 : 0;
                persistenceLoaded = true;
            }
            else if (PlayerData.TryGetInt(player, dataKey, out int intValue))
            {
                storedSelected = intValue;
                persistenceLoaded = true;
            }

            if (!persistenceLoaded) return;
            if (!selectorUIButtons[storedSelected].Selectable)
            {
                PlayerData.SetInt(dataKey, selectedState);
                return;
            }
            
            UpdateSelection(storedSelected, true, false, false);
        }

        protected virtual void ChangeExternalSelection(int id, bool skipBehaviour)
        {
            if (itemObjs != null && itemObjs.Length != 0)
            {
                foreach (var item in itemObjs)
                {
                    if (item == null) continue;
                    item.SetActive(false);
                }
                if (itemObjs.Length - 1 >= id && itemObjs[id] != null) itemObjs[id].SetActive(true);
            }
            
            if (animators != null && animators.Length > 0)
            {
                foreach (var animator in animators)
                {
                    if (animator == null) continue;
                    animator.SetInteger(animParamName, id);
                }
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

        /// <summary>
        /// Sets the state of the Selector UI, acts the same way as if it was done through the UI.
        /// </summary>
        /// <param name="buttonId">State that should be selected.</param>
        /// <returns>True if successful, false if not.</returns>
        public bool _ButtonSelected(int buttonId)
        {
            if (buttonId < 0 || buttonId >= selectorUIButtons.Length)  return false; 
            if (secondClick && buttonId == selectedState)
            {
                if (buttonId == secondClickState) return false;
                return UpdateSelection(secondClickState, false, false, false);
            }
            else
            {
                return UpdateSelection(buttonId, false, false, false);
            }
        }

        /// <summary>
        /// Forces updates the Selector to update the position, size, corner radius to the correct button.
        /// </summary>
        public void _ForceUpdateSelector()
        {
            selector._MoveTo(selectorUIButtons[selectedState]);
        }

        protected virtual bool UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (newState == selectedState && !skipSameCheck) return false;
            prevSelectedState = selectedState;
            selectedState = newState;

            ChangeExternalSelection(selectedState, false);

            if (usePersistence && !skipPersistence)
            {
                if (selectorUIButtons.Length == 2)
                {
                    PlayerData.SetBool(dataKey, selectedState == 1); // Save as bool for toggles to save space
                }
                else
                {
                    PlayerData.SetInt(dataKey, selectedState);
                }
            }
            
            // Button Transition
            selectorUIButtons[prevSelectedState]._UpdateSelected(false);
            selectorUIButtons[selectedState]._UpdateSelected(true);
            
            // UI Selector Animation
            selector._MoveTo(selectorUIButtons[selectedState]);
            return true;
        }

    }
}
