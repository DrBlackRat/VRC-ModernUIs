
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Notifications
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CustomNetNotificationSenderPresetSelectorInstance : UdonSharpBehaviour
    {
        [HideInInspector] public CustomNetNotificationSender customNetNotificationSender;
        [HideInInspector] public WhitelistGetterBase whitelist;
        
        [HideInInspector] public string[] headers;
        [HideInInspector] public string[] messages;
        [HideInInspector] public int[] iconIds;
        [HideInInspector] public int[] soundIds;
        [HideInInspector] public float[] durations;

        private VRCPlayerApi localPlayer;
        private bool isWhitelisted;
        
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            if (whitelist != null)
            {
                whitelist._SetUpConnection((IUdonEventReceiver)this);
            } else
            {
                isWhitelisted = true;
            }
        }

        public void _WhitelistUpdated()
        {
            isWhitelisted = whitelist._IsPlayerWhitelisted(localPlayer);
        }
        
        [HideInInspector] public int selectionId;
        public void _SelectionChanged()
        {
            _ApplyPreset(selectionId);
        }
        
        /// <summary>
        /// Applies a preset to the Custom Net Notification Sender.
        /// </summary>
        /// <param name="id">int | ID of the preset to apply.</param>
        /// <returns>True if successful, false if not.</returns>
        public bool _ApplyPreset(int id)
        {
            if (id < 0 || id >= headers.Length) return false;
            if (customNetNotificationSender == null) return false;
            if (!isWhitelisted) return false;
            
            customNetNotificationSender._ApplyValues(headers[id], messages[id], iconIds[id], soundIds[id], durations[id]);
            return true;
        }
    }
}
