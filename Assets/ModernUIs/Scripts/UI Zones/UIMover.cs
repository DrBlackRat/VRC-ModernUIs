using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class UIMover : UdonSharpBehaviour
    {
        [Tooltip("Menu / Object you want to move.")]
        [SerializeField] private Transform menu;
        
        [Tooltip("Default Position and Rotation of the Menu, used when ever player isn't in a different Zone.")]
        [SerializeField] private Transform defaultTransform;

        [Tooltip("Zones the UI should move to.")]
        [SerializeField] private UIZone[] zones;

        private int zoneCounter;

        private void Start()
        {
            if (zones == null || zones.Length == 0) return;
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null) continue;
                zones[i]._SetupUIMover(this, i);
            }
        }

        public void _PlayerEnteredZone(int id)
        {
            zoneCounter++;
            if (zones[id] == null) return;
            menu.SetPositionAndRotation(zones[id].transform.position, zones[id].transform.rotation);
        }

        public void _PlayerLeftZone(int id)
        {
            zoneCounter--;
            if (zoneCounter == 0)
            {
                menu.SetPositionAndRotation(defaultTransform.position, defaultTransform.rotation);
            }
        }
    }
}


