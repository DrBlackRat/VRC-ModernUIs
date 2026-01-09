using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.Hideout
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistMute : UdonSharpBehaviour
    {
        [Header("Whitelist:")]
        [Tooltip("Players who should be muted.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("If enabled everyone who is NOT on the whitelist will be muted.")]
        [SerializeField] private bool invertWhitelist;
        
        [Header("Settings:")]
        [Tooltip("The default gain that should be applied once unmuted.")]
        [SerializeField] public float defaultGain = 15f;
        [Tooltip("The default near distance that should be applied once unmuted.")]
        [SerializeField] private float defaultNearDistance = 0f;
        [Tooltip("The default far distance that should be applied once unmuted.")]
        [SerializeField] private float defaultFarDistance = 25f;
        
        /// <summary>
        /// Gain used when not muted.
        /// </summary>
        public float DefaultGain
        {
            get => defaultGain;
            set => defaultGain = Mathf.Clamp(value, 0f, 24f);
        }

        /// <summary>
        /// Near distance used when not muted.
        /// </summary>
        public float DefaultNearDistance
        {
            get => defaultNearDistance;
            set => defaultNearDistance = Mathf.Clamp(value, 0f, 1000000f);
        }

        /// <summary>
        /// Far distance used when not muted.
        /// </summary>
        public float DefaultFarDistance
        {
            get => defaultFarDistance;
            set => defaultFarDistance = Mathf.Clamp(value, 0f, 1000000f);
        }

        private bool initialized;
        
        private void Start()
        {
            whitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_UpdateMuteState));
            initialized = true;
        }

        private void OnEnable()
        {
            if (!initialized) return;
            _UpdateMuteState();
        }

        private void OnDisable()
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            
            foreach (var player in players)
            {
                player.SetVoiceGain(defaultGain);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!initialized) return;
            ApplyMute(player);
        }
        
        public void _UpdateMuteState()
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            
            foreach (var player in players)
            {
                ApplyMute(player);
            }
        }
        
        private void ApplyMute(VRCPlayerApi player)
        {
            if (!player.IsValid()) return;
            bool shouldMute = invertWhitelist ? !whitelist._IsPlayerWhitelisted(player) : whitelist._IsPlayerWhitelisted(player);
            
            player.SetVoiceGain(shouldMute ? 0f : defaultGain);
            player.SetVoiceDistanceFar(shouldMute ? 0f : defaultFarDistance);
            player.SetVoiceDistanceNear(shouldMute ? 0f : defaultNearDistance);
        }
    }
}
