using DrBlackRat.VRC.ModernUIs.Whitelist;
using UdonSharpEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class WhitelistPermissionManagerCallback
    {
        [PostProcessScene(-49)]
        public static void OnPostProcessScene()
        {
            WhitelistPermissionManager[] permissionManagers = Object.FindObjectsOfType<WhitelistPermissionManager>(true);
            foreach (var permissionManager in permissionManagers)
            {
                permissionManager.TransferValues();
                Object.DestroyImmediate(permissionManager);
            }
        }
    }
}