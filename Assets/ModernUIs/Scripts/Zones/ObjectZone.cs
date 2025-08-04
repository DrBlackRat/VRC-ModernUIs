
using System.Security.Policy;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Collider))]
    public class ObjectZone : UdonSharpBehaviour
    {
        [Tooltip("Transform of which the position, rotation and scale will be used. If left empty the transform this is attached to will be used. ")]
        [SerializeField] private Transform zoneTransform;
        /// <summary>
        /// Transform of which the positions, rotation & scale should be used for this zone.
        /// </summary>
        public Transform ZoneTransform => zoneTransform;
        
        private ObjectMover objectMover;
        private int id;
        private Collider zoneCollider;
        private bool inZone; // Workaround for OnPlayerTriggerEnter being fired twice when respawning

        private void Start()
        {
            if (zoneTransform == null)
                zoneTransform = transform;
            zoneCollider = GetComponent<Collider>();
        }

        public void _SetupObjectZone(ObjectMover newObjectMover, int newID)
        {
            objectMover = newObjectMover;
            id = newID;
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal || inZone) return;
            inZone = true;
            objectMover._PlayerEnteredZone(id);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            inZone = false;
            objectMover._PlayerLeftZone(id);
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
