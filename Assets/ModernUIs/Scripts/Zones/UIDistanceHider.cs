using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using VRC.SDK3.Components;

namespace DrBlackRat.VRC.ModernUIs
{
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIDistanceHider : UdonSharpBehaviour
    {
        // Canvases
        [SerializeField] 
        [FormerlySerializedAs("canvasObj"), FormerlySerializedAs("canvasObjs")]
        [Tooltip("Canvases that will have their collider disabled when hidden. Disables being able to click on them.")]
        private Canvas[] canvases;
        
        [SerializeField] 
        [FormerlySerializedAs("hidingCanvases"), FormerlySerializedAs("canvasesToHide")]
        [Tooltip("Canvas Groups that will have their alpha set to 0 when hidden. Used for hiding the UI Elements")]
        private CanvasGroup[] canvasGroupsToHide;
        
        [SerializeField] 
        [FormerlySerializedAs("infoCanvas"), FormerlySerializedAs("canvasesToShow")]
        [Tooltip("Canvas Groups that will have their alpha set to 1 when hidden. Useful for info messages about it being distance hidden.")]
        private CanvasGroup[] canvasGroupsToShow;

        [SerializeField]
        [Tooltip("If enabled, the Game Objects the Canvas Groups are on will also be disabled / enabled depending on if they are hidden.\nOnly enable this if you know what you are doing! This might break syncing, etc.")]
        private bool alsoDisableCanvasGroupObjects;
        
        // GameObjects
        [SerializeField]
        [Tooltip("GameObjects that will be disabled when hidden.")]
        private GameObject[] objectsToDisable;
        
        [SerializeField] 
        [Tooltip("GameObjects that will be enabled when hidden.")]
        private GameObject[] objectsToEnable;
        
        // Animation
        [SerializeField] 
        [FormerlySerializedAs("smoothingCurve")] 
        private AnimationCurve transitionCurve;
        
        [SerializeField] 
        [FormerlySerializedAs("movementDuration")] 
        private float transitionDuration;

        private VRCTweenHandle[] hideTweenHandles;
        private VRCTweenHandle[] showTweenHandles;
        private VRCTweenHandle finishEventHandle;
        
        private Collider[] canvasColliders;
        private Collider zoneCollider;

        private void Start()
        {
            zoneCollider = GetComponent<Collider>(); // Only needed for backwards compatibility, old setups won't grab it in reset.
            canvasColliders = new Collider[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                if (!canvases[i]) continue;
                canvasColliders[i] = canvases[i].GetComponent<Collider>();
            }

            SetupTween();
            SetAllObjectsHidden(true);
            SetAllCanvasGroupsHidden(true);
        }

        private void SetupTween()
        {
            showTweenHandles = new VRCTweenHandle[canvasGroupsToShow.Length];
            hideTweenHandles = new VRCTweenHandle[canvasGroupsToHide.Length];

            for (int i = 0; i < showTweenHandles.Length; i++)
            {
                if (!canvasGroupsToShow[i]) continue;
                showTweenHandles[i] = canvasGroupsToShow[i].TweenFade(0f, transitionDuration, VRCTweenEase.None)
                    .SetEase(transitionCurve)
                    .Pause();
            }

            for (int j = 0; j < hideTweenHandles.Length; j++)
            {
                if (!canvasGroupsToHide[j]) continue;
                hideTweenHandles[j] = canvasGroupsToHide[j].TweenFade(0f, transitionDuration, VRCTweenEase.None)
                    .SetEase(transitionCurve)
                    .Pause();
            }
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            gameObject.layer = 0;
        }
#endif
        
        #region Trigger Events & Fixes
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            SetAndAnimateHidden(false);
        }
        
        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            SetAndAnimateHidden(true);
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
        #endregion

        private void SetAndAnimateHidden(bool isHidden)
        {
            SetAllObjectsHidden(isHidden);
            AnimateAllCanvasGroupsHidden(isHidden);
        }
        
        #region Set State And Alpha
        private void SetAllObjectsHidden(bool isHidden)
        {
            foreach (var disableObj in objectsToDisable)
            {
                if (!disableObj) continue;
                disableObj.SetActive(!isHidden);
            }
            foreach (var enableObj in objectsToEnable)
            {
                if (!enableObj) continue;
                enableObj.SetActive(isHidden);
            }

            foreach (var canvasCollider in canvasColliders)
            {
                if (!canvasCollider) continue;
                canvasCollider.enabled = !isHidden;
            }
        }

        private void SetAllCanvasGroupsHidden(bool isHidden)
        {
            SetCanvasGroupHidden(canvasGroupsToHide, isHidden);
            SetCanvasGroupHidden(canvasGroupsToShow, !isHidden);
        }
        
        private void SetCanvasGroupHidden(CanvasGroup[] targets, bool isHidden)
        {
            var alpha = isHidden ? 0f : 1f;
            foreach (var canvasGroup in targets)
            {
                if (!canvasGroup) continue;

                if (alsoDisableCanvasGroupObjects)
                    canvasGroup.gameObject.SetActive(!isHidden);

                canvasGroup.alpha = alpha;
            }
        }
        #endregion
        
        #region Animate Alpha
        private void AnimateAllCanvasGroupsHidden(bool isHidden)
        {
            AnimateCanvasGroupHidden(hideTweenHandles, canvasGroupsToHide, isHidden);
            AnimateCanvasGroupHidden(showTweenHandles, canvasGroupsToShow, !isHidden);
            
            finishEventHandle.Kill();
            finishEventHandle = VRCTween.DelayedCall(this, isHidden ? nameof(_FinishHideTransition) : nameof(_FinishShowTransition), transitionDuration);
        }
        
        private void AnimateCanvasGroupHidden(VRCTweenHandle[] handles, CanvasGroup[] canvasGroups, bool isHidden)
        {
            var alpha = isHidden ? 0f : 1f;
            for (int i = 0; i < canvasGroups.Length; i++)
            {
                if (!canvasGroups[i]) 
                    continue;
                
                if (alsoDisableCanvasGroupObjects)
                    canvasGroups[i].gameObject.SetActive(true);
                
                handles[i].ChangeEndValue(alpha, true)
                    .Restart();
            }
        }
        
        public void _FinishHideTransition()
        {
            SetAllCanvasGroupsHidden(true);
        }
        
        public void _FinishShowTransition()
        {
            SetAllCanvasGroupsHidden(false);
        }
        #endregion
    }
}
