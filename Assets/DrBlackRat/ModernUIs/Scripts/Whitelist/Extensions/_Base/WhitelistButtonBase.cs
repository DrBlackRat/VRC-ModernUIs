
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist.Base
{
    /// <summary>
    /// Abstract base class for a button that supports whitelist restrictions.
    /// The button can only be used by whitelisted users and can update its text 
    /// dynamically based on the user's whitelist status.
    /// Derived classes should call <see cref="_ButtonPressed"/> when the button is pressed.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistButtonBase : UdonSharpBehaviour
    {
        [Tooltip("Whitelist the Local Player needs to be on to press this button. If not provided everyone is considered whitelisted.")]
        [SerializeField] protected WhitelistGetterBase accessWhitelist;
        [Tooltip("If enabled the whitelist will be inverted.")]
        [SerializeField] protected bool invertAccessWhitelist;
        
        [Tooltip("Button of which the intractability will be changed depending on if the player is whitelisted or not.")]
        [SerializeField] private Button button;
        [Tooltip("Game Object that is enabled when whitelisted, usually used for text.")]
        [SerializeField] private GameObject whitelistedTextObj;
        [Tooltip("Game Object that is disabled when whitelisted, usually used for text.")]
        [SerializeField] private GameObject notWhitelistedTextObj;
        protected bool IsWhitelisted { get; private set; }
        protected VRCPlayerApi LocalPlayer { get; private set; }

        private void Start()
        {
            LocalPlayer = Networking.LocalPlayer;
            if (accessWhitelist != null)
            {
                accessWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_WhitelistUpdated));
            }
            else
            {
                IsWhitelisted = true;
            }
            
            UpdateUI();
            Init();
        }

        public void _WhitelistUpdated()
        {
            if (accessWhitelist == null) return;
            
            var whitelisted = accessWhitelist._IsPlayerWhitelisted(LocalPlayer);
            IsWhitelisted = invertAccessWhitelist ? !whitelisted : whitelisted;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (whitelistedTextObj != null) whitelistedTextObj.SetActive(IsWhitelisted);
            if (notWhitelistedTextObj != null) notWhitelistedTextObj.SetActive(!IsWhitelisted);
            button.interactable = IsWhitelisted;
        }

        /// <summary>
        /// Should be invoked when the button is pressed.
        /// Checks if the local player is whitelisted and then calls <see cref="WhitelistButtonPressed"/>.
        /// </summary>
        public void _ButtonPressed()
        {
            if (!IsWhitelisted)
            {
                MUIDebug.LogError("Whitelist Button | Tried to call button event without being whitelisted!");
                return;
            }
            WhitelistButtonPressed();
        }

        /// <summary>
        /// Called after <c>Start()</c> has completed.
        /// Intended to be used as the initialization entry point for derived classes.
        /// </summary>
        protected virtual void Init()
        {
            
        }
        
        /// <summary>
        /// Called when the button is pressed and the local player is whitelisted.
        /// </summary>
        protected virtual void WhitelistButtonPressed()
        {
            
        }
    }
}
