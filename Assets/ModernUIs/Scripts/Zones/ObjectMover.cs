using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class ObjectMover : UdonSharpBehaviour
    {
        [FormerlySerializedAs("menu")]
        [Tooltip("Menu / Object you want to move.")]
        [SerializeField] private Transform moveableTransform;
        [Tooltip("If enabled, the scale of the zone transform will also be applied. Should be left off unless you specifically want the scale to change.")]
        [SerializeField] private bool applyScale;
        
        [Tooltip("Default Position and Rotation of the Menu, used when ever player isn't in a different Zone.")]
        [SerializeField] private Transform defaultTransform;

        [Tooltip("Zones the Menu / Object should move to.")]
        [SerializeField] private ObjectZone[] zones;

        private int zoneCounter;

        private void Start()
        {
            if (zones == null || zones.Length == 0) return;
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null) continue;
                zones[i]._SetupObjectZone(this, i);
            }
        }

        public void _PlayerEnteredZone(int id)
        {
            zoneCounter++;
            if (zones[id] == null) return;
            moveableTransform.SetPositionAndRotation(zones[id].ZoneTransform.position, zones[id].ZoneTransform.rotation);
            if (applyScale)
                moveableTransform.localScale = zones[id].ZoneTransform.localScale;
        }

        public void _PlayerLeftZone(int id)
        {
            zoneCounter--;
            if (zoneCounter == 0)
            {
                moveableTransform.SetPositionAndRotation(defaultTransform.position, defaultTransform.rotation);
                if (applyScale)
                    moveableTransform.localScale = defaultTransform.localScale;
            }
        }
    }
}


