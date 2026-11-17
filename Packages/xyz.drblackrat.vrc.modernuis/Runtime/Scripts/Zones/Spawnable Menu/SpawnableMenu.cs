
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SpawnableMenu : UdonSharpBehaviour
    {
        #region Variables
        // Desktop Open Method
        [SerializeField]
        [Tooltip("Method used for opening the spawnable menu in Desktop.")]
        private KeyCode openKey = KeyCode.E;
        public KeyCode OpenKey => openKey;
        
        // VR Open Method
        [SerializeField] 
        private VRMenuOpenMethod vrOpenMethod = VRMenuOpenMethod.DownAndUp;
        public VRMenuOpenMethod VROpenMethod => vrOpenMethod;
        [SerializeField]
        [Range(0, 5)] 
        [Tooltip("Only used if VR Open Method is Hold Up or Hold Down.\nTime for how long you have to hold the button.")]
        private float vrHoldTime = 1f;
        public float VRHoldTime => vrHoldTime;
        [SerializeField]
        [Tooltip("How much the stick has to be moved to be considered pressing it.")]
        private float vrStickThreshold = 0.8f;
        
        // Mobile Open Method
        [SerializeField] 
        [Tooltip("UI that is enabled if a player is on a touch device.")]
        private GameObject mobileUI;
        public GameObject MobileUI => mobileUI;
        
        // Settings
        [SerializeField] 
        [Tooltip("Transform that should be positioned in front of the player using the offset.")]
        private Transform menuTransform;
        [SerializeField] 
        [Tooltip("Offset from the players camera used to position the menu.")]
        private Vector3 headOffset = new Vector3(0f, 0f, 1f);
        
        [SerializeField] 
        [Tooltip("Object Mover used to force the menu to the provided transform.")]
        private ObjectMover objectMover;
        [SerializeField] 
        [Tooltip("Id / Index of the zone the transform is attached on the Object Mover.")]
        private int zoneId;
        
        [SerializeField] 
        private AnimationCurve smoothingCurve;
        [SerializeField] 
        private float movementDuration = 0.5f;

        [SerializeField]
        [Tooltip("If enabled, the menu will automatically adjust it's scale to match the player's eye height.")]
        private bool autoAdjustScale = true;
        [SerializeField] 
        [Tooltip("Height of the player at which the menu will have it's normal scale. If a player's eye height differs from this value, the menu will be rescaled.")]
        private float referenceHeight = 1.7f;
        [SerializeField]
        [Tooltip("Transform of the distance disable trigger to also scale with the menu.")]
        private Transform distanceDisable;
        
        // Private Variables
        private bool _isOpen;
        
        private bool _vrIsHolding;
        private bool _vrHoldingOvertime;
        private float _vrHoldElapsed;
        
        private bool _vrStickWasDown;
        private bool _vrStickDownOverTime;
        private float _vrStickDownElapsed;
        private const float VrStickDownResetTime = 0.5f;
        
        private Vector3 _openScale;
        private Vector3 _distanceDisableScale;
        
        private VRCTweenHandle _menuScaleTween;
        
        private VRCPlayerApi _localPlayer;
        #endregion

        #region Unity & VRChat Events
        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            
            _openScale = menuTransform.localScale;
            menuTransform.localScale = Vector3.zero;

            if (distanceDisable != null)
            {
                _distanceDisableScale = distanceDisable.localScale;
            }

            if (mobileUI != null)
            {
                mobileUI.SetActive(InputManager.GetLastUsedInputMethod() == VRCInputMethod.Touch);
            }
        }

        private void OnDisable()
        {
            _isOpen = false;

            _menuScaleTween.Kill();
            menuTransform.localScale = Vector3.zero;
            objectMover.UnforceZone();
            
            _vrStickWasDown = false;
            _vrStickDownOverTime = false;
            _vrStickDownElapsed = 0f;

            _vrIsHolding = false;
            _vrHoldingOvertime = false;
            _vrHoldElapsed = 0f;
        }
        
        private void Update()
        {
            CheckInput();
        }
        #endregion

        #region Input Handling
        private void CheckInput()
        {
            // Desktop
            if (Input.GetKeyDown(openKey))
                _ToggleMenu();
            
            // VR
            switch (vrOpenMethod)
            {
                case VRMenuOpenMethod.HoldUp:
                case VRMenuOpenMethod.HoldDown:
                    VRHoldUpdateCheck();
                    break;
                case VRMenuOpenMethod.DownAndUp:
                    VRDownUpUpdateCheck();
                    break;
            }
        }

        private void VRHoldUpdateCheck()
        {
            if (!_vrIsHolding || _vrHoldingOvertime) return;
            _vrHoldElapsed += Time.deltaTime;

            if (_vrHoldElapsed < vrHoldTime) return;
            _ToggleMenu();
            _vrHoldingOvertime = true;
        }

        private void VRDownUpUpdateCheck()
        {
            if (!_vrStickWasDown || _vrStickDownOverTime) return;
            _vrStickDownElapsed += Time.deltaTime;

            if (_vrStickDownElapsed < VrStickDownResetTime) return;
            _vrStickDownOverTime = true;
        }

        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!isActiveAndEnabled ||
                !_localPlayer.IsUserInVR())
            {
                return;
            }
            
            switch (vrOpenMethod)
            {
                case VRMenuOpenMethod.HoldUp:
                    _vrIsHolding = value >= vrStickThreshold;
                    break;

                case VRMenuOpenMethod.HoldDown:
                    _vrIsHolding = value <= -vrStickThreshold;
                    break;

                case VRMenuOpenMethod.DownAndUp:
                    if (!_vrStickWasDown)
                    {
                        _vrStickWasDown = value <= -vrStickThreshold;
                    }
                    
                    if (_vrStickDownOverTime && value >= 0)
                    {
                        _vrStickDownElapsed = 0f;
                        _vrStickDownOverTime = false;
                        _vrStickWasDown = false;
                    }

                    if (_vrStickWasDown && !_vrStickDownOverTime && value >= vrStickThreshold)
                    {
                        _vrStickWasDown = false;
                        _vrStickDownElapsed = 0f;
                        _ToggleMenu();
                    }
                    break;
            }
            
            if (!_vrIsHolding)
            {
                _vrHoldElapsed = 0f;
                _vrHoldingOvertime = false;
            }
        }
        #endregion

        #region Toggle / Open / Close Menu
        /// <summary>
        /// Toggles the spawnable menu.
        /// </summary>
        public void _ToggleMenu()
        {
            if (_isOpen)
            {
                _CloseMenu();
            }
            else
            {
                _OpenMenu();
            }
        }

        /// <summary>
        /// Opens the spawnable menu if currently closed.
        /// </summary>
        public void _OpenMenu()
        {
            if (!isActiveAndEnabled || 
                _isOpen)
            {
                return;
            }
            _isOpen = true;

            var screenCamera = VRCCameraSettings.ScreenCamera;
            float sizeRatio =  autoAdjustScale ? _localPlayer.GetAvatarEyeHeightAsMeters() / referenceHeight : 1;

            menuTransform.position = screenCamera.Position + screenCamera.Rotation * headOffset * sizeRatio;
            menuTransform.rotation = screenCamera.Rotation;
            transform.position = screenCamera.Position;

            if (distanceDisable != null)
            {
                distanceDisable.position = _localPlayer.GetPosition();
                distanceDisable.localScale = _distanceDisableScale * sizeRatio;
            }

            objectMover.ForceZone(zoneId);
            
            _menuScaleTween.Kill();
            _menuScaleTween = menuTransform.TweenScale(_openScale * sizeRatio, movementDuration, VRCTweenEase.None)
                .SetEase(smoothingCurve);
        }

        /// <summary>
        /// Closes the spawnable menu if currently open.
        /// </summary>
        public void _CloseMenu()
        {
            if (!isActiveAndEnabled || 
                !_isOpen)
            {
                return;
            }
            _isOpen = false;

            _menuScaleTween.Kill();
            _menuScaleTween = menuTransform.TweenScale(Vector3.zero, movementDuration, VRCTweenEase.None)
                .SetEase(smoothingCurve)
                .OnComplete(objectMover, nameof(objectMover.UnforceZone));
        }
        #endregion
    }
    
    public enum VRMenuOpenMethod
    {
        HoldUp,
        HoldDown,
        DownAndUp,
    }
}