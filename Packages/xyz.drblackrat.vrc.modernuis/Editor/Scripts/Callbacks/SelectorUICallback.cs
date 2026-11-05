using DrBlackRat.VRC.ModernUIs.SelectorUI;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class SelectorUICallback
    {
        [PostProcessScene(-49)]
        public static void OnPostProcessScene()
        {
            SelectorUI.SelectorUI[] selectorUis = Object.FindObjectsOfType<SelectorUI.SelectorUI>(true);

            foreach (var selectorUi in selectorUis)
            {
                selectorUi.selectorUIButtons = selectorUi.GetComponentsInChildren<SelectorUIButton>(true);
                if (selectorUi.selectorUIButtons == null || selectorUi.selectorUIButtons.Length == 0) MUIDebug.LogError("No Selector UI Buttons found in Selector UI children!\nSelector UI Buttons are required for the Selector UI to function!");
                
                selectorUi.selector = selectorUi.GetComponentInChildren<Selector>(true);
                if (selectorUi.selector == null) MUIDebug.LogError("No Selector found in Selector UI children!\nA Selector is required for the Selector UI to function!");
            }
        }
    }
}
