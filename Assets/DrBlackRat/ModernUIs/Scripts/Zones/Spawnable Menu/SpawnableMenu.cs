
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SpawnableMenu : UdonSharpBehaviour
    {
        [Tooltip("Method used for opening the spawnable menu in Desktop.")]
        [SerializeField] private KeyCode openKey = KeyCode.E;
        public KeyCode OpenKey => openKey;
        
        [SerializeField] private VRMenuOpenMethod vrOpenMethod = VRMenuOpenMethod.DownAndUp;
        [Range(0, 5)] [Tooltip("Only used if VR Open Method is Hold Up or Hold Down.\nTime for how long you have to hold the button.")]
        [SerializeField] private float vrHoldTime = 1f;
        [Tooltip("How much the stick has to be moved to be considered pressing it.")]
        [SerializeField] private float vrStickThreshold = 0.8f;
        
        public VRMenuOpenMethod VROpenMethod => vrOpenMethod;
        public float VRHoldTime => vrHoldTime;
        public GameObject MobileUI => mobileUI;

        [Tooltip("UI that is enabled if a player is on a touch device.")]
        [SerializeField] private GameObject mobileUI;
        
        [Tooltip("Transform that should be positioned in front of the player using the offset.")]
        [SerializeField] private Transform menuTransform;
        [Tooltip("Offset from the players camera used to position the menu.")]
        [SerializeField] private Vector3 headOffset = new Vector3(0f, 0f, 1f);
        
        [Tooltip("Object Mover used to force the menu to the provided transform.")]
        [SerializeField] private ObjectMover objectMover;
        [Tooltip("Id / Index of the zone the transform is attached on the Object Mover.")]
        [SerializeField] private int zoneId;
        
        [SerializeField] private AnimationCurve smoothingCurve;
        [SerializeField] private float movementDuration = 0.5f;

        private bool animate;
        private float animElapsedTime;
        private Vector3 newScale;
        private Vector3 prevScale;

        private bool isOpen;

        // VR Hold Mode
        private bool vrIsHolding;
        private bool vrHoldingOvertime;
        private float vrHoldElapsed;
        
        // VR Up Down Mode
        private bool vrStickWasDown;
        private bool vrStickDownOverTime;
        private float vrStickDownElapsed;
        private const float VrStickDownResetTime = 0.5f;
        
        private Vector3 openScale;
        private VRCPlayerApi localPlayer;

        #region Unity & VRChat Events
        private void OnDisable()
        {
            isOpen = false;
            animate = false;
            animElapsedTime = 0f;

            menuTransform.localScale = Vector3.zero;
            objectMover.UnforceZone();
            
            // VR Reset
            vrStickWasDown = false;
            vrStickDownOverTime = false;
            vrStickDownElapsed = 0f;

            vrIsHolding = false;
            vrHoldingOvertime = false;
            vrHoldElapsed = 0f;
        }

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            
            openScale = menuTransform.localScale;
            menuTransform.localScale = Vector3.zero;
            
            if (mobileUI != null)
                mobileUI.SetActive(InputManager.GetLastUsedInputMethod() == VRCInputMethod.Touch);
        }

        private void Update()
        {
            CheckInput();
        }

        private bool IsEnabled() => enabled && gameObject.activeInHierarchy;
        #endregion

        #region Input Handling
        // ReSharper disable Unity.PerformanceAnalysis
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
            if (!vrIsHolding || vrHoldingOvertime) return;
            vrHoldElapsed += Time.deltaTime;

            if (vrHoldElapsed < vrHoldTime) return;
            _ToggleMenu();
            vrHoldingOvertime = true;
        }

        private void VRDownUpUpdateCheck()
        {
            if (!vrStickWasDown || vrStickDownOverTime) return;
            vrStickDownElapsed += Time.deltaTime;

            if (vrStickDownElapsed < VrStickDownResetTime) return;
            vrStickDownOverTime = true;
        }

        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!IsEnabled()) return;
            if (!localPlayer.IsUserInVR())
                return;
            
            switch (vrOpenMethod)
            {
                case VRMenuOpenMethod.HoldUp:
                    vrIsHolding = value >= vrStickThreshold;
                    break;

                case VRMenuOpenMethod.HoldDown:
                    vrIsHolding = value <= -vrStickThreshold;
                    break;

                case VRMenuOpenMethod.DownAndUp:
                    if (!vrStickWasDown)
                    {
                        vrStickWasDown = value <= -vrStickThreshold;
                    }
                    
                    if (vrStickDownOverTime && value >= 0)
                    {
                        vrStickDownElapsed = 0f;
                        vrStickDownOverTime = false;
                        vrStickWasDown = false;
                    }

                    if (vrStickWasDown && !vrStickDownOverTime && value >= vrStickThreshold)
                    {
                        vrStickWasDown = false;
                        vrStickDownElapsed = 0f;
                        _ToggleMenu();
                    }
                    break;
            }
            
            if (!vrIsHolding)
            {
                vrHoldElapsed = 0f;
                vrHoldingOvertime = false;
            }
        }
        #endregion

        #region Toggle / Open / Close Menu
        /// <summary>
        /// Toggles the spawnable menu.
        /// </summary>
        public void _ToggleMenu()
        {
            if (isOpen)
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
            if (!IsEnabled()) return;
            if (isOpen) return;
            isOpen = true;

            var headData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            menuTransform.position = headData.position + headData.rotation * headOffset;
            menuTransform.rotation = headData.rotation;
            transform.position = headData.position;

            AnimateScale(true);
            objectMover.ForceZone(zoneId);
        }

        /// <summary>
        /// Closed the spawnable menu if currently open.
        /// </summary>
        public void _CloseMenu()
        {
            if (!IsEnabled()) return;
            if (!isOpen) return;
            isOpen = false;

            AnimateScale(false);
        }
        #endregion

        #region Animation
        private void AnimateScale(bool open)
        {
            if (open)
            {
                newScale = openScale;
                prevScale = menuTransform.localScale;
            }
            else
            {
                newScale = Vector3.zero;
                prevScale = menuTransform.localScale;
            }
            animate = false;
            SendCustomEventDelayedFrames(nameof(_StartAnimation), 1);
        }
        
        /// <summary>
        /// Internal Event | DO NOT CALL MANUALLY
        /// </summary>
        public void _StartAnimation()
        {
            animate = true;
            animElapsedTime = 0f;
            _CustomUpdate();
        }
        
        /// <summary>
        /// Internal Event | DO NOT CALL MANUALLY
        /// </summary>
        public void _CustomUpdate()
        {
            if (!animate) return;
            Animate();
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 1);
        }

        private void Animate()
        {
            animElapsedTime += Time.deltaTime;
            var percentageComplete = animElapsedTime / movementDuration;
            var smoothPercentageComplete = smoothingCurve.Evaluate(percentageComplete);

            var scale = Vector3.LerpUnclamped(prevScale, newScale, smoothPercentageComplete);
            menuTransform.localScale = Vector3.Max(scale, Vector3.zero);
            
            if (percentageComplete >= 1f)
            {
                animElapsedTime = 0f;
                animate = false;

                if (!isOpen)
                {
                    objectMover.UnforceZone();
                }
            }
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