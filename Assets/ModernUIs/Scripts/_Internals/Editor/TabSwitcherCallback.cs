using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Utils.Editor
{
    public static class TabSwitcherCallback
    {
        [PostProcessScene(-49)]
        public static void OnPostProcessScene()
        {
            TabSwitcher[] tabSwitchers = Object.FindObjectsOfType<TabSwitcher>();

            foreach (var tabSwitcher in tabSwitchers)
            {
                tabSwitcher.tabs = tabSwitcher.GetComponentsInChildren<Tab>();
                if (tabSwitcher.tabs == null || tabSwitcher.tabs.Length == 0) MUIDebug.LogError("No Tabs found in Tab Switcher children!\nTabs are required for the Tab Switcher to function!");
            }
        }
    }
}
