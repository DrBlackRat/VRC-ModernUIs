
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistEventSender : UdonSharpBehaviour
    {
        [Tooltip("Whitelist to react to local player changes to.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("If enabled the Removed Event will only fire if a player was added and then removed,")]
        [SerializeField] private bool onlyFireOnChange;

        [Tooltip("Udon Behaviours to send the events to.")]
        [SerializeField] private UdonBehaviour[] udonBehaviours;
        [Tooltip("Event that will be fired if the Local Player was added to the Whitelist.")]
        [SerializeField] private string addedEventName = "_AddedToWhitelist";
        [Tooltip("Event that will be fired if the Local Player was removed from the Whitelist.")]
        [SerializeField] private string removedEventName = "_RemovedToWhitelist";

        private bool isWhitelisted;
        private VRCPlayerApi localPlayer;

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            var newIsWhitelisted = whitelist._IsPlayerWhitelisted(localPlayer);
            if (onlyFireOnChange && newIsWhitelisted == isWhitelisted) return;

            isWhitelisted = newIsWhitelisted;
            UpdateExternalConnection(isWhitelisted);
        }
        
        private void UpdateExternalConnection(bool added)
        {
            var eventName = added ? addedEventName : removedEventName;
            if (udonBehaviours != null && udonBehaviours.Length > 0)
            {
                foreach (var behaviour in udonBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SendCustomEvent(eventName);
                }
            }
        }
    }
}

