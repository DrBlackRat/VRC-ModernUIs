
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CollapseButton : UdonSharpBehaviour
    {
        [Tooltip("Default expanded / collapsed state. If enabled names are hidden by default and you have to first expand it to show.")]
        [SerializeField] private bool collapsed;
        
        [Tooltip("Turn on if this Collapse Button should be saved using Persistence.")]
        [SerializeField] private bool usePersistence;
        [Tooltip("Data Key that will be used to save / load, everything using Persistence should have a different Data Key.")]
        [SerializeField] private string dataKey = "CHANGE THIS";
        
        [Tooltip("Icon that is shown if it's expanded.")]
        [SerializeField] private GameObject expandedIcon;
        [Tooltip("Icon that is shown if it's collapsed.")]
        [SerializeField] private GameObject collapsedIcon;
        
        [Tooltip("Game Objects to disable / enable when collapsed or not.")]
        [SerializeField] private GameObject[] collapsableGameObjects;
        
        [Tooltip("ID of this collapse button, set on the external Udon Behaviours to identify this specific collapse button.")]
        [SerializeField] private int collapseButtonID;
        [Tooltip("Name of the Integer Variable of the collapse button ID that will be set when the collapse button changes state.")]
        [SerializeField] private string collapseButtonIdValueName = "collapseButtonID";
        
        [Tooltip("Udon Behaviours that will have events send to.")]
        [SerializeField] private UdonBehaviour[] externalBehaviours;
        [Tooltip("Name of the event that will be fired if the collapse button is collapsed.")]
        [SerializeField] private string collapsedEventName = "_Collapsed";
        [Tooltip("Name of the event that will be fired if the collapse button is expanded.")]
        [SerializeField] private string expandedEventName = "_Expanded";
        
        // Legacy
        [SerializeField] private FancyWhitelistNameDisplay fancyWhitelistNameDisplay;

        // Legacy
        [Tooltip("If enabled the \"Other Users\" collapsed state will be changed, if not it will change the \"Whitelisted Users\"collapsed state.")]
        [SerializeField] private bool otherUserMode;
        [SerializeField] private WhitelistEditorBase whitelistEditorBase;
        
        void Start()
        {
            UpdateCollapsedState(collapsed, false, true);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!usePersistence || player != Networking.LocalPlayer) return;

            if (PlayerData.TryGetBool(player, dataKey, out bool value))
            {
                Debug.LogError($"Found Collapsed: {value}");
                UpdateCollapsedState(value, false);
            }
        }

        public void _ToggleCollapsed()
        {
            UpdateCollapsedState(!collapsed, true);
        }
        
        private void UpdateCollapsedState(bool newCollapsed, bool saveData, bool skipSameCheck = false)
        {
            if (newCollapsed == collapsed && !skipSameCheck) return;
            collapsed = newCollapsed;
            
            if (saveData)
                PlayerData.SetBool(dataKey, collapsed);
            
            UpdateUI();
            UpdateExternal(collapsed);
        }

        private void UpdateUI()
        {
            if (expandedIcon != null)
                expandedIcon.SetActive(!collapsed);
            
            if (collapsedIcon != null)
                collapsedIcon.SetActive(collapsed);
        }

        private void UpdateExternal(bool newCollapsed)
        {
            if (collapsableGameObjects != null && collapsableGameObjects.Length > 0)
            {
                foreach (var collapsableObj in collapsableGameObjects)
                {
                    if (collapsableObj == null) continue;
                    collapsableObj.SetActive(!newCollapsed);
                }
            }

            if (externalBehaviours != null && externalBehaviours.Length > 0)
            {
                foreach (var behaviour in externalBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SetProgramVariable(collapseButtonIdValueName, collapseButtonID);
                    behaviour.SendCustomEvent(collapsed ? collapsedEventName : expandedEventName);
                }
            }
            
            // Legacy
            if (fancyWhitelistNameDisplay != null)
                fancyWhitelistNameDisplay._UpdateCollapsed(collapsed);

            // Legacy
            if (whitelistEditorBase != null)
            {
                if (otherUserMode)
                {
                    whitelistEditorBase._UpdateOtherUsersCollapsed(collapsed);
                }
                else
                {
                    whitelistEditorBase._UpdateWhitelistCollapsed(collapsed);
                }
            }
        }
    }
}
