
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    public class DisBridgeToWhitelistBridge : DisBridgePlugin
    {
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
            var supporters = disBridge.GetSupporterDisplaynames();
        }
    }
}
