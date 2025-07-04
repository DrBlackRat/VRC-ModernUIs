using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class MobileUIHiderCallback
    {
        [PostProcessScene(-100)]
        public static void OnPostProcessScene()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            MobileUIHider[] mobileUIHiders = Object.FindObjectsOfType<MobileUIHider>();
            foreach (var mobileUIHider in mobileUIHiders)
            {
                foreach (var obj in mobileUIHider.objectsToDisable)
                {
                    if (obj == null) continue;
                    obj.SetActive(false);
                }

                foreach (var obj in mobileUIHider.objectsToEnable)
                {
                    if (obj == null) continue;
                    obj.SetActive(true);
                }
                
                for (int i = 0; i < mobileUIHider.moveTransforms.Length; i++)
                {
                    if (mobileUIHider.moveTransforms[i] == null) continue;
                    mobileUIHider.moveTransforms[i].anchoredPosition = mobileUIHider.hiddenPositions[i];
                }
                
                for (int i = 0; i < mobileUIHider.sizeTransforms.Length; i++)
                {
                    if (mobileUIHider.sizeTransforms[i] == null) continue;
                    mobileUIHider.sizeTransforms[i].sizeDelta = mobileUIHider.hiddenSize[i];
                }
            }
#endif
        }
    }
}
