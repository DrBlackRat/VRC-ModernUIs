using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class TabSwitcherCallback
    {
        [PostProcessScene(-49)]
        public static void OnPostProcessScene()
        {
            TabSwitcher[] tabSwitchers = Object.FindObjectsOfType<TabSwitcher>(true);

            foreach (var tabSwitcher in tabSwitchers)
            {
                /* Old Stuff
                tabSwitcher.tabs = tabSwitcher.GetComponentsInChildren<Tab>(true);
                if (tabSwitcher.tabs == null || tabSwitcher.tabs.Length == 0) MUIDebug.LogError("No Tabs found in Tab Switcher children!\nTabs are required for the Tab Switcher to function!");
                */
                tabSwitcher.tabs = GetChildTabs(tabSwitcher);
            }
        }

        private static Tab[] GetChildTabs(TabSwitcher tabSwitcher)
        {
            Tab[] tabs = new Tab[]{};
            Transform tabTrans = tabSwitcher.transform;

            for (int i = 0; i < tabTrans.childCount; i++)
            {
                var childObj = tabTrans.GetChild(i).gameObject;
                var tab = childObj.GetComponent<Tab>();
                if (tab != null)
                    tabs = tabs.Add(tab);
            }
            
            return tabs;
        }
    }
}
