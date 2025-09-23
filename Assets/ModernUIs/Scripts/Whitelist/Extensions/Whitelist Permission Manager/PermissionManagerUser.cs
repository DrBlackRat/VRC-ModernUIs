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
        [SerializeField] protected GameObject[] toggleObjs;
        [SerializeField] protected Toggle[] toggles;
        [SerializeField] protected Image[] buttonImages;
        [SerializeField] protected Image[] checkmarkImages;

        private string username;
        private int permissions;
        private WhitelistPermissionManagerInstance _permissionManagerInstance;

        #region Permission Manager To User Api
        /// <summary>
        /// Sets up this Permission Manger User.
        /// </summary>
        /// <param name="newPermissionManagerInstance">Refernce to the Permission Manager</param>
        /// <param name="permissions">Amount of permissions this user can have.</param>
        /// <param name="newUsername">Username / Display name of this user.</param>
        public void _Setup(WhitelistPermissionManagerInstance newPermissionManagerInstance, int newPermissions, bool[] hasAccess, bool[] hasPermission,string newUsername)
        {
            _permissionManagerInstance = newPermissionManagerInstance;
            username = newUsername;
            nameTmp.text = newUsername;
            permissions = newPermissions;

            for (int i = 0; i < permissions; i++)
            {
                toggleObjs[i].SetActive(true);
                toggles[i].interactable = hasAccess[i];
                toggles[i].SetIsOnWithoutNotify(hasPermission[i]);
            }
        }

        /// <summary>
        /// Sets up the correct Theme colors.
        /// </summary>
        public void _ApplyThemeColors(Color textColor, Color backgroundColor, Color buttonColor, Color buttonTextColor)
        {
            nameTmp.color = textColor;
            backgroundImage.color = backgroundColor;

            for (int i = 0; i < permissions; i++)
            {
                buttonImages[i].color = buttonColor;
                checkmarkImages[i].color = buttonTextColor;
            }
        }
        
        /// <summary>
        /// Sets if a specific permission can be changed.
        /// </summary>
        /// <param name="i">Index of the permission that should have its access changed.</param>
        /// <param name="access">True if this permission can be changed, false when not.</param>
        public void _UpdatePermissionAccess(int i, bool access)
        {
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

        #region User To Permission Manager Api
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

