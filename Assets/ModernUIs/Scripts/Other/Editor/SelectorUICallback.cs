using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class SelectorUICallback
    {
        [PostProcessScene(-49)]
        public static void OnPostProcessScene()
        {
            SelectorUI[] selectorUis = Object.FindObjectsOfType<SelectorUI>();

            foreach (var selectorUi in selectorUis)
            {
                if (selectorUi.selectorUIButtons == null || selectorUi.selectorUIButtons.Length == 0)
                {
                    selectorUi.selectorUIButtons = selectorUi.GetComponentsInChildren<SelectorUIButton>();
                }
                
                if (selectorUi.selector == null)
                {
                    selectorUi.selector = selectorUi.GetComponentInChildren<Selector>();
                }
            }
        }
    }
}
