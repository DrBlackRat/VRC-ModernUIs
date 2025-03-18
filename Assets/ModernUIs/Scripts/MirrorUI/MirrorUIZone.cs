﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(BoxCollider))]
    public class MirrorUIZone : UdonSharpBehaviour
    {
        [Header("Settings:")]
        [Tooltip("Mirror UI the Trigger Events should be sent to. You can have multiple of these attached to a Mirror UI.")]
        [SerializeField] private MirrorUI mirrorBehaviour;
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            mirrorBehaviour._ZoneUpdated(true);
            
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            mirrorBehaviour._ZoneUpdated(false);
            
        }
    }
}
