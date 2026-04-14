using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class TabSwitcher : UdonSharpBehaviour
    {
        [SerializeField] private VRCTweenEase transitionEasing = VRCTweenEase.OutBack;
        [SerializeField] private float transitionDuration = 0.75f;
        
        [Tooltip("Prefab that will be instantiated when adding a new Tab.")]
        [SerializeField] private GameObject tabPrefab;

        private int prevTabID = 0;
        private int currentTabID = 0;
        private RectTransform tabsTransform;
        private VRCTweenHandle tween;

        [HideInInspector] public int tabId;
        [HideInInspector] public Tab[] tabs;

        private void Start()
        {
            tabsTransform = GetComponent<RectTransform>();
            // Hide Tabs on Start
            foreach (var tab in tabs)
            {
                tab._Hide();
            }
        }

        public void _SwitchTab()
        {
            prevTabID = currentTabID;
            currentTabID = tabId;
            
            // Show tabs that are visible during transition
            for (int i = prevTabID;(prevTabID < currentTabID)? i <= currentTabID : i >= currentTabID ; i += (prevTabID < currentTabID)? 1 : -1)
            {
                tabs[i]._Show();
            }
            
            VRCTween.Kill(tween);
            tween = VRCTween.TweenAnchorPos(tabsTransform, tabs[currentTabID]._GetPos(), transitionDuration, transitionEasing);
            VRCTween.OnComplete(tween, (IUdonEventReceiver)this ,nameof(_TransitionFinished));
        }

        public void _TransitionFinished()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == currentTabID) continue;
                tabs[i]._Hide();
            }
        }
        #region Adding Tabs

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [ContextMenu("Add Tab")]
        public void AddTab()
        {
            var prevTabs = GetComponentsInChildren<Tab>();
            
            var newTab = Instantiate(tabPrefab, transform);
            newTab.name = $"Page {prevTabs.Length + 1}";

            if (prevTabs.Length < 2)
            {
                MUIDebug.LogWarning("Tab Switcher | Could not automatically align newly generated Tab as a minimum of 2 Tabs is required for that. Please align the newly generated Tab manually.");
                return;
            }
            var tabOffset = prevTabs[1].transform.localPosition - prevTabs[0].transform.localPosition;
            newTab.transform.localPosition = prevTabs[^1].transform.localPosition + tabOffset;
        }
#endif
        #endregion
    }
}
