
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class LockButton : UdonSharpBehaviour
    {
        // Connection
        [SerializeField]
        [Tooltip("The Udon Behaviours that will be receiving the lock state change event, as well as the locking player, the lock state and the identifier of this Lock Button.")]
        private UdonBehaviour[] lockStateReceivers;
        [SerializeField]
        [Tooltip("The identifier of this Lock Button. If a system supports multiple Lock Buttons, this can be used to differentiate them.")]
        private int identifier = 0;
        
        // Whitelist
        [SerializeField]
        [Tooltip("(Optional) Whitelist used to limit who can lock / unlock.")]
        private WhitelistGetterBase whitelist;
        
        // Networking
        [SerializeField] 
        [Range(1, 10)]
        [Tooltip("How long to wait before retrying applying net data sender is not allowed to yet.")]
        protected float netRetryDelay = 5f;
        [SerializeField] 
        [Range(1, 10)]
        [Tooltip("How many times to attempt applying net data sender is not allowed to yet.")]
        protected int maxNetRetryAttempts = 4;
        [SerializeField] 
        [Tooltip("By default only retries or failed attempts are logged. If enabled, other things like successful attempts, updating a newly joined player, etc are logged as well.")]
        protected bool extraLogging = true;

        
        // UI
        private VRCPlayerApi localPlayer;
        
        private bool isLocked;
        private VRCPlayerApi lockingPlayer;
        
        protected int netAttempts;
        protected bool netRetryScheduled;
        protected VRCPlayerApi netSendingPlayer;
        private bool netIsLocked;
        private VRCPlayerApi netLockingPlayer;
        
        // "Interface" these will be called / set on the receiving Udon Behaviors
        private const string LockStateChangeEvent = "_OnLockStateChanged";      // public void _OnLockStateChanged();
        private const string LockingPlayerVariableName = "LockingPlayer";       // [HideInInspector] public VRCPlayerApi LockingPlayer; (Can be null if no longer present)
        private const string LockStateBoolVariableName = "IsLocked";            // [HideInInspector] public bool IsLocked;
        private const string IdentifierVariableName = "LockButtonIdentifier";   // [HideInInspector] public int LockButtonIdentifier;
        
        protected const int NET_MAX_QUEUE_SIZE = 5; // Max number of network events that can be queued at once

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (lockingPlayer != localPlayer)
                return;

            SetNetLocked(player.playerId, isLocked);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == lockingPlayer)
                Unlock();
        }

        public void _OnLockButtonPressed()
        {
            if (isLocked)
            {
                if (!CanPlayerUnlock(localPlayer))
                    return;
                
                Unlock();
                NetUnlock(-1);
            }
            else
            {
                if (!CanPlayerLock(localPlayer))
                    return;
                
                Lock(localPlayer);
                NetLock(-1);
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
        private void Lock(VRCPlayerApi player) => SetLocked(true, player);
        private void Unlock() => SetLocked(false, null);
        
        private void SetLocked(bool locked, VRCPlayerApi player)
        {
            isLocked = locked;
            lockingPlayer = locked ? player : null; // When not locked, there is no locking player

            UpdateExternal(locked, lockingPlayer);
        }

        private void UpdateExternal(bool locked, VRCPlayerApi player)
        {
            foreach (var receiver in lockStateReceivers)
            {
                if (receiver == null)
                    continue;
                
                receiver.SetProgramVariable(LockStateBoolVariableName, locked);
                receiver.SetProgramVariable(LockingPlayerVariableName, player);
                receiver.SetProgramVariable(IdentifierVariableName, identifier);
                receiver.SendCustomEvent(LockStateChangeEvent);
            }
        }
        #endregion
        
        #region Sending Networking Data
        private void NetLock(int receiverId) => SetNetLocked(receiverId, true);
        private void NetUnlock(int receiverId) => SetNetLocked(receiverId, false);
        
        private void SetNetLocked(int receiverId, bool locked)
        {
            SendCustomNetworkEvent(NetworkEventTarget.Others, nameof(OnNetLockReceived), receiverId, locked);
        }
        #endregion
        
        #region Receiving Networking Data
        [NetworkCallable(NET_MAX_QUEUE_SIZE)]
        public void OnNetLockReceived(int receiverId, bool locked)
        {
            if (!NetworkCalling.InNetworkCall)
            {
                MUIDebug.LogError($"OnNetworkedDataReceived can only be called as a network event! ({gameObject.name})", gameObject);
                return;
            }
            
            var callingPlayer = NetworkCalling.CallingPlayer;
            if (!Utilities.IsValid(callingPlayer))
            {
                MUIDebug.LogError($"OnNetworkedDataReceived called by an invalid player! ({gameObject.name})", gameObject);
                return;
            }
            
            if (receiverId != -1 && receiverId != localPlayer.playerId)
                return;
            
            if (isLocked == locked)
                return;
            
            netIsLocked = locked;
            netLockingPlayer = callingPlayer;
            
            netSendingPlayer = callingPlayer;
            DiscardNetAttempts();
            TryApplyNetData();
        }
        
        public void _DelayedNetAttempt()
        {
            netRetryScheduled = false;
            if (netAttempts == 0) // 0th = finished or received new data
                return; 
            
            TryApplyNetData();
        }
        
        private void TryApplyNetData()
        {
            netAttempts++;
            if ((netIsLocked && !CanPlayerLock(netLockingPlayer)) || (!netIsLocked && !CanPlayerUnlock(netLockingPlayer)))
            {
                if (netAttempts >= maxNetRetryAttempts)
                {
                    MUIDebug.LogError($"{netSendingPlayer.displayName} is still not allowed to lock or unlock! Aborting applying net data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
                    DiscardNetAttempts();
                    return;
                }

                if (!netRetryScheduled)
                {
                    netRetryScheduled = true;
                    MUIDebug.LogWarning($"{netSendingPlayer.displayName} is not allowed to lock or unlock but tried to on {gameObject.name}. Retrying in {netRetryDelay}s, Attempt {netAttempts} / {maxNetRetryAttempts}.", gameObject);
                    SendCustomEventDelayedSeconds(nameof(_DelayedNetAttempt), netRetryDelay);
                }
                
                return;
            }
            
            if (netAttempts > 1)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} is now allowed to lock or unlock! Applying networked data on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
            } 
            else if (extraLogging)
            {
                MUIDebug.Log($"{netSendingPlayer.displayName} has send network data on {gameObject.name}.", gameObject);
            }
            
            DiscardNetAttempts();
            SetLocked(netIsLocked, netLockingPlayer);
        }
        

        private void DiscardNetAttempts()
        {
            netAttempts = 0;
        }
        #endregion
    }
}

