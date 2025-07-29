
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.General
{
    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ZoneToggle : UdonSharpBehaviour
    {
        [Header("Game Objects")]
        [Tooltip("Game Objects that should only be enabled if the player is inside the zone.")]
        [SerializeField] private GameObject[] objsToEnable;
        [Tooltip("Game Objects that should only be enabled if the player is NOT inside the zone. This is rarely used.")]
        [SerializeField] private GameObject[] objsDisable;
        
        private Collider zoneCollider;
        private bool inZone; // Workaround for OnPlayerTriggerEnter being fired twice when respawning
        
        private void Start()
        {
            ToggleObjs(false);
            zoneCollider = GetComponent<Collider>();
        }
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || inZone) return;
            inZone = true;
            ToggleObjs(true);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            inZone = false;
            ToggleObjs(false);
        }

        private void ToggleObjs(bool state)
        {
            foreach (var obj in objsToEnable)
            {
                if (obj == null) continue;
                obj.SetActive(state);
            }

            foreach (var obj in objsDisable)
            {
                if (obj == null) continue;
                obj.SetActive(!state);
            }
        }
        
        
        // Spawn / Respawn Fixes
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
    }
}
