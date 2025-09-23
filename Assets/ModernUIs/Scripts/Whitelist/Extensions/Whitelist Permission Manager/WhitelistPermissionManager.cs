using System;
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
        
        // Prefab Theming (Provided by Themed Component normally)
        [SerializeField] protected Color textColor;
        [SerializeField] protected Color backgroundColor;
        
        [SerializeField] protected Color buttonColor;
        [SerializeField] protected Color buttonTextColor;
        
        [Serializable]
        private class Permission
        {
            [Tooltip("Whitelists that are being managed. \nIf the provided whitelist is getter only, it will be treated as read only.")]
            public WhitelistGetterBase whitelist;
            [Tooltip("Admin Whitelists that controls access of the permission. \nIf left empty it will be treated as if the local player is an Admin.")]
            public WhitelistGetterBase adminWhitelist;
            [Tooltip("If enabled the permission will be read only. \nEnabled automatically for getter only Whitelists.")]
            public bool readOnly;
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
            
            WhitelistGetterBase[] whitelists = new WhitelistGetterBase[permissions.Length];
            WhitelistGetterBase[] adminWhitelists = new WhitelistGetterBase[permissions.Length];
            bool[] readOnly = new bool[permissions.Length];

            for (int i = 0; i < permissions.Length; i++)
            {
                whitelists[i] = permissions[i].whitelist;
                adminWhitelists[i] = permissions[i].adminWhitelist;
                readOnly[i] = permissions[i].readOnly;
            }

            instance.whitelists = whitelists;
            instance.adminWhitelists = adminWhitelists;
            instance.readOnly = readOnly;

            instance.pmUserPrefab = pmUserPrefab;
            instance.pmUserTransform = pmUserTransform;

            instance.textColor = textColor;
            instance.backgroundColor = backgroundColor;
            instance.buttonColor = buttonColor;
            instance.buttonTextColor = buttonTextColor;
            
            UdonSharpEditorUtility.CopyProxyToUdon(instance);
        }
        
        public Color TextColor
        {
            get => textColor;
            set => textColor = value;
        }

        public Color BackgroundColor
        {
            get => backgroundColor;
            set => backgroundColor = value;
        }

        public Color ButtonColor
        {
            get => buttonColor;
            set => buttonColor = value;
        }

        public Color ButtonTextColor
        {
            get => buttonTextColor;
            set => buttonTextColor = value;
        }
#endif
    }
}
