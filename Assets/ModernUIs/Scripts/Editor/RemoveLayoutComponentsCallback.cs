using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace DrBlackRat.VRC.ModernUIs
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
                foreach (var contentSizeFitter in fitterComponents)
                {
                    Object.Destroy(contentSizeFitter);
                    //contentSizeFitter.enabled = false;
                }
                
                var verticalComponents = remover.GetComponentsInChildren<VerticalLayoutGroup>();
                foreach (var verticalLayout in verticalComponents)
                {
                    Object.Destroy(verticalLayout);
                    //verticalLayout.enabled = false;
                }
                
                var horizontalComponents = remover.GetComponentsInChildren<HorizontalLayoutGroup>();
                foreach (var horizontalLayout in horizontalComponents)
                {
                    Object.Destroy(horizontalLayout);
                    //horizontalLayout.enabled = false;
                }
                
                var gridComponents = remover.GetComponentsInChildren<GridLayoutGroup>();
                foreach (var gridLayout in gridComponents)
                {
                    Object.Destroy(gridLayout);
                    //gridLayout.enabled = false;
                }
                
                var layoutElementComponents = remover.GetComponentsInChildren<LayoutElement>();
                foreach (var layoutElement in layoutElementComponents)
                {
                    Object.Destroy(layoutElement);
                    //layoutElement.enabled = false;
                }
            }
        }
    }
}
