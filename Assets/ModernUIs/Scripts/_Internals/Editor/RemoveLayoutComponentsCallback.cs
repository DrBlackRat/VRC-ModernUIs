using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class RemoveLayoutComponentsCallback
    {
        [PostProcessScene(-50)]
        public static void OnPostProcessScene()
        {
            RemoveLayoutComponents[] removers = Object.FindObjectsOfType<RemoveLayoutComponents>(true);
            Canvas.ForceUpdateCanvases();
            
            foreach (var remover in removers)
            {
                if (!remover.removeLayoutComponents) continue;
                
                var layoutComponents = remover.GetComponentsInChildren<LayoutGroup>(true);
                for (int i = layoutComponents.Length - 1; i >= 0; i--)
                {
                    var component = layoutComponents[i];
                    if (remover.layoutGroupsToIgnore.Contains<LayoutGroup>(component)) continue;
                    if (component.gameObject.GetComponent<KeepLayoutComponents>()) continue;
                    
                    Object.DestroyImmediate(component);
                }
                
                var fitterComponents = remover.GetComponentsInChildren<ContentSizeFitter>(true);
                for (int i = fitterComponents.Length - 1; i >= 0; i--)
                {
                    var fitterComponent = fitterComponents[i];
                    if (remover.contentSizeFittersToIgnore.Contains<ContentSizeFitter>(fitterComponent)) continue;
                    if (fitterComponent.gameObject.GetComponent<KeepLayoutComponents>()) continue;
                    
                    Object.DestroyImmediate(fitterComponent);
                }
                
                var layoutElementComponents = remover.GetComponentsInChildren<LayoutElement>(true);
                for (int i = layoutElementComponents.Length - 1; i >= 0; i--)
                {
                    var layoutElement = layoutElementComponents[i];
                    if (remover.layoutElementsToIgnore.Contains<LayoutElement>(layoutElement)) continue;
                    if (layoutElement.gameObject.GetComponent<KeepLayoutComponents>()) continue;
                    
                    Object.DestroyImmediate(layoutElement);
                }
            }
            
            KeepLayoutComponents[] keepers = Object.FindObjectsOfType<KeepLayoutComponents>(true);
            foreach (var keeper in keepers)
            {
                Object.DestroyImmediate(keeper);
            }
        }
    }
}
