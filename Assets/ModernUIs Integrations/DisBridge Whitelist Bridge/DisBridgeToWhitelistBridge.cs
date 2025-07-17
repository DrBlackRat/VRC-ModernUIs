
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
    public class DisBridgeToWhitelistBridge : DisBridgePlugin
    {
        private BasicWhitelist whitelist;
        private DataDictionary allPlayers = new DataDictionary();
        private DataList users = new DataList();
        
        private void Start()
        {
            whitelist = gameObject.GetComponent<BasicWhitelist>();
            disBridge.AddPlugin(gameObject);
        }

        //Runs when DisBridge finishes pulling the roles.
        public override void _UVR_Init()
        {
            
        }
        
        //Runs when a RoleContainer's user list has updated.
        public override void _UVR_Update()
        {
            users = new DataList();
            var keys = allPlayers.GetKeys();
            for (int i = 0; i < keys.Count; i++)
            {
                var displayName = keys[i];
                var player = (VRCPlayerApi)allPlayers[displayName].Reference;
                if (_IsMemberInRoles(player))
                {
                    users.Add(displayName);
                }
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var token = new DataToken(player);
            var displayName = player.displayName;
            
            allPlayers.Add(displayName, token);

            if (_IsMemberInRoles(player))
            {
                users.Add(displayName);
                whitelist._ReplaceWhitelist(users);
            }
                
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player.isLocal) return;
            var displayName = player.displayName;
            
            allPlayers.Remove(displayName);

            if (users.Contains(displayName))
            {
                users.Remove(displayName);
                whitelist._ReplaceWhitelist(users);
            }
        }
    }
}
