using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PermissionManagerUser : UdonSharpBehaviour
    {
        [Header("General;")]
        [SerializeField] protected TextMeshProUGUI nameTmp;
        [SerializeField] protected Image backgroundImage;

        [Header("Per Button:")]
        [SerializeField] protected Toggle[] toggles;
        [SerializeField] protected Image[] buttonImages;
        [SerializeField] protected Image[] checkmarkImages;
        
        [Header("Teleport:")]
        [SerializeField] protected Button teleportButton;

        private string username;
        private int permissions;
        
        private bool hasTeleport;
        private bool hasTeleportAccess;
        
        private WhitelistPermissionManagerInstance _permissionManagerInstance;

        #region Permission Manager To User Api

        /// <summary>
        /// Sets up this Permission Manger User.
        /// </summary>
        /// <param name="newPermissionManagerInstance">Refernce to the Permission Manager</param>
        /// <param name="newPermissions">Number of permissions this user can have.</param>
        /// <param name="hasPermission">State of each permission.</param>
        /// <param name="newUsername">Username / Display name of this user.</param>
        /// <param name="hasAccess">Access state of each permission.</param>
        /// <param name="newHasTeleport">If a teleport button should be displayed.</param>
        public void _Setup(WhitelistPermissionManagerInstance newPermissionManagerInstance, int newPermissions, bool[] hasAccess, bool[] hasPermission, string newUsername, bool newHasTeleport)
        {
            _permissionManagerInstance = newPermissionManagerInstance;
            username = newUsername;
            nameTmp.text = newUsername;
            permissions = newPermissions;

            for (int i = 0; i < permissions; i++)
            {
                toggles[i].gameObject.SetActive(true);
                toggles[i].interactable = hasAccess[i];
                toggles[i].SetIsOnWithoutNotify(hasPermission[i]);
            }

            if (newHasTeleport)
            {
                hasTeleport = true;
                hasTeleportAccess = hasAccess[permissions];
                
                teleportButton.gameObject.SetActive(hasTeleport);
                teleportButton.interactable = hasTeleportAccess;
            }
        }
        
        /// <summary>
        /// Sets if a specific permission can be changed.
        /// </summary>
        /// <param name="i">Index of the permission that should have its access changed.</param>
        /// <param name="access">True if this permission can be changed, false when not.</param>
        public void _UpdatePermissionAccess(int i, bool access)
        {
            if (hasTeleport && i == permissions)
            {
                hasTeleportAccess = access;
                teleportButton.interactable = access;
                return;
            }
            
            toggles[i].interactable = access;
        }

        /// <summary>
        /// Sets the current state of a specific permission.
        /// </summary>
        /// <param name="i">Index of the permission that should have its state changed.</param>
        /// <param name="state">True if this permission is enabled, false when not.</param>
        public void _UpdatePermissionState(int i, bool state)
        {
            toggles[i].SetIsOnWithoutNotify(state);
        }

        /// <summary>
        /// Removes this Permission Manager User
        /// </summary>
        public void _Destroy()
        {
            Destroy(gameObject);
        }
        #endregion

        #region Button / Toggle Events

        public void _Teleport()
        {
            if (!hasTeleport || !hasTeleportAccess)
                return;

            var player = GeneralExtensions.GetPlayerByDisplayName(username);
            if (player != null)
            {
                Networking.LocalPlayer.TeleportTo(player.GetPosition(), player.GetRotation());
            }
        }
        
        private void ChangeState(int i)
        {
            _permissionManagerInstance.ButtonStateChange(username, i, toggles[i].isOn);
        }

        // I don't like this but idk what to do about it...
        public void _ToggleChanged0() => ChangeState(0);
        public void _ToggleChanged1() => ChangeState(1);
        public void _ToggleChanged2() => ChangeState(2);
        public void _ToggleChanged3() => ChangeState(3);
        public void _ToggleChanged4() => ChangeState(4);
        public void _ToggleChanged5() => ChangeState(5);
        public void _ToggleChanged6() => ChangeState(6);
        public void _ToggleChanged7() => ChangeState(7);
        public void _ToggleChanged8() => ChangeState(8);
        public void _ToggleChanged9() => ChangeState(9);
        public void _ToggleChanged10() => ChangeState(10);
        public void _ToggleChanged11() => ChangeState(11);
        public void _ToggleChanged12() => ChangeState(12);
        public void _ToggleChanged13() => ChangeState(13);
        public void _ToggleChanged14() => ChangeState(14);
        public void _ToggleChanged15() => ChangeState(15);
        #endregion
    } 
}

