using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonVR.DisBridge;
using DrBlackRat.VRC.ModernUIs.Whitelist;

namespace UdonVR.DisBridge.Plugins
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TestPlugin : DisBridgePlugin
    {
        
        private void Start()
        {
            disBridge.AddPlugin(gameObject);
        }
        public override void _UVR_Init()
        {
            Debug.LogError("AAAAAAAA DisBridge Init");
        }
        public override void _UVR_Update()
        {
            Debug.LogError("AAAAAAAA DisBridge Update");
        }
    }
}