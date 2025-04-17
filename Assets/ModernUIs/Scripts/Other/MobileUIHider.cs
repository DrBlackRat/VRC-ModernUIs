using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs
{
    public class MobileUIHider : MonoBehaviour
    {
        // This is just a marker for the OnPostProcessScene script
        [Header("Settings:")]
        [Tooltip("Objects that should be disabled for users on Android or iOS.")]
        public GameObject[] objectsToDisable;
        [Space(10)] 
        [Tooltip("UI Transforms that should be moved if a user is on Android or iOS.")]
        public RectTransform[] moveTransforms;
        [Tooltip("Position of Element Transforms if a user is on Android or iOS. Same order as Move Transforms is used.")]
        public Vector2[] hiddenPositions;
        [Space(10)]
        [Tooltip("UI Transforms that should be resized if a user is on Android or iOS.")]
        public RectTransform[] sizeTransforms;
        [Tooltip("Size of Element Transforms if a user is on Android or iOS. Same order as Size Transforms is used.")]
        public Vector2[] hiddenSize;

    }
}