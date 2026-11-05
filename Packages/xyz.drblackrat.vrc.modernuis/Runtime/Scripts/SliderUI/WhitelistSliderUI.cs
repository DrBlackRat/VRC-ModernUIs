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
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistSliderUI : SliderUI
    {
        [Range(0f, 30f)] [Tooltip("Persistence Load delay, useful if you need to wait for the Whitelist to finish updating etc.")]
        [SerializeField] protected float loadDelay = 5f;
        
        [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist that is storing info on which user is whitelisted. If left empty, the whitelist wont be used.")]
        [SerializeField] protected WhitelistGetterBase whitelist;
        
        protected bool hasAccess;
        
        protected override void Start()
        {
            base.Start();
            if (whitelist != null)
            {
                whitelist._SetUpConnection(GetComponent<UdonBehaviour>());
            }
            hasAccess = CheckAccess();
            UpdateInteractable();
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal || !usePersistence) return;
            SendCustomEventDelayedSeconds(nameof(_DelayedLoad), loadDelay);
        }
        
        public virtual void _DelayedLoad()
        {
            if (!hasAccess) return;
            if (PlayerData.TryGetFloat(localPlayerApi, dataKey, out float newValue))
            {
                UpdateValue(newValue, true, false, false);
            }
        }
        
        public virtual void _WhitelistUpdated()
        {
            hasAccess = CheckAccess();
            UpdateInteractable();
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
