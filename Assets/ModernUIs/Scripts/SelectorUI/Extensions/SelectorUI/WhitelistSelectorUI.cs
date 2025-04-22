using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistSelectorUI : SelectorUI
    {
        [Header("Whitelist:")] 
        [SerializeField] [Tooltip("Whitelist Manager that is storing info on which user is whitelisted. If left empty, the whitelist wont be used.")]
        protected WhitelistManager whitelistManager;
        [Tooltip("Color for the selector if a user is whitelisted.")]
        [SerializeField] protected Color whitelistedColor;
        [Tooltip("Color for the selector if a user NOT is whitelisted.")]
        [SerializeField] protected Color notWhitelistedColor;

        protected bool hasAccess;
        protected Image selectorImage;

        protected override void Start()
        {
            base.Start();
            if (whitelistManager != null)
            {
                whitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
            }
            if (!selector.overrideDefaults) selector._SetDefaultColors(whitelistedColor, notWhitelistedColor);
            hasAccess = CheckAccess();
            _UpdateInteractable();
        }

        public virtual void _WhitelistUpdated()
        {
            hasAccess = CheckAccess();
            _UpdateInteractable();
        }

        protected bool CheckAccess()
        {
            var access = false;
            if (whitelistManager == null)
            {
                access = true;
            }
            else
            {
                access = whitelistManager._IsPlayerWhitelisted(Networking.LocalPlayer);
            }
            return access;
        }

        protected virtual void _UpdateInteractable()
        {
            foreach (var selectorUIButton in selectorUIButtons)
            {
                selectorUIButton._UpdateLocked(!hasAccess);
            }
            selector._SetEnabled(hasAccess);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!hasAccess) return;
            base.OnPlayerRestored(player);
        }
        
        protected override bool UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!hasAccess && !skipSameCheck && !fromNet)
            {
                MUIDebug.LogError("You are not whitelisted!");
                return false;
            }
            return base.UpdateSelection(newState, skipPersistence, skipSameCheck, fromNet);
        }
    }
}
