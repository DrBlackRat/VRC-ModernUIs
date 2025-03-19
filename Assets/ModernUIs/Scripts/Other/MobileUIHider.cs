
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MobileUIHider : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [SerializeField] private GameObject[] objectsToDisable;
        [Space(10)] 
        [SerializeField] private RectTransform[] moveTransforms;
        [SerializeField] private Vector2[] hiddenPositions;


        private void Start()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            foreach (var obj in objectsToDisable)
            {
                if (obj == null) continue;
                obj.SetActive(false);
            }
            
            for (int i = 0; i < moveTransforms.Length; i++)
            {
                if (moveTransforms[i] == null) continue;
                moveTransforms[i].anchoredPosition = hiddenPositions[i];
            }
#endif

        }
    }
}
