using DrBlackRat.VRC.ModernUIs.Whitelist;
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DisBridgeToWhitelistBridge : DisBridgePlugin
    {
        [Tooltip("Basic Whitelist that should have Dis Bridge users added / removed to. Needs to be on a separate Game Object!")]
        public BasicWhitelist whitelist;

        [Tooltip("If enabled Dis Bridge users will be removed from the Whitelist when they leave the instance.")]
        public bool removePlayersOnLeave;
        
        private bool receivedFirstUpdate = false;
        private DataList whitelistedUsers = new DataList();
        
        private void Start()
        {
            disBridge.AddPlugin(this);
        }

        public override void _UVR_Update()
        {
            MUIDebug.Log($"DisBridge on {gameObject.name} Updated!", gameObject);
            
            receivedFirstUpdate = true;
            whitelistedUsers.Clear();
            
            var allPlayers = VRCPlayerApi.GetPlayers();
            foreach (var player in allPlayers)
            {
                if (!_IsMemberInRoles(player))
                    continue;
                
                whitelistedUsers.Add(player.displayName);
            }
            
            SendToWhitelist();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!receivedFirstUpdate)
                return;
            
            if (whitelistedUsers.Contains(player.displayName))
                return;
            
            if (!_IsMemberInRoles(player))
                return;
            
            whitelistedUsers.Add(player.displayName);
            SendToWhitelist();
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!removePlayersOnLeave)
                return;
            
            if (!receivedFirstUpdate)
                return;
            
            if (whitelistedUsers.Remove(player.displayName))
                SendToWhitelist();
        }

        private void SendToWhitelist()
        {
            whitelist._ReplaceWhitelist(whitelistedUsers, null, null, true);
        }
    }
}
