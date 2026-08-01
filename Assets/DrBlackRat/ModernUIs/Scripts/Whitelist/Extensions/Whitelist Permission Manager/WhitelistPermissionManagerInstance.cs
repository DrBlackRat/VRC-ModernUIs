using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    /// <summary>
    /// Instance of a Whitelist Permission Manager. (In Game)
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistPermissionManagerInstance : UdonSharpBehaviour
    {
        [HideInInspector] public string[] permissionNames;
        [HideInInspector] public WhitelistGetterBase[] whitelists;
        [HideInInspector] public WhitelistGetterBase[] adminWhitelists;
        [HideInInspector] public bool[] isReadOnly;
        [HideInInspector] public bool[] showConformationDialog;
        
        [HideInInspector] public GameObject confirmationDialog;
        [HideInInspector] public TextMeshProUGUI confirmationDialogText;

        [HideInInspector] public string confirmationDialogAddString;
        [HideInInspector] public string confirmationDialogRemoveString;
        
        [HideInInspector] public bool autoSort;

        [HideInInspector] public bool allowTeleport;
        [HideInInspector] public WhitelistGetterBase teleportWhitelist;
        
        // PM = PermissionManager
        [HideInInspector] public GameObject pmUserPrefab;
        [HideInInspector] public Transform pmUserTransform;
        
        protected DataDictionary pmUsers = new DataDictionary(); // Key: username, Value: Reference to the Permission Manager User
        protected bool hasInitialized;
        protected int permissions;
        
        protected VRCPlayerApi localPlayer;
        protected string localDisplayName;
        
        protected int showingConfirmationDialogFor = -1; // Index of the permission that is currently being changed.
        protected bool isAddConfirmationDialog = false; // True if the dialog is for adding a user, false if it's for removing a user.'
        protected string confirmationDialogUsername;
        
        protected bool[] hasAccess; // Last index is used for Teleport
        
        protected bool sortCooldownEnabled = true;

        private const int MAX_WHITELISTS = 8; // Currently set to 8 due to UI limitations
        
        #region Unity & VRChat Events
        private void Start()
        {
            if (whitelists.Length > MAX_WHITELISTS || adminWhitelists.Length > MAX_WHITELISTS || isReadOnly.Length > MAX_WHITELISTS)
            {
                MUIDebug.LogError($"Permission Manager | Only a max of {MAX_WHITELISTS} Whitelists can be used!");
                return;
            }

            if (whitelists.Length != adminWhitelists.Length || whitelists.Length != isReadOnly.Length)
            {
                MUIDebug.LogError($"Permission Manager | There needs to be an equal amount of Whitelists, Admin Whitelists and Read Only toggles!");
                return;
            }

            permissions = whitelists.Length;
            hasAccess = new bool[permissions + (allowTeleport ? 1 : 0)];
            
            localPlayer = Networking.LocalPlayer;
            localDisplayName = localPlayer.displayName;

            if (allowTeleport)
            {
                if (teleportWhitelist == null)
                {
                    hasAccess[permissions] = true;
                }
                else
                {
                    adminWhitelists = adminWhitelists.Add(teleportWhitelist);
                    teleportWhitelist._SetUpConnection((IUdonEventReceiver)this, $"_AdminWhitelistUpdated{permissions}");
                }
            }

            for (int i = 0; i < permissions; i++)
            {
                var whitelist = whitelists[i];
                var adminWhitelist = adminWhitelists[i];
                
                isReadOnly[i] = whitelist._IsReadOnly() || isReadOnly[i];
                
                whitelist._SetUpConnection((IUdonEventReceiver)this, $"_WhitelistUpdated{i}");

                if (!isReadOnly[i])
                {
                    if (adminWhitelist != null)
                    {
                        adminWhitelist._SetUpConnection((IUdonEventReceiver)this, $"_AdminWhitelistUpdated{i}");;
                    }
                    else
                    {
                        UpdatePermissionAccess(i, true);
                    }
                }
            }
            
            SendCustomEventDelayedSeconds(nameof(_EndSortCooldown), 10f);
            hasInitialized = true;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            AddUser(player.displayName);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == localPlayer) return;
            RemoveUser(player.displayName);
        }
        
        public void _EndSortCooldown()
        {
            sortCooldownEnabled = false;
            _SortUsers();
        }

        #endregion

        #region Whitelist Updates
        protected void WhitelistUpdated(int i)
        {
            UpdatePermissionState(i);
        }

        protected void UpdatePermissionState(int i)
        {
            var checkConfirmationDialog = showingConfirmationDialogFor == i;
            
            var pmUsersKeys = pmUsers.GetKeys();
            for (int j = 0; j < pmUsersKeys.Count; j++)
            {
                var isWhitelisted = whitelists[i]._IsPlayerWhitelisted(pmUsersKeys[j].String);

                if (checkConfirmationDialog &&
                    isAddConfirmationDialog == isWhitelisted &&
                    pmUsersKeys[j].String == confirmationDialogUsername)
                {
                    HideConfirmationDialog();
                }
                
                if (pmUsers.TryGetValue(pmUsersKeys[j], TokenType.Reference, out DataToken value))
                {
                    var pmUserScript = (PermissionManagerUser)value.Reference;
                    pmUserScript._UpdatePermissionState(i, isWhitelisted);
                }
            }
        }

        public void _WhitelistUpdated0() => WhitelistUpdated(0);
        public void _WhitelistUpdated1() => WhitelistUpdated(1);
        public void _WhitelistUpdated2() => WhitelistUpdated(2);
        public void _WhitelistUpdated3() => WhitelistUpdated(3);
        public void _WhitelistUpdated4() => WhitelistUpdated(4);
        public void _WhitelistUpdated5() => WhitelistUpdated(5);
        public void _WhitelistUpdated6() => WhitelistUpdated(6);
        public void _WhitelistUpdated7() => WhitelistUpdated(7);
        public void _WhitelistUpdated8() => WhitelistUpdated(8);
        public void _WhitelistUpdated9() => WhitelistUpdated(9);
        public void _WhitelistUpdated10() => WhitelistUpdated(10);
        public void _WhitelistUpdated11() => WhitelistUpdated(11);
        public void _WhitelistUpdated12() => WhitelistUpdated(12);
        public void _WhitelistUpdated13() => WhitelistUpdated(13);
        public void _WhitelistUpdated14() => WhitelistUpdated(14);
        public void _WhitelistUpdated15() => WhitelistUpdated(15);
        

        #endregion
        
        #region Admin Whitelist Updates
        protected void AdminWhitelistUpdated(int i)
        {
            UpdatePermissionAccess(i, adminWhitelists[i]._IsPlayerWhitelisted(localDisplayName));
        }

        protected void UpdatePermissionAccess(int i, bool access)
        {
            if (hasAccess[i] == access) return;
            hasAccess[i] = access;

            if (showingConfirmationDialogFor == i && !access)
                CancelChange();
            
            var pmUsersKeys = pmUsers.GetKeys();
            for (int j = 0; j < pmUsersKeys.Count; j++)
            {
                if (pmUsers.TryGetValue(pmUsersKeys[j], TokenType.Reference, out DataToken value))
                {
                    var pmUserScript = (PermissionManagerUser)value.Reference;
                    pmUserScript._UpdatePermissionAccess(i, access);
                }
            }
        }

        public void _AdminWhitelistUpdated0() => AdminWhitelistUpdated(0);
        public void _AdminWhitelistUpdated1() => AdminWhitelistUpdated(1);
        public void _AdminWhitelistUpdated2() => AdminWhitelistUpdated(2);
        public void _AdminWhitelistUpdated3() => AdminWhitelistUpdated(3);
        public void _AdminWhitelistUpdated4() => AdminWhitelistUpdated(4);
        public void _AdminWhitelistUpdated5() => AdminWhitelistUpdated(5);
        public void _AdminWhitelistUpdated6() => AdminWhitelistUpdated(6);
        public void _AdminWhitelistUpdated7() => AdminWhitelistUpdated(7);
        public void _AdminWhitelistUpdated8() => AdminWhitelistUpdated(8);
        public void _AdminWhitelistUpdated9() => AdminWhitelistUpdated(9);
        public void _AdminWhitelistUpdated10() => AdminWhitelistUpdated(10);
        public void _AdminWhitelistUpdated11() => AdminWhitelistUpdated(11);
        public void _AdminWhitelistUpdated12() => AdminWhitelistUpdated(12);
        public void _AdminWhitelistUpdated13() => AdminWhitelistUpdated(13);
        public void _AdminWhitelistUpdated14() => AdminWhitelistUpdated(14);
        public void _AdminWhitelistUpdated15() => AdminWhitelistUpdated(15);
        #endregion

        #region UI Events
        /// <summary>
        /// Called by Permission Manager users if a button was pressed.
        /// </summary>
        /// <param name="username">User of which the button was pressed.</param>
        /// <param name="i">Index of the permission that had its state changed.</param>
        /// <param name="state">True if this permission was enabled, false when not.</param>
        public void ButtonStateChange(string username, int i, bool state)
        {
            if (!hasAccess[i] || isReadOnly[i]) return;
            var whitelist = (WhitelistSetterBase)whitelists[i];

            if (showConformationDialog[i])
            {
                ShowConformationDialog(username, i, state);
                return;
            }
            
            if (state)
            {
                whitelist._AddUser(username, (IUdonEventReceiver)this, $"_WhitelistUpdated{i}");
            }
            else
            {
                whitelist._RemoveUser(username, (IUdonEventReceiver)this, $"_WhitelistUpdated{i}");
            }
        }

        private void ShowConformationDialog(string username, int i, bool state)
        {
            confirmationDialogUsername = username;
            isAddConfirmationDialog = state;
            showingConfirmationDialogFor = i;
            
            confirmationDialog.SetActive(true);
            var text = state ? confirmationDialogAddString : confirmationDialogRemoveString;
            confirmationDialogText.text = text.Replace("<username>", username).Replace("<permissionName>", permissionNames[i]);
        }
        
        private void HideConfirmationDialog()
        {
            confirmationDialog.SetActive(false);
            showingConfirmationDialogFor = -1;
        }

        public void ConfirmChange()
        {
            var i = showingConfirmationDialogFor;
            if (i == -1) return;
            
            if (!hasAccess[i] || isReadOnly[i]) return;
            var whitelist = (WhitelistSetterBase)whitelists[i];
            
            if (isAddConfirmationDialog)
            {
                whitelist._AddUser(confirmationDialogUsername, (IUdonEventReceiver)this, $"_WhitelistUpdated{i}");
            }
            else
            {
                whitelist._RemoveUser(confirmationDialogUsername, (IUdonEventReceiver)this, $"_WhitelistUpdated{i}");
            }

            HideConfirmationDialog();
        }
        
        public void CancelChange()
        {
            var i = showingConfirmationDialogFor;
            if (i == -1) return;
            
            // Revert the toggle state
            if (pmUsers.TryGetValue(confirmationDialogUsername, TokenType.Reference, out DataToken value))
            {
                var pmUserScript = (PermissionManagerUser)value.Reference;
                pmUserScript._UpdatePermissionState(i, !isAddConfirmationDialog);
            }
            HideConfirmationDialog();
        }
        #endregion

        #region Adding / Removing Users
        private void AddUser(string username)
        {
            if (!hasInitialized || pmUsers.ContainsKey(username)) return;

            var newPmUserObj = Instantiate(pmUserPrefab, pmUserTransform.position, pmUserTransform.rotation, pmUserTransform);
            newPmUserObj.name = username + " | PM User";
            newPmUserObj.transform.localScale = Vector3.one;
            newPmUserObj.SetActive(true);

            var hasPermission = new bool[permissions];
            for (int i = 0; i < permissions; i++)
            {
                hasPermission[i] = whitelists[i]._IsPlayerWhitelisted(username);
            }
            
            var newPmUser = newPmUserObj.GetComponent<PermissionManagerUser>();
            newPmUser._Setup(this, permissions, hasAccess, hasPermission, username, allowTeleport);
            
            pmUsers.Add(username, newPmUser);
            
            if (autoSort && !sortCooldownEnabled) _SortUsers();
        }

        private void RemoveUser(string username)
        {
            if (!hasInitialized || !pmUsers.ContainsKey(username)) return;

            if (pmUsers.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var pmUser = (PermissionManagerUser)value.Reference;
                pmUser._Destroy();
                pmUsers.Remove(username);
            }
        }

        /// <summary>
        /// Sorts the users alphabetically.
        /// </summary>
        public void _SortUsers()
        {
            var userKeys = pmUsers.GetKeys();
            userKeys.Sort();

            for (int i = 0; i < userKeys.Count; i++)
                ((PermissionManagerUser)pmUsers[userKeys[i]].Reference).transform.SetSiblingIndex(i + 2); // +2 as the first two elements are placeholder objects
        }
        #endregion
    }
}
