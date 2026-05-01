using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUISeparator : UdonSharpBehaviour
    {
        [SerializeField] private RectTransform recTransform;
        [SerializeField] private Vector2[] positions;
        
        [FormerlySerializedAs("animationCurve")]
        [SerializeField] private AnimationCurve smoothingCurve;
        [SerializeField] private float movementDuration;
        
        private VRCTweenHandle tweenHandle;
        private bool isTweenSetup;

        [HideInInspector] public int selectionId;

        public void _SelectionChanged()
        {
            if (!isTweenSetup)
            {
                tweenHandle = recTransform.TweenAnchorPos(new Vector2(), movementDuration, VRCTweenEase.None)
                    .SetEase(smoothingCurve)
                    .Pause();
                isTweenSetup = true;
            }
            
            tweenHandle.ChangeEndValue(positions[selectionId], true)
                .Restart();
        }
    }
}
