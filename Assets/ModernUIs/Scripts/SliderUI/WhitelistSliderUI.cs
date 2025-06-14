using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.Helpers;
using DrBlackRat.VRC.ModernUIs.Whitelist;

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistSliderUI : SliderUI
    {
        [Tooltip("Whitelist Manager that is storing info on which user is whitelisted. If left empty, the whitelist wont be used.")]
        [SerializeField] protected WhitelistManager whitelistManager;
        
        protected bool hasAccess;
        
        protected override void Start()
        {
            base.Start();
            if (whitelistManager != null)
            {
                whitelistManager._SetUpConnection(GetComponent<UdonBehaviour>());
            }
            hasAccess = CheckAccess();
            UpdateInteractable();
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!hasAccess) return;
            base.OnPlayerRestored(player);
        }
        
        public virtual void _WhitelistUpdated()
        {
            hasAccess = CheckAccess();
            UpdateInteractable();
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
        
        protected virtual void UpdateInteractable()
        {
            slider.interactable = hasAccess;
        }

        protected override bool UpdateValue(float newValue, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!hasAccess && !skipSameCheck && !fromNet)
            {
                MUIDebug.LogError("You are not whitelisted!");
                return false;
            }
            return base.UpdateValue(newValue, skipPersistence, skipSameCheck, fromNet);
        }
    }
}
