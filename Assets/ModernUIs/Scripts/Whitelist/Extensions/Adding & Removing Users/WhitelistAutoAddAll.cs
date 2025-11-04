
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WhitelistAutoAddAll : UdonSharpBehaviour
{
    [Tooltip("Whitelist Setter to add all users to.")]
    [SerializeField] private WhitelistSetterBase whitelist;
    [Tooltip("If enabled the Whitelist will only be added to if the local player is the owner of it.\nWill be force enabled for synced whitelists.")]
    [SerializeField] private bool onlyAddIfOwner;

    private void Start()
    {
        
    }
}
