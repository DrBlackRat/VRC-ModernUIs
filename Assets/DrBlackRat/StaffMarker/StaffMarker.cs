
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StaffMarker : UdonSharpBehaviour
    {
        [Header("Settings")]
        [Tooltip("Default enabled state of the Marker, should match the default on the Selector UI.")]
        [SerializeField] private bool hasEnabled;
        [Tooltip("Whitelist a player needs to be on to be able to use this.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("Data Key that is used on the toggle. Expects an int that can either be 0 or 1.")]
        [SerializeField] private string toggleDataKey;
        
        [Header("Marker")]
        [Range(0f, 5f)] [Tooltip("Height above a player's head the Marker should be placed at.")]
        [SerializeField] private float heightAboveHead = 1f;
        [Tooltip("Marker Object that should be enabled / disabled. Should be a child of this Player Object.")]
        [SerializeField] private GameObject markerObject;

        private VRCPlayerApi owner;
        private bool hasAccess;  // If player is whitelisted
        private bool showMarker; // If marker should be enabled and move with players head

        private void Start()
        {
            owner = Networking.GetOwner(gameObject);
            UpdateShowMarker(true);
            
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(owner);
            UpdateShowMarker();
        }

        public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
        {
            if (player != owner) return;
            foreach (var info in infos)
            {
                if (info.Key == toggleDataKey && info.State != PlayerData.State.Unchanged)
                {
                    if (PlayerData.TryGetInt(player, toggleDataKey, out int value))
                    {
                        hasEnabled = value == 1;
                        UpdateShowMarker();
                    }
                    return;
                }
            }
        }

        private void UpdateShowMarker(bool skipSameCheck = false)
        {
            Debug.LogError($"Enabled: {hasEnabled} Access: {hasAccess}");
            bool newShowMarker = hasEnabled && hasAccess;
            if (newShowMarker == showMarker && !skipSameCheck) return;
            showMarker = newShowMarker;
            
            markerObject.SetActive(showMarker);
        }

        public override void PostLateUpdate()
        {
            if (!showMarker) return;
            transform.position = owner.GetBonePosition(HumanBodyBones.Head) + Vector3.up * heightAboveHead;
        }
    }
}

