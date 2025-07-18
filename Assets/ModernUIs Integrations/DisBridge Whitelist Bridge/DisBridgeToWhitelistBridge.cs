
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Helpers;
using VRC.SDK3.Data;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(BasicWhitelist))]
    public class DisBridgeToWhitelistBridge : DisBridgePlugin
    {
        private BasicWhitelist whitelist;
        
        private void Start()
        {
            whitelist = gameObject.GetComponent<BasicWhitelist>();
            disBridge.AddPlugin(this);
        }

        //Runs when DisBridge finishes pulling the roles.
        public override void _UVR_Init()
        {
            
        }
        
        //Runs when a RoleContainer's user list has updated.
        public override void _UVR_Update()
        {

        }

        public override void _UVR_UserJoined(VRCPlayerApi _player)
        {
            whitelist._AddUser(_player.displayName);
        }
        
        public override void _UVR_UserLeft(VRCPlayerApi _player)
        {
            whitelist._RemoveUser(_player.displayName);
        }
    }
}
