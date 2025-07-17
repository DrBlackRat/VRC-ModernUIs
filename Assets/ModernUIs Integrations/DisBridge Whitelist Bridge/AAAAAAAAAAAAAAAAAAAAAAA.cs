using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonVR.DisBridge;

namespace UdonVR.DisBridge.Plugins
{
    public class AAAAAAAAAAAAAAAAAAAAAAA : DisBridgePlugin
    {
        // Built in Variables: 
            // - PluginManager disBridge
            // - RoleContainer[] _roleContainers
            // - bool useStaff
            // - bool useSupporters
        
        // Built in Methods: 
            // - bool TryGetPlayerRoleContainer(VRCPlayerApi _player, out RoleContainer _roleContainer)
            // - bool TryGetPlayerRoleContainer(VRCPlayerApi _player, ref RoleContainer[] _playersRoles)
            // - bool IsMemberInRoles(VRCPlayerApi _player)
    
        //Built-in Start method. It's recommended to use "_UVR_Init()" instead of "Start" when making a DisBridge Plugin.
        private void Start()
        {
            disBridge.AddPlugin(gameObject);
        }

        //Runs when DisBridge finishes pulling the roles.
        public override void _UVR_Init()
        {

        }
        
        //Runs when a RoleContainer's user list has updated.
        public override void _UVR_Update()
        {
        
        }
    }
}