
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class LockableSyncedSelectorUI : SyncedSelectorUI
    {
        [Tooltip("Default locked state of the Selector UI.")]
        [SerializeField] protected bool locked;

        [Tooltip("Button that switches the locked state.")]
        [SerializeField] protected Button lockButton;
        [Tooltip("Text Mesh Pro UI of which the text should be changed between \"Locked\", \"Unlocked\" and \"Not Whitelisted\".")]
        [SerializeField] protected TextMeshProUGUI lockTextState;
        [Tooltip("Text Mesh Pro UI of which the text should be changed to display the current network owner.")]
        [SerializeField] protected TextMeshProUGUI lockTextOwner;
        [Tooltip("Icon that is enabled if the Selector UI is locked.")]
        [SerializeField] protected GameObject lockIcon;
        [Tooltip("Icon that is enabled if the Selector UI is unlocked.")]
        [SerializeField] protected GameObject unlockIcon;
        
        [UdonSynced] protected bool netLocked;
        protected bool canChangeState;

        protected override void Start()
        {
            base.Start();
            UpdateLock(locked, true);
        }
        
        public void _LockPressed()
        {
            if (!canChangeState) return;
            UpdateLock(!locked, false);
        }

        public override void _WhitelistUpdated()
        {
            base._WhitelistUpdated();
            UpdateLock(locked, true);
        }
        
        protected override void _UpdateInteractable()
        {
            foreach (var selectorUIButton in selectorUIButtons)
            {
                selectorUIButton._UpdateLocked(!canChangeState);
            }
            selector._SetEnabled(canChangeState);
        }

        #region Networking / Ownership / Persistence
        public override void OnDeserialization()
        {
            UpdateLock(netLocked, true);
            base.OnDeserialization();
        }
        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            if (!canChangeState && !Networking.LocalPlayer.IsOwner(gameObject) )
            {
                MUIDebug.LogError($"{requester.displayName} has tried to make {newOwner.displayName} the Network Owner, but isn't allowed to!");
                return false;
            }

            return base.OnOwnershipRequest(requester, newOwner);
        }        
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            UpdateLock(locked, true);
        }
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!canChangeState) return;
            base.OnPlayerRestored(player);
        }
        #endregion

        protected void UpdateLock(bool newlocked, bool fromNet)
        {
            locked = newlocked;
            netLocked = locked;
            
            if (!fromNet)
            {
                if (!Networking.LocalPlayer.IsOwner(gameObject))
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                RequestSerialization();
            }
            
            // If a player is whitelisted (or no whitelist is given) and it's unlocked they can change the Selector UI
            // If a player is whitelisted (or no whitelist is given) and whitelistOverride is enabled they can change the Selector UI
            canChangeState = hasAccess && (!locked || Networking.LocalPlayer.IsOwner(gameObject));

            //Update Button
            lockIcon.SetActive(locked);
            unlockIcon.SetActive(!locked);
            lockButton.interactable = canChangeState;
            lockTextOwner.text = $"Owner: {Networking.GetOwner(gameObject).displayName}";
            // Lock Button Text (This is stupidly big for what it is lol)
            string lockText;
            if (locked)
            {
                lockText = (!hasAccess && Networking.LocalPlayer.IsOwner(gameObject))
                    ? "Not Whitelisted"
                    : "Locked";         
            }
            else
            {
                lockText = (!hasAccess)
                    ? "Not Whitelisted"
                    : "Unlocked";
            }
            lockTextState.text = lockText;
            
            _UpdateInteractable();
        }
        
        protected override bool UpdateSelection(int newState, bool skipPersistence, bool skipSameCheck, bool fromNet)
        {
            if (!skipSameCheck && !fromNet && !canChangeState)
            {
                MUIDebug.LogError("You are not allowed to change this Selector UI!");
                return false;
            }

            if (!base.UpdateSelection(newState, skipPersistence, skipSameCheck, fromNet)) return false;
            if (!fromNet) UpdateLock(locked, false);
            return true;
        }
    }
}
