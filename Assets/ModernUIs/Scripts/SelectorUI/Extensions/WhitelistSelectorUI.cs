using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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

        protected bool hasAccess;

        protected override void Start()
        {
            base.Start();
            if (whitelistManager != null)
            {
                whitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
            }
            hasAccess = CheckAccess();
        }

        public void _WhitelistUpdated()
        {
            hasAccess = CheckAccess();
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

            foreach (var selectorUIButton in selectorUIButtons)
            {
                selectorUIButton._UpdateInteractable(access);
            }
            return access;
        }

        public override bool _UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!hasAccess && !skipSameCheck && !fromNet) return false;
            return base._UpdateSelection(newState, skipPersistence, skipSameCheck, fromNet);
        }
    }
}
