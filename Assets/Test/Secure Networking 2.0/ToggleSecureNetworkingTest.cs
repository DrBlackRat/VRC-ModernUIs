
using DrBlackRat.VRC.ModernUIs.Base;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Testing
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ToggleSecureNetworkingTest : SecureNetworkingBase
    {
        public Toggle toggle;
        public WhitelistGetterBase whitelist;
        public GameObject toggleObject;
        
        private bool isLocalWhitelisted;
        private bool defaultState;
        private const string NetStateKey = "toggleState";
        
        private const string LogPrefix = "<color=#34c0eb>Toggle</color> | ";
        
        protected override void Start()
        {
            base.Start();
            defaultState = toggle.isOn;
            whitelist._SetUpConnection(this, nameof(_WhitelistUpdated));
        }

        public void _WhitelistUpdated()
        {
            Debug.Log(LogPrefix + "Whitelist Updated");
            isLocalWhitelisted = whitelist._IsPlayerWhitelisted(LocalPlayer);
            MoveAuthoritativeSenderIfNeeded();
        }

        public void _TogglePressed()
        {
            Debug.Log(LogPrefix + "Toggle Pressed");
            SetToggleState(toggle.isOn);
            SendNetworkData();
        }

        private void SetToggleState(bool state)
        {
            Debug.Log(LogPrefix + $"Setting Toggle State: {state}");
            toggle.SetIsOnWithoutNotify(state);
            toggleObject.SetActive(state);
        }
        

        #region Secure Networking
        protected override bool IsAllowedSender(VRCPlayerApi player)
        {
            Debug.Log(LogPrefix + "Is Allowed Sender");
            
            if (player == LocalPlayer)
                return isLocalWhitelisted;
            
            return whitelist._IsPlayerWhitelisted(player);
        }

        protected override VRCPlayerApi GetAllowedSender()
        {
            Debug.Log(LogPrefix + "Getting Allowed Sender");
            return whitelist._GetPlayerInInstance();
        }

        protected override void ApplyNetData(DataDictionary netData)
        {
            Debug.Log(LogPrefix + "Applying Net Data");

            if (netData.TryGetValue(NetStateKey, out DataToken result))
            {
                SetToggleState(result.Boolean);
            } 
            else
            {
                Debug.LogError("Net Data did not contain toggle state!");
            }
        }

        protected override DataDictionary GetDataForSending()
        {
            Debug.Log(LogPrefix + "Getting Data For Sending");
            
            var netDict = new DataDictionary();
            netDict.Add(NetStateKey, toggle.isOn);
            return netDict;
            
        }

        protected override void ApplyDefaultData()
        {
            Debug.Log(LogPrefix + "Applying Default Data");
            
            SetToggleState(defaultState);
        }
        #endregion
    }
}

