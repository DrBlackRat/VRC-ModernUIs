
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(Collider))]
    public class PlayerZoneConstraint : UdonSharpBehaviour
    {
        // General
        [SerializeField] 
        [Tooltip("Mode of the Whitelist Zone.\n\n" +
                 "Keep Out: keep player outside the zone\n" +
                 "Keep In: keep player inside the zone")]
        private ZoneConstraintMode mode = ZoneConstraintMode.KeepOut;
        [SerializeField]
        [FormerlySerializedAs("exitTransform")]
        [Tooltip("Transform a player will be teleported to if they breaches the zone mode check.\nNeeds to be outside the zone for Keep Out and inside the zone for Keep In!")]
        private Transform teleportTransform;
        
        // Whitelist
        [SerializeField]
        [Tooltip("Optional Whitelist that can be used to to limit who is kept in / out of the zone.")]
        private WhitelistGetterBase whitelist;
        [SerializeField]
        [Tooltip("Determines how the whitelist is interpreted.\n\n" +
                 "Exempt: Whitelisted players are exempt from this zone constraint.\n" +
                 "Include: Only whitelisted players are affected by this zone constraint.")]
        private ZoneConstraintWhitelistMode whitelistMode = ZoneConstraintWhitelistMode.Exempt;
        
        // Advanced Protection
        [SerializeField]
        [Tooltip("Checks if the players head is inside the bounds manually instead of relying on triggers. \nThis is more expensive but prevents things like stations or play space moving getting around triggers.")]
        private bool advancedProtection;
        [SerializeField]
        [Range(0.1f, 5f)]
        [Tooltip("Time in seconds after which the advanced protection check for entry again.")]
        private float advancedUpdateInterval = 0.5f;
        
        // Events
        [SerializeField]
        [Tooltip("Udon Behaviours that will be notified when the player breaches the zone check.\nCan be used to e.g. send a notification.")]
        private UdonBehaviour[] zoneBreachEventReceivers;
        [SerializeField]
        [Tooltip("Name of the event that will be fired on the Zone Breach Event Receivers when the player breaches the zone mode check.")]
        private string zoneBreachEventName = "_OnZoneBreached";

        // Internal
        private Collider zoneCollider;
        private VRCPlayerApi localPlayer;
        
        private VRCTweenHandle advancedUpdateEventHandle;
        private bool hasInitialized;
        private bool hasFinishedFirstCheck;
        
        private bool isWhitelisted;
        private bool inZone;
        
        #region Unity, VRChat & ModernUIs Events
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            localPlayer = Networking.LocalPlayer;
            
            if (whitelist != null)
            {
                whitelist._SetUpConnection(this, nameof(_WhitelistUpdated));
            }

            StartChecks();
            hasInitialized = true;
        }

        private void OnEnable()
        {
            if (!hasInitialized) 
                return;

            StartChecks();
        }

        private void StartChecks()
        {
            if (advancedProtection)
            {
                _AdvancedCheckUpdate();
            }
            else
            {
                SendCustomEventDelayedFrames(nameof(_CheckForZoneBreach), 5);
            }
        }

        private void OnDisable()
        {
            inZone = false;
            advancedUpdateEventHandle.Kill();
        }

        public void _WhitelistUpdated()
        {
            isWhitelisted = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            
            if (hasFinishedFirstCheck)
                _CheckForZoneBreach();
        }
        #endregion
        
        #region Zone Breach Checks
        public void _CheckForZoneBreach()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            switch (mode)
            {
                case ZoneConstraintMode.KeepOut:
                    if (inZone && !IsLocalPlayerAllowed())
                    {
                        PlayerIsBreachingZoneCheck();
                    }
                    break;
                case ZoneConstraintMode.KeepIn:
                    if (!inZone && !IsLocalPlayerAllowed())
                    {
                        PlayerIsBreachingZoneCheck();
                    }
                    break;
                default:
                    MUIDebug.LogError($"Unknown Mode: {mode} on {gameObject} zone!", gameObject);
                    break;
            }
            
            hasFinishedFirstCheck = true;
        }

        private bool IsLocalPlayerAllowed()
        {
            if (whitelist == null)
            {
                return false;
            }

            switch (whitelistMode)
            {
                case ZoneConstraintWhitelistMode.Exempt:
                    return isWhitelisted;
                case ZoneConstraintWhitelistMode.Include:
                    return !isWhitelisted;
                default:
                    return false;
            }
        }

        private void PlayerIsBreachingZoneCheck()
        {
            TeleportPlayer();
            SendZoneBreachEvent();
        }

        private void TeleportPlayer()
        {
            if (!IsTeleportPositionValid())
            {
                return;
            }
            
            MUIDebug.Log($"You are breaching the zone check of the {gameObject.name} zone! Teleporting...", gameObject);
            Networking.LocalPlayer.TeleportTo(teleportTransform.position, teleportTransform.rotation);
        }

        private void SendZoneBreachEvent()
        {
            foreach (var behaviour in zoneBreachEventReceivers)
            {
                if (behaviour == null) continue;
                behaviour.SendCustomEvent(zoneBreachEventName);
            }
        }
        
        private bool IsTeleportPositionValid()
        {
            if (mode == ZoneConstraintMode.KeepOut && zoneCollider.bounds.Contains(teleportTransform.position))
            {
                MUIDebug.LogError($"Could not teleport player on {gameObject.name} zone as the teleport position is inside the zone!\n" +
                                  $"In Keep Out mode this would cause the player to get stuck, please move the teleport position outside the zone or change the mode!", gameObject);
                return false;
            }

            if (mode == ZoneConstraintMode.KeepIn && !zoneCollider.bounds.Contains(teleportTransform.position))
            {
                MUIDebug.LogError($"Could not teleport player on {gameObject.name} zone as the teleport position is outside the zone!\n" +
                                  $"In Keep In mode this would cause the player to get stuck, please move the teleport position inside the zone or change the mode!", gameObject);
                return false;
            }

            return true;
        }
        #endregion

        #region Advanced Protection
        public void _AdvancedCheckUpdate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            inZone = zoneCollider.bounds.Contains(headPos);
            _CheckForZoneBreach();

            advancedUpdateEventHandle =
                VRCTween.DelayedCall(this, nameof(_AdvancedCheckUpdate), advancedUpdateInterval);
        }
        #endregion
        
        #region Simple Protection
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (advancedProtection ||
                !Utilities.IsValid(player) || 
                !player.IsValid() ||
                !player.isLocal)
            {
                return;
            }           
            
            inZone = true;
            _CheckForZoneBreach();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (advancedProtection ||
                !Utilities.IsValid(player) || 
                !player.IsValid() ||
                !player.isLocal)
            {
                return;
            }   
            
            inZone = false;
            _CheckForZoneBreach();
        }
        #endregion
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            gameObject.layer = 0;
        }
#endif
        
#if UNITY_EDITOR
        // Fix spawning inside the collider being broken in client sim
        private Collider _collider;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                return;
            }
            
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                return;
            }
            
            _collider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 1);
        }

        public void _EnableCollider()
        {
            _collider.enabled = true;
        }
#endif
    }
    
    internal enum ZoneConstraintMode
    {
        KeepOut,
        KeepIn,
    }

    internal enum ZoneConstraintWhitelistMode
    {
        Exempt,
        Include,
    }
    
    
}

