using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;


namespace DrBlackRat.VRC.ModernUIs.Utils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EulaScreen : UdonSharpBehaviour
    {
        [Tooltip("Object to disable if Eula has been accepted. Use full for things like a collider box.")]
        [SerializeField] private GameObject disableObject;

        [Tooltip("Object to disable, move and scale when the Eula has been accepted.")]
        [SerializeField] private GameObject eulaObject;
        [Tooltip("Transform the Eula will animate it's scale and position to.")]
        [SerializeField] private Transform hiddenTransform;
        [Tooltip("Canvas Group of which the alpha will be animated to make it slowly fade out.")]
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Tooltip("Game Object that gets enabled if the Eula has changed since it was last accepted.")]
        [SerializeField] private GameObject versionChangeInfo;
        
        [Tooltip("Turn on if this Eula should be saved using Persistence.")]
        [SerializeField] private bool usePersistence = true;
        [Tooltip("Data Key that will be used to save / load this Eula, everything using Persistence should have a different Data Key.")]
        [SerializeField] private string dataKey = "CHANGE THIS";
        [Tooltip("Current version of the Eula, if the accepted one is lower than the current a user will have to accept it again.")]
        [SerializeField] private int ruleVer;
        
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;
        
        private bool animate;
        private float movementElapsedTime;
        private Vector3 oldUiPos;
        private Vector3 oldUiScale;

        private void Start()
        {
            // Disable Changed Info if left on in editor by accident
            if (versionChangeInfo != null) versionChangeInfo.SetActive(false);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;
            if (!PlayerData.TryGetInt(player, dataKey, out int value)) return;
            if (value >= ruleVer)
            {
                _HideEula();
            }
            else
            {
                if (versionChangeInfo != null) versionChangeInfo.SetActive(true);
            }
        }
        
        public void _Accepted()
        {
            if (usePersistence) PlayerData.SetInt(dataKey, ruleVer);
            _HideEula();
        }

        private void _HideEula()
        {
            disableObject.SetActive(false);

            oldUiPos = eulaObject.transform.position;
            oldUiScale = eulaObject.transform.localScale;
            animate = true;
            movementElapsedTime = 0f; 
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
            movementElapsedTime += Time.deltaTime;
            var percentageComplete = movementElapsedTime / movementDuration;
            var smoothPosPercentageComplete = animationCurve.Evaluate(percentageComplete);
            
            eulaObject.transform.position = Vector3.LerpUnclamped(oldUiPos, hiddenTransform.position, smoothPosPercentageComplete);
            eulaObject.transform.localScale = Vector3.LerpUnclamped(oldUiScale, hiddenTransform.localScale, smoothPosPercentageComplete);
            canvasGroup.alpha = Mathf.LerpUnclamped(1f, 0f, smoothPosPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                eulaObject.SetActive(false);
                movementElapsedTime = 0f;
                animate = false;
            }
        }
    }
}
