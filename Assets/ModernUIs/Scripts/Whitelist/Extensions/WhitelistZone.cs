
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
    [RequireComponent(typeof(BoxCollider))]
    public class WhitelistZone : UdonSharpBehaviour
    {
        [Tooltip("Mode of the Whitelist Zone.\n" +
                 "Keep Out: keep *not* whitelist players outside the zone\n" +
                 "Keep In: keep *not* whitelist players inside the zone\n" +
                 "Keep Out Invert: keep whitelist players outside the zone\n" +
                 "Keep In Invert: keep whitelist players inside the zone")]
        [SerializeField] private WhitelistZoneMode mode = WhitelistZoneMode.KeepOut;
        
        [SerializeField] private WhitelistGetterBase whitelist;
        [FormerlySerializedAs("exitTransform")]
        [Tooltip("Transform a player will be teleported to if they breaches the zone mode check.\nNeeds to be outside the zone for Keep Out modes and inside the zone for Keep In modes!")]
        [SerializeField] private Transform teleportTransform;
        
        [Tooltip("Checks if the players head is inside the bounds manually instead of relying on triggers. \nThis is more expensive but prevents things like stations or play space moving causing issues.")]
        [SerializeField] private bool advancedProtection;
        [Tooltip("Time in seconds after which the advanced protection check for entry again.")]
        [Range(0.1f, 5f)]
        [SerializeField] private float advancedUpdateInterval = 0.5f;
        
        [Tooltip("Udon Behaviours that will be notified when the player breaches the zone check.\nCan be used to e.g. send a notification.")]
        [SerializeField] private UdonBehaviour[] zoneBreachEventReceivers;
        [Tooltip("Name of the event that will be fired on the Zone Breach Event Receivers when the player breaches the zone mode check.")]
        [SerializeField] private string zoneBreachEventName = "_OnZoneBreached";

        private bool isTeleportValid;
        private Collider zoneCollider;
        private VRCPlayerApi localPlayer;
        
        private VRCTweenHandle advancedUpdateEventHandle;
        private bool hasInitialized;
        private bool hasAccess;
        private bool inZone;
        
        #region Unity, VRChat & ModernUIs Events
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            localPlayer = Networking.LocalPlayer;
            isTeleportValid = IsTeleportPositionValid();
            
            if (advancedProtection) 
                _AdvancedCheckUpdate();
            
            if (whitelist == null)
                whitelist._SetUpConnection(this);
            hasInitialized = true;
        }

        private void OnEnable()
        {
            if (!hasInitialized) 
                return;
            
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 1);
            
            if (advancedProtection)
                _AdvancedCheckUpdate();
        }

        private void OnDisable()
        {
            inZone = false;
            advancedUpdateEventHandle.Kill();
        }

        public void _WhitelistUpdated()
        {
            hasAccess = whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);
            CheckForZoneBreach();
        }
        #endregion
        
        #region Zone Breach Checks
        private void CheckForZoneBreach()
        {
            if (!isActiveAndEnabled)
                return;
            
            switch (mode)
            {
                case WhitelistZoneMode.KeepOut:
                    if (inZone && !hasAccess) PlayerIsBreachingZoneCheck();
                    break;
                case WhitelistZoneMode.KeepIn:
                    if (!inZone && !hasAccess) PlayerIsBreachingZoneCheck();
                    break;
                case WhitelistZoneMode.KeepOutInvert:
                    if (inZone && hasAccess) PlayerIsBreachingZoneCheck();
                    break;
                case WhitelistZoneMode.KeepInInvert:
                    if (!inZone && hasAccess) PlayerIsBreachingZoneCheck();
                    break;
                default:
                    MUIDebug.LogError($"Unknown Mode: {mode} on {gameObject} zone!", gameObject);
                    break;
            }
        }

        private void PlayerIsBreachingZoneCheck()
        {
            TeleportPlayer();
            SendZoneBreachEvent();
        }

        private void TeleportPlayer()
        {
            if (!isTeleportValid)
            {
                MUIDebug.LogError($"Teleport position on {gameObject.name} zone is invalid! Please move the teleport position or change the mode!", gameObject);
                return;
            } 
            
            MUIDebug.Log($"You are breaching the zone check of the {gameObject.name} zone! Teleporting...", gameObject);
            Networking.LocalPlayer.TeleportTo(teleportTransform.position, teleportTransform.rotation);
        }

        private void SendZoneBreachEvent()
        {
            if (zoneBreachEventReceivers == null || zoneBreachEventReceivers.Length <= 0) 
                return;
            
            foreach (var behaviour in zoneBreachEventReceivers)
            {
                if (behaviour == null) continue;
                behaviour.SendCustomEvent(zoneBreachEventName);
            }
        }
        
        private bool IsTeleportPositionValid()
        {
            if ((mode == WhitelistZoneMode.KeepOut || mode == WhitelistZoneMode.KeepOutInvert) &&
                zoneCollider.bounds.Contains(teleportTransform.position))
            {
                MUIDebug.LogError($"Can not teleport players on {gameObject.name} zone as the teleport position is inside the zone!\n" +
                                  $"In Keep Out mode this would cause the player to get stuck, please move the teleport position outside the zone or change the mode!", gameObject);
                return false;
            }

            if ((mode == WhitelistZoneMode.KeepIn || mode == WhitelistZoneMode.KeepInInvert) &&
                !zoneCollider.bounds.Contains(teleportTransform.position))
            {
                MUIDebug.LogError($"Can not teleport players on {gameObject.name} zone as the teleport position is outside the zone!\n" +
                                  $"In Keep In mode this would cause the player to get stuck, please move the teleport position inside the zone or change the mode!", gameObject);
                return false;
            }

            return true;
        }
        #endregion

        #region Advanced Protection
        public void _AdvancedCheckUpdate()
        {
            Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            bool newInZone = zoneCollider.bounds.Contains(headPos);

            if (newInZone != inZone)
            {
                inZone = newInZone;
                CheckForZoneBreach();
            }

            advancedUpdateEventHandle =
                VRCTween.DelayedCall(this, nameof(_AdvancedCheckUpdate), advancedUpdateInterval);
        }
        #endregion
        
        #region Simple Protection
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (advancedProtection || !player.isLocal || inZone) return;
            inZone = true;
            CheckForZoneBreach();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (advancedProtection || !player.isLocal) return;
            inZone = false;
            CheckForZoneBreach();
        }
        
        // Spawn / Respawn Fixes
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal || advancedProtection) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!player.isLocal || advancedProtection) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public void _EnableCollider()
        {
            zoneCollider.enabled = true;
        }
        #endregion
    }
    
    internal enum WhitelistZoneMode
    {
        KeepOut,
        KeepIn,
        KeepOutInvert,
        KeepInInvert,
    }
}

