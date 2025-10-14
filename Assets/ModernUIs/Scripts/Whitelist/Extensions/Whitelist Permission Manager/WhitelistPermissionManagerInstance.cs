using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
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
        [HideInInspector] public WhitelistGetterBase[] whitelists;
        [HideInInspector] public WhitelistGetterBase[] adminWhitelists;
        [HideInInspector] public bool[] readOnly;
        
        // PM = PermissionManager
        [HideInInspector] public GameObject pmUserPrefab;
        [HideInInspector] public Transform pmUserTransform;
        
        protected DataDictionary pmUsers = new DataDictionary(); // Key: username, Value: Reference to the Permission Manager User
        protected bool hasInitialized;
        protected int permissions;
        
        protected VRCPlayerApi localPlayer;
        protected string localDisplayName;
        
        protected bool[] hasAccess;

        private const int MAX_WHITELISTS = 8; // Currently set to 8 due to UI limitations, actual max is 8
        
        #region Unity & VRChat Events
        private void Start()
        {
            if (whitelists.Length > MAX_WHITELISTS || adminWhitelists.Length > MAX_WHITELISTS || readOnly.Length > MAX_WHITELISTS)
            {
                MUIDebug.LogError($"Permission Manager | Only a max of {MAX_WHITELISTS} Whitelists can be used!");
                return;
            }

            if (whitelists.Length != adminWhitelists.Length || whitelists.Length != readOnly.Length)
            {
                MUIDebug.LogError($"Permission Manager | There needs to be an equal amount of Whitelists, Admin Whitelists and Read Only toggles!");
                return;
            }

            permissions = whitelists.Length;
            hasAccess = new bool[permissions]; ;
            
            localPlayer = Networking.LocalPlayer;
            localDisplayName = localPlayer.displayName;

            for (int i = 0; i < permissions; i++)
            {
                var whitelist = whitelists[i];
                var adminWhitelist = adminWhitelists[i];
                
                readOnly[i] = whitelist._IsReadOnly() || readOnly[i];
                
                whitelist._SetUpConnection((IUdonEventReceiver)this, $"_WhitelistUpdated{i}");

                if (!readOnly[i])
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
        #endregion

        #region Whitelist Updates
        protected void WhitelistUpdated(int i)
        {
            UpdatePermissionState(i);
        }

        protected void UpdatePermissionState(int i)
        {
            var pmUsersKeys = pmUsers.GetKeys();
            for (int j = 0; j < pmUsersKeys.Count; j++)
            {
                var isWhitelisted = whitelists[i]._IsPlayerWhitelisted(pmUsersKeys[j].String);
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

        /// <summary>
        /// Called by Permission Manager users if a button was pressed.
        /// </summary>
        /// <param name="username">User of which the button was pressed.</param>
        /// <param name="i">Index of the permission that had its state changed.</param>
        /// <param name="state">True if this permission was enabled, false when not.</param>
        public void ButtonStateChange(string username, int i, bool state)
        {
            if (!hasAccess[i] || readOnly[i]) return;
            var whitelist = (WhitelistSetterBase)whitelists[i];
            
            if (state)
            {
                whitelist._AddUser(username, (IUdonEventReceiver)this, $"_WhitelistUpdated{i}");
            }
            else
            {
                whitelist._RemoveUser(username, (IUdonEventReceiver)this, $"_WhitelistUpdated{i}");
            }
        }

        private void AddUser(string username)
        {
            if (!hasInitialized || pmUsers.ContainsKey(username)) return;

            var newPmUserObj = Instantiate(pmUserPrefab, pmUserTransform.position, pmUserTransform.rotation, pmUserTransform);
            newPmUserObj.transform.localScale = Vector3.one;
            newPmUserObj.SetActive(true);

            var hasPermission = new bool[permissions];
            for (int i = 0; i < permissions; i++)
            {
                hasPermission[i] = whitelists[i]._IsPlayerWhitelisted(username);
            }
            
            var newPmUser = newPmUserObj.GetComponent<PermissionManagerUser>();
            newPmUser._Setup(this, permissions, hasAccess, hasPermission, username);
            
            pmUsers.Add(username, newPmUser);
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
    }
}
