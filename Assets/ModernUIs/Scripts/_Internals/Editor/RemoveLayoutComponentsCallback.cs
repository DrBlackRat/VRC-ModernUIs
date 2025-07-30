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
                    
                    Object.DestroyImmediate(component);
                }
                
                var fitterComponents = remover.GetComponentsInChildren<ContentSizeFitter>(true);
                for (int i = fitterComponents.Length - 1; i >= 0; i--)
                {
                    var fitterComponent = fitterComponents[i];
                    if (remover.contentSizeFittersToIgnore.Contains<ContentSizeFitter>(fitterComponent)) continue;
                    
                    Object.DestroyImmediate(fitterComponent);
                }
                
                var layoutElementComponents = remover.GetComponentsInChildren<LayoutElement>(true);
                for (int i = layoutElementComponents.Length - 1; i >= 0; i--)
                {
                    var layoutElement = layoutElementComponents[i];
                    if (remover.layoutElementsToIgnore.Contains<LayoutElement>(layoutElement)) continue;
                    
                    Object.DestroyImmediate(layoutElement);
                }
                
                /* Prev Code, just here in case I need it again
                var horizontalComponents = remover.GetComponentsInChildren<HorizontalLayoutGroup>(true);
                for (int i = horizontalComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(horizontalComponents[i]);
                }
                
                var verticalComponents = remover.GetComponentsInChildren<VerticalLayoutGroup>(true);
                for (int i = verticalComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(verticalComponents[i]);
                }
                
                var gridComponents = remover.GetComponentsInChildren<GridLayoutGroup>(true);
                for (int i = gridComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(gridComponents[i]);
                }
                */
            }
        }
    }
}
