using DrBlackRat.VRC.ModernUIs.Notifications;
using DrBlackRat.VRC.ModernUIs.SelectorUI;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using UdonSharpEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class InstanceTransfersCallback
    {
        [PostProcessScene(-49)]
        public static void OnPostProcessScene()
        {
            TransferPermissionManagers();
            TransferCustomNetNotificationSenderPresetSelector();
            TransferSelectorUIRelays();
        }

        private static void TransferPermissionManagers()
        {
            var permissionManagers = Object.FindObjectsOfType<WhitelistPermissionManager>(true);
            foreach (var permissionManager in permissionManagers)
            {
                permissionManager.TransferValues();
                Object.DestroyImmediate(permissionManager);
            } 
        }

        private static void TransferCustomNetNotificationSenderPresetSelector()
        {
            var presetSelectors =
                Object.FindObjectsOfType<CustomNetNotificationSenderPresetSelector>();
            
            foreach (var presetSelector in presetSelectors)
            {
                presetSelector.TransferValues();
                Object.DestroyImmediate(presetSelector);
            }
        }

        private static void TransferSelectorUIRelays()
        {
            var selectorUIRelays = Object.FindObjectsOfType<SelectorUIRelay>(true);
            foreach (var selectorUIRelay in selectorUIRelays)
            {
                selectorUIRelay.TransferValues();
                Object.DestroyImmediate(selectorUIRelay);
            }
        }
    }
}