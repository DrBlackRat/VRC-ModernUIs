using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIDistanceHider : UdonSharpBehaviour
    {
        [Tooltip("Canvas Group that will be hidden when Player is outside of the trigger.")]
        [SerializeField] private CanvasGroup[] hidingCanvases;
        [Tooltip("GameObjects that will be disabled when Player is outside of the trigger.")]
        [SerializeField] private GameObject[] objectsToDisable;
        [Tooltip("Canvas Group that will be shown when Player is outside of the trigger. Use full for things like a distance hidden info message.")]
        [SerializeField] private CanvasGroup infoCanvas;
        
        [SerializeField] private AnimationCurve smoothingCurve;
        [SerializeField] private float movementDuration;
        
        private bool animate;
        private float animationElapsedTime;
        
        private float prevHiddenAlpha;
        private float newHiddenAlpha;

        private float prevShownAlpha;
        private float newShownAlpha;
        
        private Collider zoneCollider;

        private bool hidden;

        private bool hasInfoCanvas;

        private void Start()
        {
            hasInfoCanvas = infoCanvas != null;
            zoneCollider = GetComponent<Collider>();
            if (zoneCollider == null)
            {
                MUIDebug.LogError("UI Distance Hider: No collider found! Add a Box or Sphere Collider set to \"is Trigger\" for this to work correctly!");
                return;
            }
            UpdateHiddenState(true);
        }
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            UpdateHiddenState(false);
        }
        
        
        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            UpdateHiddenState(true);
        }
        
        // Spawn / Respawn Fixes
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public void _EnableCollider()
        {
            zoneCollider.enabled = true;
        }


        private void UpdateHiddenState(bool newHidden)
        {
            hidden = newHidden;
            
            //GameObjects
            if (objectsToDisable != null && objectsToDisable.Length != 0)
            {
                foreach (var disableOb in objectsToDisable)
                {
                    if (disableOb == null) continue;
                    disableOb.SetActive(!hidden);
                }
            }
            
            // UI Animation
            animate = false;
            SendCustomEventDelayedFrames(nameof(_StartAnimation), 0);
        }
        
        public void _StartAnimation()
        {
            prevHiddenAlpha = newHiddenAlpha;
            newHiddenAlpha = hidden ? 0 : 1;

            prevShownAlpha = newShownAlpha;
            newShownAlpha = hidden ? 1 : 0;
            
            animate = true;
            animationElapsedTime = 0f;
            _CustomUpdate();
        }
        
        public void _CustomUpdate()
        {
            if (!animate) return;
            AnimateUI();
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        private void AnimateUI()
        {
            animationElapsedTime += Time.deltaTime;
            var percentageComplete = animationElapsedTime / movementDuration;
            UpdateCanvasAlpha(smoothingCurve.Evaluate(percentageComplete));
            if (percentageComplete >= 1f)
            {
                animationElapsedTime = 0f;
                animate = false;
            }
        }

        private void UpdateCanvasAlpha(float transition)
        {
            foreach (var hidingCanvas in hidingCanvases)
            {
                hidingCanvas.alpha = Mathf.Lerp(prevHiddenAlpha, newHiddenAlpha, transition);
            }

            if (hasInfoCanvas)
            {
                infoCanvas.alpha = Mathf.Lerp(prevShownAlpha, newShownAlpha, transition);
            }
            
            if (transition >= 1f)
            {
                foreach (var hidingCanvas in hidingCanvases)
                {
                    hidingCanvas.alpha = newHiddenAlpha;
                }

                if (hasInfoCanvas)
                {
                    infoCanvas.alpha = newShownAlpha;
                }
            }
        }
    }
}
