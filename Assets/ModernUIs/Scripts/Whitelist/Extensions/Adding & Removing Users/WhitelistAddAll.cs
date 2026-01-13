
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistAddAll : WhitelistButtonBase
    {
        [Tooltip("Whitelist of which to add all players to.")]
        [SerializeField] private WhitelistSetterBase whitelist;
        protected override void WhitelistButtonPressed()
        {
            if (whitelist == null)
            {
                MUIDebug.LogError("Whitelist Add All | Could not add players, no whitelist provided!", gameObject);
                return;
            }

            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            
            whitelist._AddUsers(players);
        }
    }
}
