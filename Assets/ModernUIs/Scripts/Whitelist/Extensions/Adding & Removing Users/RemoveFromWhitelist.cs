using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RemoveFromWhitelist : AddToWhitelist
    {
        public override void _InputFinished()
        {
            if (requireWhitelisted && !hasAccess) return;
            whitelistManager._RemoveUser(inputField.text);
            inputField.SetTextWithoutNotify(String.Empty);
        }
    }
}
