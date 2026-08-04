
using System.Security.Policy;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ObjectZone : UdonSharpBehaviour
    {
        [Tooltip(
            "If enabled, while in this zone the object will continuously be moved to this zone instead of just once while entering it.")]
        [SerializeField]
        private bool constraintMode;

        public bool ConstraintMode => constraintMode;

        [Tooltip(
            "Transform of which the position, rotation and scale will be used. If left empty the transform this is attached to will be used. ")]
        [SerializeField]
        private Transform zoneTransform;

        [Tooltip(
            "Objects that are only enabled if the movable object is currently in this Zone. Useful for things like backgrounds etc.")]
        [SerializeField]
        private GameObject[] zoneObjects;

        /// <summary>
        /// Transform of which the positions, rotation & scale should be used for this zone.
        /// </summary>
        public Transform ZoneTransform => zoneTransform;

        private ObjectMover objectMover;
        private int id;

        private bool inZone; // Workaround for OnPlayerTriggerEnter being fired twice when respawning

        public void _SetupObjectZone(ObjectMover newObjectMover, int newID)
        {
            if (zoneTransform == null)
                zoneTransform = transform;
            _ZoneSelected(false);

            objectMover = newObjectMover;
            id = newID;
        }

        public void _ZoneSelected(bool state)
        {
            if (zoneObjects != null && zoneObjects.Length != 0)
            {
                foreach (var toggleObject in zoneObjects)
                {
                    if (toggleObject == null) continue;
                    toggleObject.SetActive(state);
                }
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || 
                !player.IsValid() ||
                !player.isLocal)
            {
                return;
            }
            
            inZone = true;
            objectMover._PlayerEnteredZone(id);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!Utilities.IsValid(player) || 
                !player.IsValid() ||
                !player.isLocal)
            {
                return;
            }

            inZone = false;
            objectMover._PlayerLeftZone(id);
        }
        
#if UNITY_EDITOR
        // Fix spawning inside the collider being broken in client sim
        private Collider _collider;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal)
            {
                return;
            }
            
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                return;
            }
            
            _collider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 1);
        }

        public void _EnableCollider()
        {
            _collider.enabled = true;
        }
#endif
    }
}
