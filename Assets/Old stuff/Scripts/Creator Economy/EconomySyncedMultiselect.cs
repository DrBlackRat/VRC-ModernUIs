using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [DefaultExecutionOrder(-100)]
    public class EconomySyncedMultiselect : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [Tooltip("Default Locked state.")]
        [UdonSynced] [SerializeField] private bool locked;
        [Tooltip("ESM Buttons this should be connected to.")]
        [SerializeField] private EsmButton[] esmButtons;
        
        [Header("Persistence:")]
        [Tooltip("Turn on if this Toggle should be saved using Persistence.")]
        [SerializeField] private bool usePersistence = true;
        [Tooltip("Data Key that will be used to save / load this Setting, everything using Persistence should have a different Data Key.")]
        [SerializeField] private string dataKey = "CHANGE THIS";

        [Header("Toggles:")]
        [Tooltip("Objects that should be turned on / off. The same order as buttons will be used.")]
        [SerializeField] private GameObject[] itemObjs;
        [Space(10)] 
        [Tooltip("UdonBehaviour that will receive the events listed below depending on what item got selected.")]
        [SerializeField] private UdonBehaviour toggleBehaviour;
        [Tooltip("Select event names that will be send to the UdonBehaviour. The same order as buttons will be used.")]
        [SerializeField] private string[] selectEventNames;
        [Tooltip("Unselect event names that will be send to the UdonBehaviour. The same order as buttons will be used.")]
        [SerializeField] private string[] unSelectEventNames;

        [SerializeField] private string lockEventName;
        [SerializeField] private string unLockEventName;

        [Header("Lock Button:")] 
        [SerializeField] private Button lockButton;
        [SerializeField] private TextMeshProUGUI lockTextState;
        [SerializeField] private TextMeshProUGUI lockTextOwner;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject unlockIcon;

        [Header("UI Animation:")] 
        [SerializeField] private RectTransform selector;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float movementDuration;
        
        private bool animateUI;
        private float movementElapsedTime;
        private Vector3 oldSelectorPos;

        private int item = 0;
        [UdonSynced] private int netItem;

        private bool offOverride;

        private void Start()
        {
            for (int i = 0; i < esmButtons.Length; i++)
            {
                esmButtons[i]._SetUpConnection(this, i);
            }
            
            UpdateLock();
            UpdateSelector();
        }
        public void _CustomUpdate()
        {
            if (!animateUI) return;
            AnimateUI(oldSelectorPos, esmButtons[item].transform.position);
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 0);
        }
        
        // Network
        public override void OnDeserialization()
        {
            if (netItem != item) _ChangeItem(netItem, false, true);
            UpdateLock();
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            // Prevent Error when leaving
            if (player.isLocal) return;
            UpdateLock();
        }
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            UpdateLock();
        }
        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {
            return !locked || newOwner.IsOwner(gameObject);
        }
        
        // Persistence
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!usePersistence) return;
            if (!player.isLocal || !player.IsOwner(gameObject)) return;
            if (PlayerData.TryGetInt(player, dataKey, out int value))
            {
                if (!esmButtons[value]._Owned()) return;
                _ChangeItem(value, true, false);
            }
        }

        private void ToggleItems()
        {
            if (offOverride)
            {
                if (itemObjs != null)
                {
                    foreach (var item in itemObjs)
                    {
                        if (item == null) continue;
                        item.SetActive(false);
                    }
                }
                
                if (toggleBehaviour == null) return;
                foreach (var eventName in unSelectEventNames)
                {
                    toggleBehaviour.SendCustomEvent(eventName);
                }
            }
            else
            {
                if (itemObjs != null)
                {
                    foreach (var item in itemObjs)
                    {
                        if (item == null) continue;
                        item.SetActive(false);
                    }
                    if (itemObjs != null && itemObjs.Length > 0 && itemObjs[item] != null) itemObjs[item].SetActive(true);
                }
                
                if (toggleBehaviour == null) return;
                foreach (var eventName in unSelectEventNames)
                {
                    toggleBehaviour.SendCustomEvent(eventName);
                }
                toggleBehaviour.SendCustomEvent(selectEventNames[item]);
            }
        }
        
        private void UpdateSelector()
        {
            // UI Animation
            animateUI = true;
            movementElapsedTime = 0f; 
            oldSelectorPos = selector.position;
            _CustomUpdate();
        }

        private void UpdateLock()
        {
            foreach (var button in esmButtons)
            {
                button._UpdateLock(locked && !Networking.LocalPlayer.IsOwner(gameObject));
            }
            lockIcon.SetActive(locked);
            unlockIcon.SetActive(!locked);
            lockButton.interactable = !locked || Networking.LocalPlayer.IsOwner(gameObject);
            lockTextState.text = locked ? "Locked" : "Unlocked";
            lockTextOwner.text = $"By: {Networking.GetOwner(gameObject).displayName}";
            
            if (toggleBehaviour != null) toggleBehaviour.SendCustomEvent(locked? lockEventName : unLockEventName);
            
        }

        // Update Input & Sync
        public void _ChangeItem(int newItem, bool fromStorage, bool fromNet)
        {
            if (locked && !Networking.LocalPlayer.IsOwner(gameObject) && !fromNet) return;
            if (item == newItem) return;
            item = newItem;
            netItem = item;
            
            if (!fromStorage && !fromNet && usePersistence) PlayerData.SetInt(dataKey, item);
            
            UpdateSelector();
            ToggleItems();
            
            // Networking
            if (fromNet) return;
            if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            RequestSerialization();
        }
        
        // Lock Button
        public void _Lock()
        {
            if (locked && !Networking.LocalPlayer.IsOwner(gameObject)) return;
            // Needs to become owner before locking, otherwise it would get stuck
            if (!Networking.LocalPlayer.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            locked = !locked;
            UpdateLock();
            // Networking
            RequestSerialization();
        }
        
        // Pole Toggle
        public void _EnableItems()
        {
            offOverride = false;
            ToggleItems();
        }
        public void _DisableItems()
        {
            offOverride = true;
            ToggleItems();
        }
        
        // Animation
        private void AnimateUI(Vector3 startPos, Vector3 endPos)
        {
            movementElapsedTime += Time.deltaTime;
            var percentageComplete = movementElapsedTime / movementDuration;
            var smoothPercentageComplete = animationCurve.Evaluate(percentageComplete);
            // Set Selector Position
            selector.position = Vector3.LerpUnclamped(startPos, endPos, smoothPercentageComplete);
            
            if (percentageComplete >= 1f)
            {
                movementElapsedTime = 0f;
                animateUI = false;
            }
        }
    }
}
