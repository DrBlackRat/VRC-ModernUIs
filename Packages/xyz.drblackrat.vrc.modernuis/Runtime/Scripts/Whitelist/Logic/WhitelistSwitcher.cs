using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistSwitcher : WhitelistGetterBase
    {
        [SerializeField]
        [Tooltip("Whitelists to switch between, whitelist 0 will be used as the default.")]
        private WhitelistGetterBase[] whitelists;
        
        private int _currentWhitelistIndex;
        private const int MaxWhitelists = 16;
        
        #region Unity Events

        protected override void Start()
        {
            base.Start();

            if (whitelists.Length >= MaxWhitelists)
            {
                MUIDebug.LogError($"Whitelist Switcher: Can not have more than {MaxWhitelists} whitelists on {gameObject.name}!", gameObject);
                return;
            }
            
            for (int i = 0; i < whitelists.Length; i++)
            {
                whitelists[i]._SetUpConnection(this, $"_OnWhitelistUpdated{i}");
            }
        }
        #endregion
        
        #region Public API
        [HideInInspector]
        public int whitelistIndex;
        
        /// <summary>
        /// Switches the current whitelist to the one at the provided index.
        /// </summary>
        public void _SwitchWhitelist()
        {
            if (whitelistIndex >= whitelists.Length || whitelistIndex < 0)
            {
                MUIDebug.LogError($"Whitelist Switcher: Can not switch to index {whitelistIndex} on {gameObject.name}, as it's out of bounds!", gameObject);
                return;
            }

            if (debugLogging)
            {
                MUIDebug.Log($"Whitelist Switcher: Switched to index {whitelistIndex} on {gameObject.name}.", gameObject);
            }

            _currentWhitelistIndex = whitelistIndex;
            WhitelistUpdated();
        }
        #endregion
        
        #region Whitelist Updates
        public void _OnWhitelistUpdated0() => OnWhitelistUpdated(0);
        public void _OnWhitelistUpdated1() => OnWhitelistUpdated(1);
        public void _OnWhitelistUpdated2() => OnWhitelistUpdated(2);
        public void _OnWhitelistUpdated3() => OnWhitelistUpdated(3);
        public void _OnWhitelistUpdated4() => OnWhitelistUpdated(4);
        public void _OnWhitelistUpdated5() => OnWhitelistUpdated(5);
        public void _OnWhitelistUpdated6() => OnWhitelistUpdated(6);
        public void _OnWhitelistUpdated7() => OnWhitelistUpdated(7);
        public void _OnWhitelistUpdated8() => OnWhitelistUpdated(8);
        public void _OnWhitelistUpdated9() => OnWhitelistUpdated(9);
        public void _OnWhitelistUpdated10() => OnWhitelistUpdated(10);
        public void _OnWhitelistUpdated11() => OnWhitelistUpdated(11);
        public void _OnWhitelistUpdated12() => OnWhitelistUpdated(12);
        public void _OnWhitelistUpdated13() => OnWhitelistUpdated(13);
        public void _OnWhitelistUpdated14() => OnWhitelistUpdated(14);
        public void _OnWhitelistUpdated15() => OnWhitelistUpdated(15);
        
        private void OnWhitelistUpdated(int index)
        {
            if (index != _currentWhitelistIndex)
            {
                return;
            }
            
            WhitelistUpdated();
        }
        #endregion
        
        #region Whitelist Checks Overrides
        public override bool _IsPlayerWhitelisted(string username) => whitelists[_currentWhitelistIndex]._IsPlayerWhitelisted(username);
        public override bool _IsWhitelistPlayerPresent(string username) => whitelists[_currentWhitelistIndex]._IsWhitelistPlayerPresent();
        public override string _GetNamesFormatted() => whitelists[_currentWhitelistIndex]._GetNamesFormatted();
        public override string[] _GetUsersAsArray() => whitelists[_currentWhitelistIndex]._GetUsersAsArray();
        public override DataList _GetUsersAsList() => whitelists[_currentWhitelistIndex]._GetUsersAsList();
        public override int _GetCount() => whitelists[_currentWhitelistIndex]._GetCount();
        #endregion

        #region Added By Checks Overrides
        public override DataList _GetUsersAddedBy(string username) => whitelists[_currentWhitelistIndex]._GetUsersAddedBy(username);
        public override int _GetUsersAddedByCount(string username) => whitelists[_currentWhitelistIndex]._GetUsersAddedByCount(username);
        public override string _GetWhoAdded(string username) => whitelists[_currentWhitelistIndex]._GetWhoAdded(username);
        #endregion

        #region Getting Players Overrides
        public override VRCPlayerApi _GetPlayerInInstance(string playerNameToExclude) => whitelists[_currentWhitelistIndex]._GetPlayerInInstance(playerNameToExclude);
        #endregion
    }
}
