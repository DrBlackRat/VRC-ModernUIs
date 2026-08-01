
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    /// <summary>
    /// Basic usable whitelist, only supports <see cref="WhitelistSetterBase"/> functionality.
    /// Should only be used in combination with other scripts that are setting the whitelist.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BasicWhitelist : WhitelistSetterBase
    {
        
    }
}
