using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EulaScreen : UdonSharpBehaviour
    {
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
        
        [Tooltip("Objects that should be turned on when the Eula has been accepted.")]
        [SerializeField] protected GameObject[] enableObjects;
        [FormerlySerializedAs("disableObject")]
        [Tooltip("Objects that should be turned off when the Eula has been accepted. Use full for things like a collider box.")]
        [SerializeField] private GameObject[] disableObjects;
        
        [Tooltip("Animators that should have an bool with the given parameter name set to true when accepted.")]
        [SerializeField] protected Animator[] animators;
        [Tooltip("Parameter Name of the bool that should be set.")]
        [SerializeField] protected string animParamName = "eulaAccepted";
        
        [Tooltip("Udon Behaviours that the accepted event will be send to.")]
        [SerializeField] protected UdonBehaviour[] externalBehaviours;
        [Tooltip("Name of Event that will be fired if the selection changes.")]
        [SerializeField] protected string acceptedEventName = "_EulaAccepted";
        
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;
        
        private bool accepted;

        private void Start()
        {
            if (versionChangeInfo != null) 
                versionChangeInfo.SetActive(false);
            
            UpdateExternal(false);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) 
                return;
            
            if (!PlayerData.TryGetInt(player, dataKey, out int value)) 
                return;
            
            if (value < ruleVer)
            {
                if (versionChangeInfo != null) 
                    versionChangeInfo.SetActive(true);
                return;
            }
            
            HideEula();
        }
        
        
        public void _Accepted()
        {
            if (usePersistence) 
                PlayerData.SetInt(dataKey, ruleVer);
            
            HideEula();
        }
        
        private void HideEula()
        {
            MUIDebug.Log("EULA Accepted, hiding EULA screen!");
            if (accepted)
            {
                MUIDebug.LogWarning("EULA already accepted, ignoring hide request. (This should not happen normally!)");
                return;
            }
            accepted = true;
            
            UpdateExternal(true);

            VRCTween.DelayedSetActive(eulaObject, false, movementDuration);
            eulaObject.TweenPosition(hiddenTransform.position, movementDuration, VRCTweenEase.None)
                .SetEase(animationCurve);
            eulaObject.TweenScale(hiddenTransform.localScale, movementDuration, VRCTweenEase.None)
                .SetEase(animationCurve);
            
            canvasGroup.TweenFade(0f ,movementDuration, VRCTweenEase.None)
                .SetEase(animationCurve);
            
        }

        private void UpdateExternal(bool accepted)
        {
            if (enableObjects != null && enableObjects.Length != 0)
            {
                foreach (var enableObject in enableObjects)
                {
                    if (enableObject == null) continue;
                    enableObject.SetActive(accepted);
                }
            }
            
            if (disableObjects != null && disableObjects.Length != 0)
            {
                foreach (var disableObject in disableObjects)
                {
                    if (disableObject == null) continue;
                    disableObject.SetActive(!accepted);
                }
            }
            
            if (animators != null && animators.Length > 0)
            {
                foreach (var animator in animators)
                {
                    if (animator == null) continue;
                    animator.SetBool(animParamName, accepted);
                }
            }
            
            if (accepted && externalBehaviours != null && externalBehaviours.Length > 0)
            {
                foreach (var behaviour in externalBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SendCustomEvent(acceptedEventName);
                }
            }
        }
    }
}
