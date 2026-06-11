
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class LockButton : UdonSharpBehaviour
    {
        // Connection
        [SerializeField]
        [Tooltip("The Udon Behaviours that will be receiving the lock state change event, as well as the locking player and the lock state.")]
        private UdonBehaviour[] lockStateReceivers;
        
        // Whitelist
        [SerializeField]
        [Tooltip("(Optional) Whitelist used to limit who can lock / unlock.")]
        private WhitelistGetterBase whitelist;
        
        // Networking
        [SerializeField] 
        [Range(1, 10)]
        [Tooltip("How long to wait before retrying applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected float netRetryDelay = 5f;
        [SerializeField] 
        [Range(1, 10)]
        [Tooltip("How many times to attempt applying net data if the sender is not whitelisted yet. (Only used by Secure Networking)")]
        protected int maxNetRetryAttempts = 4;

        
        // UI
        private bool isLocked;
        private VRCPlayerApi lockingPlayer;
        
        private bool netIsLocked;
        private VRCPlayerApi netLockingPlayer;
        
        // "Interface" these will be called / set on the receiving Udon Behaviors
        private const string LockStateChangeEvent = "_OnLockStateChanged";  // public void _OnLockStateChanged();
        private const string LockingPlayerVariableName = "LockingPlayer";   // [HideInInspector] public VRCPlayerApi LockingPlayer; (Can be null if no longer present)
        private const string LockStateBoolVariableName = "IsLocked";        // [HideInInspector] public bool IsLocked;
        
        protected const int NET_MAX_QUEUE_SIZE = 5; // Max number of network events that can be queued at once
        protected const float NET_SEND_DELAY = 0.5f; // Delay between trying to send network events if there are already ones queued up
        
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == lockingPlayer)
                Unlock();
        }

        public void _OnLockButtonPressed()
        {
            if (isLocked)
            {
                if (!CanPlayerUnlock(Networking.LocalPlayer))
                    return;
                
                Unlock();
                SendNetUnlock();
            }
            else
            {
                if (!CanPlayerLock(Networking.LocalPlayer))
                    return;
                
                Lock(Networking.LocalPlayer);
                SendNetLock();
            }
        }

        #region Permission Checks
        private bool CanPlayerLock(VRCPlayerApi player)
        {
            return whitelist == null || whitelist._IsPlayerWhitelisted(player);;
        }
        
        private bool CanPlayerUnlock(VRCPlayerApi player)
        {
            return player == lockingPlayer;
        }
        #endregion

        #region Locking
        private void Lock(VRCPlayerApi player)
        {
            lockingPlayer = player;
            isLocked = true;
            UpdateExternal(true, player);
        }

        private void Unlock()
        {
            lockingPlayer = null;
            isLocked = false;
            UpdateExternal(false , null);
        }

        private void UpdateExternal(bool newLockState, VRCPlayerApi newLockingPlayer)
        {
            foreach (var receiver in lockStateReceivers)
            {
                if (receiver == null)
                    return;
                
                receiver.SetProgramVariable(LockStateBoolVariableName, newLockState);
                receiver.SetProgramVariable(LockingPlayerVariableName, newLockingPlayer);
                receiver.SendCustomEvent(LockStateChangeEvent);
            }
        }
        #endregion
        
        #region Networking

        private void SendNetLock()
        {
            
        }

        private void SendNetUnlock()
        {
            
        }
        
        #endregion
    }
}

