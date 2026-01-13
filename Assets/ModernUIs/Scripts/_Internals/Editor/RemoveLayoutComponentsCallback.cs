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
            RemoveLayoutComponents[] removers = Object.FindObjectsOfType<RemoveLayoutComponents>();
            Canvas.ForceUpdateCanvases();
            
            foreach (var remover in removers)
            {
                if (!remover.removeLayoutComponents) continue;
                
                var fitterComponents = remover.GetComponentsInChildren<ContentSizeFitter>();
                for (int i = fitterComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(fitterComponents[i]);
                }
                
                var verticalComponents = remover.GetComponentsInChildren<VerticalLayoutGroup>();
                for (int i = verticalComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(verticalComponents[i]);
                }
                
                var horizontalComponents = remover.GetComponentsInChildren<HorizontalLayoutGroup>();
                for (int i = horizontalComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(horizontalComponents[i]);
                }
                
                var gridComponents = remover.GetComponentsInChildren<GridLayoutGroup>();
                for (int i = gridComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(gridComponents[i]);
                }
                
                var layoutElementComponents = remover.GetComponentsInChildren<LayoutElement>();
                for (int i = layoutElementComponents.Length - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(layoutElementComponents[i]);
                }
            }
        }
    }
}
