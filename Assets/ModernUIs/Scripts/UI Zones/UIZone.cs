
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(BoxCollider))]
    public class UIZone : UdonSharpBehaviour
    {
        private UIMover uiMover;
        private int id;

        public void _SetupUIMover(UIMover newUIMover, int newID)
        {
            uiMover = newUIMover;
            id = newID;
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            uiMover._PlayerEnteredZone(id);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            uiMover._PlayerLeftZone(id);
        }
    }
}
