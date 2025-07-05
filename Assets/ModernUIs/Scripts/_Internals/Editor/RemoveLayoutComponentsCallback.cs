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
                
                var horizontalComponents = remover.GetComponentsInChildren<HorizontalLayoutGroup>(true);
                for (int i = horizontalComponents.Length - 1; i >= 0; i--)
                {
                    Object.Destroy(horizontalComponents[i]);
                }
                
                var verticalComponents = remover.GetComponentsInChildren<VerticalLayoutGroup>(true);
                for (int i = verticalComponents.Length - 1; i >= 0; i--)
                {
                    Object.Destroy(verticalComponents[i]);
                }
                
                var gridComponents = remover.GetComponentsInChildren<GridLayoutGroup>(true);
                for (int i = gridComponents.Length - 1; i >= 0; i--)
                {
                    Object.Destroy(gridComponents[i]);
                }
                
                var layoutElementComponents = remover.GetComponentsInChildren<LayoutElement>(true);
                for (int i = layoutElementComponents.Length - 1; i >= 0; i--)
                {
                    Object.Destroy(layoutElementComponents[i]);
                }
                
                var fitterComponents = remover.GetComponentsInChildren<ContentSizeFitter>(true);
                for (int i = fitterComponents.Length - 1; i >= 0; i--)
                {
                    Object.Destroy(fitterComponents[i]);
                }
            }
        }
    }
}
