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

namespace DrBlackRat.VRC.ModernUIs.SliderUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedSliderUI : WhitelistSliderUI
    {
        [Range(1, 10)][Tooltip("Hoe long to wait before retrying deserialization if the network owner is not whitelisted yet.")]
        [SerializeField] protected float deserializationRetryDelay = 5f;
        [Range(1, 10)][Tooltip("How many times to attempt deserialization if the network owner is not whitelisted yet.")]
        [SerializeField] protected int maxDeserializationAttempts = 4;
        
        [UdonSynced] protected float netValue;
        
        protected int deserializationAttempts = 0;
        protected int scheduledDeserializations = 0;
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.IsOwner(gameObject)) return;
            base.OnPlayerRestored(player);
        }

        #region Deserialization
        public override void OnDeserialization()
        {
            deserializationAttempts = 0;
            HandleDeserialization();
        }
        
        public void _DelayedDeserialization()
        {
            scheduledDeserializations--;
            if (scheduledDeserializations > 0) return;
            if (deserializationAttempts == 0) return; // 0th = finished / new start
            HandleDeserialization();
        }

        private void HandleDeserialization()
        {
            deserializationAttempts++;
            
            var owner = Networking.GetOwner(gameObject);
            if (whitelist != null && !whitelist._IsPlayerWhitelisted(owner))
            {
                if (deserializationAttempts >= maxDeserializationAttempts)
                {
                    MUIDebug.LogError($"{owner.displayName} is still not whitelisted! Aborting deserialization on {gameObject.name} after {deserializationAttempts} attempts.", gameObject);
                    deserializationAttempts = 0;
                    return;
                }
                
                MUIDebug.LogWarning($"{owner.displayName} is not whitelisted but tried to send network data on {gameObject.name}. Retrying in {deserializationRetryDelay}s, Attempt {deserializationAttempts} / {maxDeserializationAttempts}.", gameObject);
                scheduledDeserializations++;
                SendCustomEventDelayedSeconds(nameof(_DelayedDeserialization), deserializationRetryDelay);
                return;
            }
            
            if (deserializationAttempts > 1)
                MUIDebug.Log($"{owner.displayName} is now whitelisted! Running Deserialization on {gameObject.name} after {deserializationAttempts} attempts.", gameObject);
            deserializationAttempts = 0;
            UpdateValue(netValue, true, false, true);
        }
        #endregion
        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            CheckOwnership();
        }
        
        private void CheckOwnership()
        {
            if (!Networking.LocalPlayer.isMaster) return;
            var owner = Networking.GetOwner(gameObject);
            
            if (whitelist == null || whitelist._IsPlayerWhitelisted(owner)) return;
            MUIDebug.LogWarning($"{owner.displayName} is the Network Owner of {gameObject.name}, but isn't whitelisted. " +
                                $"Trying to move ownership to a whitelisted player.", gameObject);

            var newOwner = whitelist._GetPlayerInInstance();
            if (newOwner == null)
            {
                MUIDebug.LogWarning($"Could not find a whitelisted player to transfer ownership of {gameObject.name} to. " +
                                    $"Keeping current owner, but synced data may get lost." , gameObject);
                return;
            }
            
            MUIDebug.Log($"Found whitelisted player \"{newOwner.displayName}\" to transfer ownership of {gameObject.name} to.", gameObject);
            Networking.SetOwner(newOwner, gameObject);
        }

        protected override bool UpdateValue(float newValue, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!base.UpdateValue(newValue, skipPersistence, skipSameCheck, fromNet)) return false;
            
            if (!fromNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                netValue = value;
                RequestSerialization();
            }

            return true;
        }
    }
}
