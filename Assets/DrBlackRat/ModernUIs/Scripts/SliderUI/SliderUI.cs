using System;
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
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using DrBlackRat.VRC.SecureNetworking;
using VRC.SDK3.Data;

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SliderUI : SecureNetworkBehaviour
    {
        #region Veriables
        // Settings
        [SerializeField][FormerlySerializedAs("value")]
        [Tooltip("The default value the slider should be set to.")]
        private float defaultValue = 0f;
        [SerializeField]
        [Range(0.001f, 1f)]
        [Tooltip("Interval the slider would snap to, for example 0.2 means it will snap to 0, 0.2, 0.4 etc.")]
        protected float snapInterval = 0.001f;

        [SerializeField] 
        [Tooltip("If enabled, the curve below will be used to evaluate the sliders value.")]
        private bool useCurve;
        [SerializeField] 
        [Tooltip("Changes how the sliders value should be interpreted. Requires \"Use Curve\" to be enabled.")]
        private AnimationCurve sliderCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [SerializeField][FormerlySerializedAs("usePersistence"), FormerlySerializedAs("enablePersistence")]
        [Tooltip("Enables persistent storage of the slider value. Requires a \"Date Key\" to be set below!")]
        private bool saveToPersistence;
        [SerializeField][FormerlySerializedAs("dataKey")]
        [Tooltip("Data Key that will be used to save / load persistence, everything should have a different Data Key.")]
        private string persistenceDataKey = "CHANGE THIS";
        [SerializeField] [FormerlySerializedAs("loadDelay")]
        [Range(0f, 30f)] 
        [Tooltip("Persistence load delay, useful if you need to wait for a whitelist to finish loading.")]
        private float persistenceLoadDelay = 5f;
        
        [SerializeField] [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist to control who is allowed to access this whitelist. If left empty, the whitelist wont be used.")]
        private WhitelistGetterBase whitelist;
        
        [SerializeField]
        [Tooltip("The Secure Networking Instance that will be used to send the data to other players. If left empty this slider wont be networked.")]
        private SecureNetworkingInstance secureNetworkingInstance;

        // External Connections
        [SerializeField] [FormerlySerializedAs("postProcessVolume")]
        [Tooltip("Post Process Volumes of which the weight will should be set.")]
        private PostProcessVolume[] postProcessVolumes;
        [SerializeField] [FormerlySerializedAs("audioSource")]
        [Tooltip("Audio Source of which the volume should be set.")]
        private AudioSource[] audioSources;
        
        [SerializeField] 
        [Tooltip("Animators that should have a float with the given parameter name set.")]
        private Animator[] animators;
        [SerializeField] 
        [Tooltip("Name of the float parameters that should be set.")]
        private string animParamName = "sliderValue";
        
        [SerializeField] 
        [Tooltip("Udon Behaviours that should be updated when ever the slider value changes.")]
        private UdonBehaviour[] externalBehaviours;
        [SerializeField] 
        [Tooltip("Event that will be called on the External Behaviours if the value changes.")]
        private string updateEventName = "_SliderUpdated";
        [SerializeField] 
        [Tooltip("Name of the variable that will be updated on the External Behaviours if the value changes.")]
        private string variableName = "sliderValue";
        
        // UI References
        [SerializeField]
        [Tooltip("Actual Unity UI Slider that will be used to control the value.")]
        private Slider slider;
        [SerializeField] 
        [Tooltip("Text that the slider value should be displayed at. Formated as whole numbers from 0 - 100, so 0.5 would be 50. Can be left empty if not needed.")]
        private TextMeshProUGUI sliderText;
        [SerializeField] 
        [Tooltip("Changes what the slider number will be multiplied by for display.\n- 1 would keep 0.5 as 0.5\n- 100 would turn 0.5 to 50")]
        private float sliderDisplayMultiplier = 100f;
        [SerializeField] 
        [Tooltip("What the slider value will be formated as.\n- 0 means it will always be formated to show no decimal points\n- 0.0 means it will fill always be formated with one decimal point")]
        private string sliderDisplayFormat = "0";
        [SerializeField] 
        [Tooltip("Text that would be placed at the end of the slider display value, useful for things like \"%\" or \"°\".")]
        private string sliderDisplaySuffix;
        
        // Players
        private VRCPlayerApi _localPlayer;
        private bool _isLocalPlayerAllowed;
        
        // Lock Button
        [HideInInspector] public int LockButtonIdentifier;
        [HideInInspector] public VRCPlayerApi LockingPlayer;
        [HideInInspector] public bool IsLocked;
        
        // Const / Readonly
        private readonly DataToken _netValueToken = new DataToken("value");
        
        // Other
        private bool _hasWhitelist;
        private bool _isNetworked;
        
        private float _currentValue;
        #endregion

        #region VRChat & Unity Events
        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            
            _hasWhitelist = whitelist != null;
            _isNetworked = secureNetworkingInstance != null;
            
            SetSliderValue(defaultValue);
            SetExternal(defaultValue);

            if (_isNetworked)
            {
                secureNetworkingInstance._Connect(this);
            }
            
            if (_hasWhitelist)
            {
                whitelist._SetUpConnection(this, nameof(_OnWhitelistUpdated));
            }
            else
            {
                ValidatePermissions();
            }
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || 
                !saveToPersistence)
            {
                return;
            }
            SendCustomEventDelayedSeconds(nameof(_OnPlayerRestoredDelayed), persistenceLoadDelay);
        }

        public void _OnPlayerRestoredDelayed()
        {
            if (!_isLocalPlayerAllowed 
                || !_localPlayer.isMaster) // We would only be the master here if we were the first player who joined
            {
                return;
            }

            if (!PlayerData.TryGetFloat(_localPlayer, persistenceDataKey, out float value))
            {
                MUIDebug.LogError($"Failed to load slider value from persistence on {gameObject.name}!", gameObject);
                return;
            }
            
            SetSliderValue(value);
            SetExternal(value);
            TrySendNet();
        }
        
        #endregion

        #region Public API
        /// <summary>
        /// Sets the slider value, only does so if the local player is allowed to access the slider.
        /// Use <see cref="_SetValueForced"/> to force the set and ignore the local player's access state.
        /// </summary>
        /// <param name="newValue">Value the slider should be set to.</param>
        /// <returns></returns>
        public bool _TrySetValue(float newValue)
        {
            if (!_isLocalPlayerAllowed)
            {
                return false;
            }
            _SetValueForced(newValue);
            return true;
        }

        /// <summary>
        /// Sets the slider value, ignores if the local player is not allowed to access the slider.
        /// Use <see cref="_SetValue"/> to not ignore the local player's access state.
        /// </summary>
        /// <param name="newValue">Value the slider should be set to.</param>
        public void _SetValueForced(float newValue)
        {
            SetSliderValue(newValue);
            SetExternal(newValue);
        }

        /// <summary>
        /// Resets the slider to the default value, only does so if the local player is allowed to access the slider.
        /// Use <see cref="_ResetForced"/> to force the reset and ignore the local player's access state.
        /// </summary>
        /// <returns>True if the local player was allowed to set the slider value, false if not.</returns>
        public bool _TryReset()
        {
            if (!_isLocalPlayerAllowed)
            {
                return false;
            }
            _ResetForced();
            return true;
        }

        /// <summary>
        /// Resets the slider to the default value, ignores if the local player is not allowed to access the slider.
        /// Use <see cref="_Reset"/> to not ignore the local player's access state.
        /// </summary>
        public void _ResetForced()
        {
            _SetValueForced(defaultValue);
        }

        /// <summary>
        /// Saves the current slider value to persistence if persistence is enabled.
        /// </summary>
        /// <returns>True if successful, false if not.</returns>
        public bool TrySaveToPersistence()
        {
            if (!saveToPersistence)
            {
                return false;
            }
            
            PlayerData.SetFloat(persistenceDataKey, _currentValue);
            return true;
        }

        /// <summary>
        /// Tries to send the current slider value over the network if the local player is allowed to, and the slider is networked.
        /// </summary>
        /// <returns>True if successful, false if not.</returns>
        public bool TrySendOverNetwork()
        {
            if (!_isLocalPlayerAllowed)
            {
                return false;
            }
            
            return TrySendNet();
        }
        #endregion

        #region Permissions, Locking & Whitelist
        /// <summary>
        /// Called by the Whitelist when ever it got updated.
        /// </summary>
        public void _OnWhitelistUpdated()
        {
            ValidatePermissions();
        }
        
        /// <summary>
        /// Called by the Lock Button when ever it got updated.
        /// </summary>
        public void _OnLockStateChanged()
        {
            ValidatePermissions();
        }
        
        private bool IsPlayerAllowed(VRCPlayerApi player)
        {
            if (IsLocked && LockingPlayer != player)
            {
                return false;
            }
            if (_hasWhitelist && !whitelist._IsPlayerWhitelisted(player))
            {
                return false;
            }
            return true;
        }

        private void ValidatePermissions()
        {
            _isLocalPlayerAllowed = IsPlayerAllowed(_localPlayer);
            slider.interactable = _isLocalPlayerAllowed;

            if (_isNetworked)
            {
                secureNetworkingInstance._ValidateAllowedSenders();
            }
        }
        #endregion


        #region Slider
        /// <summary>
        /// Called by the slider whenever its value has been changed.
        /// </summary>
        public void _ValueUpdated()
        {
            if (!_isLocalPlayerAllowed)
            {
                MUIDebug.LogError("You are not allowed to change the slider value!");
                return;
            }
            
            var newValue = slider.value;
            newValue = Mathf.Round(newValue / snapInterval) * snapInterval;
            
            if (Mathf.Approximately(_currentValue, newValue))
            {
                return;
            }
            
            SetSliderValue(newValue);
            SetExternal(newValue);

            TrySaveToPersistence();
            TrySendNet();
        }
        
        private void SetSliderValue(float value)
        {
            _currentValue = value;
            slider.SetValueWithoutNotify(value);
            
            if (sliderText != null)
            {
                var temp = value * sliderDisplayMultiplier;
                sliderText.text = temp.ToString(sliderDisplayFormat) + sliderDisplaySuffix;
            } 
        }
        #endregion
        
        #region External Connections
        private float EvaluateCurve(float value)
        {
            value = (value - slider.minValue) / (slider.maxValue - slider.minValue);
            value = sliderCurve.Evaluate(value);
            return slider.minValue + (value * (slider.maxValue - slider.minValue));
        }
        
        private void SetExternal(float value)
        {
            if (useCurve)
            {
                value = EvaluateCurve(value);
            }
            
            foreach (var audioSource in audioSources)
            {
                if (audioSource == null) continue;
                audioSource.volume = value;
            }
            foreach (var postProcessVolume in postProcessVolumes)
            {
                if (postProcessVolume == null) continue;
                postProcessVolume.weight = value;
            }
            foreach (var animator in animators)
            {
                if (animator == null) continue;
                animator.SetFloat(animParamName, value);
            }
            foreach (var behaviour in externalBehaviours)
            {
                if (behaviour == null) continue;
                behaviour.SetProgramVariable(variableName, value);
                behaviour.SendCustomEvent(updateEventName);
            }
        }
        #endregion
        
        #region Networking
        /// <summary>
        /// Sends the current slider value to the network if the slider is networked.
        /// </summary>
        /// <returns>True if the slider is networked, false if not.</returns>
        private bool TrySendNet()
        {
            if (!_isNetworked)
            {
                return false;
            }
            
            secureNetworkingInstance._SendNetworkData();
            return true;
        }
        
        // Secure Networking
        public override bool _IsAllowedSender(VRCPlayerApi player)
        {
            return IsPlayerAllowed(player);
        }

        public override VRCPlayerApi _GetAllowedSender()
        {
            if (IsLocked)
            {
                return LockingPlayer;
            }
            
            if (_hasWhitelist)
            {
                return whitelist._GetPlayerInInstance();
            }
            
            return Networking.Master;
        }

        public override void _OnNetworkDataReceived(DataDictionary receivedData)
        {
            if (!receivedData.TryGetValue(_netValueToken, TokenType.Double,out DataToken value))
            {
                MUIDebug.LogError($"Failed to deserialize slider value on {gameObject.name}! Data may be malformed, skipping applying net data.", gameObject);
                return;
            }
            
            var newValue = (float)value.Double;
            SetSliderValue(newValue);
            SetExternal(newValue);
        }

        public override void _ResetNetToDefault()
        {
            _ResetForced();
        }

        public override DataDictionary _GetNetworkDataForSending()
        {
            var data = new DataDictionary();
            data.SetValue(_netValueToken, _currentValue);
            return data;
        }
        #endregion

    }
}
