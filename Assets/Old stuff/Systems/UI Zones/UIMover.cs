using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class UIMover : UdonSharpBehaviour
    {
        [Header("UI Menu")]
        [Tooltip("Menu / Object you want to move.")]
        [SerializeField] private Transform menu;
        
        [Header("Zones")]
        [Tooltip("Default Position of the Menu, used when ever player isn't in a different Zone.")]
        [SerializeField] private Transform defaultTransform;
        [Space(10)]
        [Tooltip("Zones the UI should move to.")]
        [SerializeField] private UIZone[] zones;

        private int zoneCounter;

        private void Start()
        {
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i]._SetupUIMover(this, i);
            }
        }

        public void _PlayerEnteredZone(int id)
        {
            zoneCounter++;
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


