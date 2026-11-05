using System;
using TMPro;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UdonSharpEditor;
#endif
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    /// <summary>
    /// Sets up a Whitelist Permission Manager Instance (Editor Only)
    /// </summary>
    [RequireComponent(typeof(WhitelistPermissionManagerInstance))]
    public class WhitelistPermissionManager : MonoBehaviour
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [SerializeField] private Permission[] permissions;

        // PM = PermissionManager
        [Tooltip("Prefab that will be instantiated for every user in the instance.")]
        [SerializeField] protected GameObject pmUserPrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] protected Transform pmUserTransform;
        
        [Tooltip("Conformation Dialog that can be shown when adding or removing a user from the whitelist.")]
        [SerializeField] protected GameObject confirmationDialog;
        [Tooltip("Text Mesh Pro UGUI component that will be used in the Conformation Dialog.")]
        [SerializeField] protected TextMeshProUGUI confirmationDialogText;
        
        [TextArea(1, 3)][Tooltip("Text that will be used in the Conformation Dialog for adding a user.")]
        [SerializeField] protected string confirmationDialogAddString = $"Are you sure you want to add \"<username>\" to <permissionName>?";
        [TextArea(1, 3)][Tooltip("Text that will be used in the Conformation Dialog for removing a user.")]
        [SerializeField] protected string confirmationDialogRemoveString = $"Are you sure you want to remove \"<username>\" from <permissionName>?";
        
        [Tooltip("If enabled the the users shown in the Permission Manager will automatically be sorted alphabetically.\n This does come with a slight performance impact, otherwise you can call \"_SortUsers\" manually.")]
        [SerializeField] protected bool autoSort = true;
        
        [Tooltip("If enabled a Teleport button will be added to each user in the Permission Manager.")]
        [SerializeField] protected bool allowTeleport = false;
        [Tooltip("Whitelist of users that are allowed to teleport.")]
        [SerializeField] protected WhitelistGetterBase teleportWhitelist;
        
        [Serializable]
        private class Permission
        {
            [Tooltip("Name of the Whitelist, currently only used by the conformation dialog.")]
            public string permissionName = "Whitelist";
            [Tooltip("Whitelists that are being managed. \nIf the provided whitelist is getter only, it will be treated as read only.")]
            public WhitelistGetterBase whitelist;
            [Tooltip("Admin Whitelists that controls access of the permission. \nIf left empty it will be treated as if the local player is an Admin.")]
            public WhitelistGetterBase adminWhitelist;
            [Tooltip("If enabled the permission will be read only. \nEnabled automatically for getter only Whitelists.")]
            public bool readOnly;
            [Tooltip("If enabled a conformation dialog will be shown when adding or removing a user from the whitelist.")]
            public bool showConformationDialog;
        }

        /// <summary>
        /// Finds the <see cref="WhitelistPermissionManagerInstance"/> and transfers Permissions, Prefabs and Themes over to it.
        /// </summary>
        public void TransferValues()
        {
            var instance = GetComponent<WhitelistPermissionManagerInstance>();
            if (instance == null)
            {
                MUIDebug.LogError("Whitelist Permission Manager | No Permission Manager Instance found!", gameObject);
                return;
            }
            
            string[] permissionNames = new string[permissions.Length];
            WhitelistGetterBase[] whitelists = new WhitelistGetterBase[permissions.Length];
            WhitelistGetterBase[] adminWhitelists = new WhitelistGetterBase[permissions.Length];
            bool[] isReadOnly = new bool[permissions.Length];
            bool[] showConfirmationDialog = new bool[permissions.Length];

            for (int i = 0; i < permissions.Length; i++)
            {
                permissionNames[i] = permissions[i].permissionName;
                whitelists[i] = permissions[i].whitelist;
                adminWhitelists[i] = permissions[i].adminWhitelist;
                isReadOnly[i] = permissions[i].readOnly;
                showConfirmationDialog[i] = permissions[i].showConformationDialog;
            }

            instance.permissionNames = permissionNames;
            instance.whitelists = whitelists;
            instance.adminWhitelists = adminWhitelists;
            instance.isReadOnly = isReadOnly;
            instance.showConformationDialog = showConfirmationDialog;

            instance.pmUserPrefab = pmUserPrefab;
            instance.pmUserTransform = pmUserTransform;

            instance.confirmationDialog = confirmationDialog;
            instance.confirmationDialogText = confirmationDialogText;
            
            instance.confirmationDialogAddString = confirmationDialogAddString;
            instance.confirmationDialogRemoveString = confirmationDialogRemoveString;
            
            instance.autoSort = autoSort;
            
            instance.allowTeleport = allowTeleport;
            instance.teleportWhitelist = teleportWhitelist;
            
            UdonSharpEditorUtility.CopyProxyToUdon(instance);
        }
#endif
    }
}
