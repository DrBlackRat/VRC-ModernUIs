
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Helpers
{
    [DefaultExecutionOrder(-50)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ToggleHelper : UdonSharpBehaviour
    {
        [FormerlySerializedAs("toggleObjects")]
        [Tooltip("Game Objects to enable / disable.")]
        [SerializeField] private GameObject[] gameObjects;
        [Tooltip("Colliders to enable / disable.")]
        [SerializeField] private Collider[] colliders;
        [Tooltip("Canvases to enable / disable.")]
        [SerializeField] private Canvas[] canvases;
        [Tooltip("Pickups to change the Pickupable state of.")]
        [SerializeField] private VRC_Pickup[] pickups;
        
        
        [Tooltip("ID at which everything should be turned on. Should be left at 1 for a simple two state Selector UI Toggle.")]
        [SerializeField] private int enabledId = 1;

        /// <summary>
        /// ID of the current selected state of a Selector UI. Needs to be set before calling <see cref="_SelectionChanged"/>.
        /// </summary>
        [HideInInspector] public int selectionId;

        private void Start()
        {
            Toggle(false);
        }

        /// <summary>
        /// Gets called by a Selector UI when the state changes, before doing so the <see cref="selectionId"/> should be set.
        /// </summary>
        public void _SelectionChanged()
        {
            Toggle(selectionId == enabledId);
        }

        /// <summary>
        /// Turns everything on.
        /// </summary>
        public void _TurnOn()
        {
            Toggle(true);
        }

        /// <summary>
        /// Turns everything off.
        /// </summary>
        public void _TurnOff()
        {
            Toggle(false);
        }

        private void Toggle(bool state)
        {
            if (gameObjects != null && gameObjects.Length != 0)
            {
                foreach (var toggleObject in gameObjects)
                {
                    if (toggleObject == null) continue;
                    toggleObject.SetActive(state);
                }
            }

            if (colliders != null && colliders.Length != 0)
            {
                foreach (var toggleCollider in colliders)
                {
                    if (toggleCollider == null) continue;
                    toggleCollider.enabled = state;
                }
            }

            if (canvases != null && canvases.Length != 0)
            {
                foreach (var canvase in canvases)
                {
                    if (canvase == null) continue;
                    canvase.enabled = state;
                }
            }

            if (pickups != null && pickups.Length != 0)
            {
                foreach (var pickup in pickups)
                {
                    if (pickup == null) continue;
                    pickup.pickupable = state;
                }
            }
        }
    }
}

