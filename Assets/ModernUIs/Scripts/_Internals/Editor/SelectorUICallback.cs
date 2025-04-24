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
                selectorUi.selectorUIButtons = selectorUi.GetComponentsInChildren<SelectorUIButton>();
                if (selectorUi.selectorUIButtons == null || selectorUi.selectorUIButtons.Length == 0) MUIDebug.LogError("No Selector UI Buttons found in Selector UI children!\nSelector UI Buttons are required for the Selector UI to function!");
                
                selectorUi.selector = selectorUi.GetComponentInChildren<Selector>();
                if (selectorUi.selector == null) MUIDebug.LogError("No Selector found in Selector UI children!\nA Selector is required for the Selector UI to function!");
            }
        }
    }
}
