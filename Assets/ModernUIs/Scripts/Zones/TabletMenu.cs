
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TabletMenu : UdonSharpBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private KeyCode openKey = KeyCode.E;
        [Header("Transform")]
        [SerializeField] private Transform menuTransform;
        [SerializeField] private Vector3 headOffset = new Vector3(0f, 0f, 2f);
        [Header("Connection")]
        [SerializeField] private ObjectMover objectMover;
        [SerializeField] private int zoneId;
        [SerializeField] private GameObject[] tabletGameObjects;
        [Header("Animation")]
        [SerializeField] private AnimationCurve smoothingCurve;
        [SerializeField] private float movementDuration = 0.5f;

        private bool animate;
        private float animElpasedTime;
        private Vector3 newScale;
        private Vector3 prevScale;

        private bool isOpen;

        private Vector3 openScale;
        private VRCPlayerApi localPlayer;

        private void OnDisable()
        {
            isOpen = false;
            animate = false;
            animElpasedTime = 0f;
            
            menuTransform.localScale = Vector3.zero;
            objectMover.UnforceZone();
            UpdateExternalConnection(false);
        }

        private void Start()
        {
            openScale = menuTransform.localScale;
            localPlayer = Networking.LocalPlayer;

            menuTransform.localScale = Vector3.zero;
        }

        private void Update()
        {
            CheckInput();
        }

        private void CheckInput()
        {
            //Desktop
            if (Input.GetKeyDown(openKey))
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
        }

        public void _OpenMenu()
        {
            if (isOpen) return;
            isOpen = true;
            
            var headData = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            menuTransform.position = headData.position + headData.rotation * headOffset;
            menuTransform.rotation = headData.rotation;
            transform.position = headData.position;
            
            AnimateScale(true);
            
            objectMover.ForceZone(zoneId);
            UpdateExternalConnection(true);
        }

        public void _CloseMenu()
        {
            if (!isOpen) return;
            isOpen = false;
            
            AnimateScale(false);
        }

        private void UpdateExternalConnection(bool state)
        {
            if (tabletGameObjects != null && tabletGameObjects.Length != 0)
            {
                foreach (var item in tabletGameObjects)
                {
                    if (item == null) continue;
                    item.SetActive(state);
                }
            }
        }

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
        
        public void _StartAnimation()
        {
            animate = true;
            animElpasedTime = 0f;
            _CustomUpdate();
        }
        
        public void _CustomUpdate()
        {
            if (!animate) return;
            Animate();
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 1);
        }

        private void Animate()
        {
            animElpasedTime += Time.deltaTime;
            var percentageComplete = animElpasedTime / movementDuration;
            var smoothPercentageComplete = smoothingCurve.Evaluate(percentageComplete);

            var scale = Vector3.LerpUnclamped(prevScale, newScale, smoothPercentageComplete);
            menuTransform.localScale = Vector3.Max(scale, Vector3.zero);
            
            if (percentageComplete >= 1f)
            {
                animElpasedTime = 0f;
                animate = false;

                if (!isOpen)
                {
                    objectMover.UnforceZone();
                    UpdateExternalConnection(false);
                }
            }
        }
        #endregion
    }
}