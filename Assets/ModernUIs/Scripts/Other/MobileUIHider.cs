
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-9000)]
    public class MobileUIHider : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [Tooltip("Objects that should be disabled for users on Android or iOS.")]
        [SerializeField] private GameObject[] objectsToDisable;
        [Space(10)] 
        [Tooltip("UI Transforms that should be moved if a user is on Android or iOS.")]
        [SerializeField] private RectTransform[] moveTransforms;
        [Tooltip("Position of Element Transforms if a user is on Android or iOS. Same order as Move Transforms is used.")]
        [SerializeField] private Vector2[] hiddenPositions;
        [Space(10)]
        [Tooltip("UI Transforms that should be resized if a user is on Android or iOS.")]
        [SerializeField] private RectTransform[] sizeTransforms;
        [Tooltip("Size of Element Transforms if a user is on Android or iOS. Same order as Size Transforms is used.")]
        [SerializeField] private Vector2[] hiddenSize;

        
        private void Start()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            foreach (var obj in objectsToDisable)
            {
                if (obj == null) continue;
                obj.SetActive(false);
                Debug.LogError("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            }
            
            for (int i = 0; i < moveTransforms.Length; i++)
            {
                if (moveTransforms[i] == null) continue;
                moveTransforms[i].anchoredPosition = hiddenPositions[i];
                Debug.LogError("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            }
            
            for (int i = 0; i < sizeTransforms.Length; i++)
            {
                if (sizeTransforms[i] == null) continue;
                sizeTransforms[i].sizeDelta = hiddenSize[i];
                Debug.LogError("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            }
#endif
            

        }
    }
}
