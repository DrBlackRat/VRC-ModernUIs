using System;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistSelectorUI : SelectorUI
    {

        [Range(0f, 30f)] [Tooltip("Persistence Load delay, useful if you need to wait for the Whitelist to finish updating etc.")]
        [SerializeField] protected float loadDelay = 5f;
        
        [FormerlySerializedAs("whitelistManager")] 
        [SerializeField] [Tooltip("Whitelist that is storing info on which user is whitelisted. If left empty, the whitelist wont be used.")]
        protected WhitelistGetterBase whitelist;
        [Tooltip("Color for the selector if a user is whitelisted.")]
        [SerializeField] protected Color whitelistedColor;
        [Tooltip("Color for the selector if a user NOT is whitelisted.")]
        [SerializeField] protected Color notWhitelistedColor;

        public Color WhitelistedColor
        {
            get => whitelistedColor;
            set => whitelistedColor = value;
        }

        public Color NotWhitelistedColor
        {
            get => notWhitelistedColor;
            set => notWhitelistedColor = value;
        }

        protected bool hasAccess;

        protected override void Start()
        {
            base.Start();
            if (whitelist != null)
            {
                whitelist._SetUpConnection(GetComponent<UdonBehaviour>());
            }
            selector._SetDefaultColors(whitelistedColor, notWhitelistedColor);
            hasAccess = CheckAccess();
            _UpdateInteractable();
        }

        public virtual void _WhitelistUpdated()
        {
            hasAccess = CheckAccess();
            _UpdateInteractable();
        }

        protected virtual bool CheckAccess()
        {
            var access = false;
            if (whitelist == null)
            {
                access = true;
            }
            else
            {
                access = whitelist._IsPlayerWhitelisted(localPlayerApi);
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
            if (!player.isLocal || !usePersistence) return;
            SendCustomEventDelayedSeconds(nameof(_DelayedLoad), loadDelay);
        }

        public virtual void _DelayedLoad()
        {
            if (!hasAccess) return;
            
            int storedSelected = 0;
            if (PlayerData.TryGetBool(localPlayerApi, dataKey, out bool boolValue))
            {
                storedSelected = boolValue ? 1 : 0;
                persistenceLoaded = true;
            }
            else if (PlayerData.TryGetInt(localPlayerApi, dataKey, out int intValue))
            {
                storedSelected = intValue;
                persistenceLoaded = true;
            }

            if (!persistenceLoaded) return;
            if (!selectorUIButtons[storedSelected].Selectable)
            {
                PlayerData.SetInt(dataKey, selectedState);
                return;
            }
            
            UpdateSelection(storedSelected, true, false, false);
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
