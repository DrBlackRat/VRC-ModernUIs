using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Collider))]
    public class PlayerZone : UdonSharpBehaviour
    {
        [Tooltip("If enabled there will be logs for entering and leaving this zone.")]
        [SerializeField] private bool logging;
        
        [Tooltip("Game Objects that should be enabled if a player is in this zone.")]
        [SerializeField] private GameObject[] gameObjects;
        
        [Tooltip("Animators that should have a bool set to true if a player is in this zone.")]
        [SerializeField] protected Animator[] animators;
        [Tooltip("Parameter Name of the bool that should be set.")]
        [SerializeField] protected string animParamName = "inZone";
        
        [Tooltip("Udon Behaviours that will have events send to.")]
        [SerializeField] protected UdonBehaviour[] externalBehaviours;
        [Tooltip("Name of the Event that will be fired if the player enters the Zone.")]
        [SerializeField] protected string enteredZoneEvent = "_ZoneEntered";
        [Tooltip("Name of the Event that will be fired if the player leaves the Zone.")]
        [SerializeField] protected string exitedZoneEvent = "_ZoneLeft";
        
        private Collider zoneCollider;
        private bool inZone; // Workaround for OnPlayerTriggerEnter being fired twice when respawning
        
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            UpdateExternal(false);
        }
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || inZone) return;
            inZone = true;
            
            if (logging)
                MUIDebug.Log($"Player Zone: {gameObject.name} | Player Entered", gameObject);
            UpdateExternal(true);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            inZone = false;
            
            if (logging)
                MUIDebug.Log($"Player Zone: {gameObject.name} | Player Exited", gameObject);
            UpdateExternal(false);
        }

        //Spawn / Respawn Fixes
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 0);
        }
        public void _EnableCollider()
        {
            zoneCollider.enabled = true;
        }

        private void UpdateExternal(bool state)
        {
            if (gameObjects != null && gameObjects.Length != 0)
            {
                foreach (var item in gameObjects)
                {
                    if (item == null) continue;
                    item.SetActive(state);
                }
            }
            
            if (animators != null && animators.Length > 0)
            {
                foreach (var animator in animators)
                {
                    if (animator == null) continue;
                    animator.SetBool(animParamName, state);
                }
            }
            
            if (externalBehaviours != null && externalBehaviours.Length > 0)
            {
                var eventName = state ? enteredZoneEvent : exitedZoneEvent;
                foreach (var behaviour in externalBehaviours)
                {
                    if (behaviour == null) continue;
                    behaviour.SendCustomEvent(eventName);
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            gameObject.layer = 0;
        }
#endif
    }
}

