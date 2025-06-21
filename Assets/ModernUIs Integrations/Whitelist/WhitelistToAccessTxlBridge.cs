
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Texel;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using VRC.SDK3.Data;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistToAccessTxlBridge : AccessControlUserSource
    {
        [SerializeField] private WhitelistManager whitelistManager;
        private bool connectedToWhitelist = false;
        
        protected override void _Init()
        {
            if (!connectedToWhitelist)
            {
                whitelistManager._SetUpConnection((IUdonEventReceiver)this);
                connectedToWhitelist = true;
                Debug.LogError("AAAAAAAAAAAAAAAAAAAAAAAA");
            }
            base._Init();
        }

        public void _WhitelistUpdated()
        {
            _UpdateHandlers(EVENT_REVALIDATE);
        }

        public override bool _ContainsAnyPlayerInWorld()
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            foreach (var player in players)
            {
                if (whitelistManager._IsPlayerWhitelisted(player)) return true;
            }

            return false;
        }
        
        public override bool _ContainsPlayer(VRCPlayerApi player)
        {
            return whitelistManager._IsPlayerWhitelisted(player);
        }

        public override bool _ContainsName(string name)
        {
            return whitelistManager._IsPlayerWhitelisted(name);
        }
        
        
    }
}

