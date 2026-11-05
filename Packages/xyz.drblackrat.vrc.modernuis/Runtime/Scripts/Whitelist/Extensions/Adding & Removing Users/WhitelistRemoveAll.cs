
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistRemoveAll : WhitelistButtonBase
    {
        [Tooltip("Whitelist of which to remove all players from.")]
        [SerializeField] private WhitelistSetterBase whitelist;
        protected override void WhitelistButtonPressed()
        {
            if (whitelist == null)
            {
                MUIDebug.LogError("Whitelist Remove All | Could not remove players, no whitelist provided!", gameObject);
                return;
            }
            
            whitelist._ClearWhitelist();
        }
    }
}
