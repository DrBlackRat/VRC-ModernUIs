
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
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

        [SerializeField]
        [Tooltip("\"Selectables\" or Buttons, Toggles, Sliders etc. that should only be interactable if unlocked or you are the locking person." +
            "\nShould only be used for none Modern UIs components!")]
        private Selectable[] selectables;
        
        // Whitelist
        [SerializeField]
        [Tooltip("(Optional) Whitelist used to limit who can lock / unlock.")]
        private WhitelistGetterBase whitelist;
        
        // Joining / Leaving / Network
        [SerializeField]
        [Tooltip("If the Lock Button should be unlocked if the player who locked it leaves the instance. If disabled and a whitelist is used another whitelist player will be chosen to lock if a whitelist is not present the master will become the locking player.")]
        private bool unlockIfPlayerLeft = true;
        [SerializeField]
        [Range(1, 10)]
        [Tooltip("How long to wait before retrying applying net data if the sender is not allowed yet.")]
        protected float netRetryDelay = 5f;
        [SerializeField]
        [Range(0, 10)]
        [Tooltip("How many times to retry applying net data if the sender is not allowed yet.")]
        protected int maxNetRetryAttempts = 3;
        
        // Other
        [SerializeField] [Tooltip("If enabled, extra debug logging will be printed to the console.")]
        private bool debugLogging = false;
        
        // UI
        [SerializeField]
        [Tooltip("The UI Button that will be used to lock / unlock the Lock Button.")]
        private Button lockButton;
        [SerializeField]
        [Tooltip("GameObjects that will be enabled when the Lock Button is locked. Used for Icons, Text, etc.")]
        private GameObject[] lockedObjects;
        [SerializeField]
        [Tooltip("GameObjects that will be enabled when the Lock Button is unlocked. Used for Icons, Text, etc.")]
        private GameObject[] unlockedObjects;
        [SerializeField]
        [Tooltip("TextMeshProUGUI that will be set to the locking player's name.")]
        private TextMeshProUGUI lockingPlayerText;
        
        // Private
        private VRCPlayerApi localPlayer;
        private VRCPlayerApi leavingPlayer;
        
        private bool isLocked;
        private VRCPlayerApi lockingPlayer;

        // Networking
        private bool netLocked;
        private VRCPlayerApi netLocking;
        private VRCTweenHandle netReceiveRetryEvent;
        private int netAttempts;
        
        // "Interface" these will be called / set on the receiving Udon Behaviors
        private const string LockStateChangeEvent = "_OnLockStateChanged";      // public void _OnLockStateChanged() {}
        private const string IdentifierVariableName = "LockButtonIdentifier";   // [HideInInspector] public int LockButtonIdentifier;
        private const string LockingPlayerVariableName = "LockingPlayer";       // [HideInInspector] public VRCPlayerApi LockingPlayer; //(Can be null if no longer present)
        private const string LockStateBoolVariableName = "IsLocked";            // [HideInInspector] public bool IsLocked;
        
        #region Unity, VRChat & ModernUI Events
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            if (whitelist != null)
                whitelist._SetUpConnection(this, nameof(_WhitelistUpdated));
            
            UpdateUI(isLocked, lockingPlayer);
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            TrySendingInitialLockState(player);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            leavingPlayer = player;
            HandleLockingPlayerLeaving(player);
            leavingPlayer = null;
        }

        public void _WhitelistUpdated()
        {
            UpdateUI(isLocked, lockingPlayer);
        }
        #endregion

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
            UpdateUI(locked, lockingPlayer);
            
            if (debugLogging)
                MUIDebug.Log($"{gameObject.name} is now {(locked ? $"locked by {lockingPlayer.displayName}" : "unlocked")}.", gameObject);
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
            
            var interactable = !locked || lockingPlayer == localPlayer;
            foreach (var selectable in selectables)
            {
                if (selectable == null)
                    continue;
                selectable.interactable = interactable;
            }
        }

        private void UpdateUI(bool locked, VRCPlayerApi player)
        {
            foreach (var obj in lockedObjects)
            {
                if (obj == null)
                    continue;
                obj.SetActive(locked);
            }

            foreach (var obj in unlockedObjects)
            {
                if (obj == null)
                    continue;
                obj.SetActive(!locked);
            }
            
            lockingPlayerText.text = locked ? player.displayName : "None";
            lockButton.interactable = locked ? CanPlayerUnlock(localPlayer) : CanPlayerLock(localPlayer);
        }
        #endregion
        
        #region Player Joining & Leaving
        // Joining / Sending Initial State
        private void TrySendingInitialLockState(VRCPlayerApi player)
        {
            if (!isLocked)
                return;
            if (lockingPlayer != localPlayer)
                return;
            if (player == localPlayer)
                return;
            
            if (debugLogging)
                MUIDebug.Log($"Sending initial lock state of {gameObject.name} to {player.displayName}!", gameObject);
            
            NetLock(player.playerId);
        }
        
        // Leaving
        private void HandleLockingPlayerLeaving(VRCPlayerApi player)
        {
            if (!isLocked)
                return;
            if (player != lockingPlayer)
                return;
            
            if (debugLogging)
                MUIDebug.Log($"Locking player of {gameObject.name} left the instance!");

            if (unlockIfPlayerLeft)
            {
                Unlock();
                return;
            }

            if (whitelist != null)
            {
                var newLockingPlayer = whitelist._GetPlayerInInstance(leavingPlayer);
                if (newLockingPlayer != null)
                {
                    Lock(newLockingPlayer);
                }
                else
                {
                    Unlock();
                }

                return;
            }

            Lock(Networking.Master);
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
        [NetworkCallable]
        public void OnNetLockReceived(int receiverId, bool newIsLocked)
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
            
            if (isLocked == newIsLocked)
                return;
            
            netLocked = newIsLocked;
            netLocking = callingPlayer;

            netAttempts = 0;
            netReceiveRetryEvent.Kill();
            
            _TryApplyNetLock();
        }

        public void _TryApplyNetLock()
        {
            if (netLocked && !CanPlayerLock(netLocking) || !netLocked && !CanPlayerLock(netLocking))
            {
                if (netAttempts >= maxNetRetryAttempts)
                {
                    MUIDebug.LogError($"{netLocking.displayName} is still not allowed to lock / unlock! Aborting changing lock on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
                    return;
                }
                
                MUIDebug.LogWarning($"Player {netLocking.displayName} tried to lock / unlock {gameObject.name} but is not allowed to!  Retrying in {netRetryDelay}s, Attempt {netAttempts} / {maxNetRetryAttempts}.", gameObject);
                netReceiveRetryEvent = VRCTween.DelayedCall(this, nameof(_TryApplyNetLock), netRetryDelay);
                netAttempts++;
                return;
            }

            if (netAttempts > 1)
            {
                MUIDebug.Log($"{netLocking.displayName} is now allowed lock / unlock! Applying changing lock on {gameObject.name} after {netAttempts} / {maxNetRetryAttempts} attempts.", gameObject);
            }

            SetLocked(netLocked, netLocking);
        }
        #endregion
    }
}

