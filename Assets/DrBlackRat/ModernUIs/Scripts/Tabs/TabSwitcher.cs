using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class TabSwitcher : UdonSharpBehaviour
    {
        [FormerlySerializedAs("transitionCurve")] 
        [SerializeField] private AnimationCurve animationCurve;
        [FormerlySerializedAs("transitionDuration")] 
        [SerializeField] private float movementDuration = 1f;
        
        [Tooltip("Prefab that will be instantiated when adding a new Tab.")]
        [SerializeField] private GameObject tabPrefab;

        private int prevTabID;
        private int currentTabID;
        private RectTransform tabsTransform;
        private VRCTweenHandle tweenHandle;

        [HideInInspector] public int tabId;
        [HideInInspector] public Tab[] tabs;

        private void Start()
        {
            tabsTransform = GetComponent<RectTransform>();
            SetupTween();

            tabsTransform.anchoredPosition = (tabs[currentTabID]._GetPos());
            _HideNoneCurrentTabs();
        }

        private void SetupTween()
        {
            tweenHandle = tabsTransform.TweenAnchorPos(new Vector2(), movementDuration, VRCTweenEase.None)
                .SetEase(animationCurve)
                .OnComplete(this, nameof(_HideNoneCurrentTabs))
                .Pause();
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
            
            tweenHandle.ChangeEndValue(tabs[currentTabID]._GetPos(), true);
            tweenHandle.Restart();
        }

        public void _HideNoneCurrentTabs()
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
            
            var newTab = UnityEditor.PrefabUtility.InstantiatePrefab(tabPrefab, transform) as GameObject;
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
