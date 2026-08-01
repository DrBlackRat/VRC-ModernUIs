
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.General
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SpawnSelector : UdonSharpBehaviour
    {
        [Header("Persistence")]
        [Tooltip("Data Key that will be used to load the selected spawn, should be the same Data Key as the UI Selector.")]
        [SerializeField] private string dataKey = "CHANGE THIS";

        [Header("Loading")]
        [Tooltip("Location the player will be teleported to once they join while they wait for persistence to finish loading.")]
        [SerializeField] private Transform waitPos;
        [Tooltip("Object that is enabled while waiting for persistence to load. Useful for things like black box.")]
        [SerializeField] private GameObject waitObj;

        [Header("Spawns")]
        [Tooltip("Spawn positions a user can select, default will be the first entry.")]
        [SerializeField] private Transform[] positions;

        private void Start()
        {
            if (waitObj != null)
                waitObj.SetActive(true);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            
            player.TeleportTo(waitPos.position, waitPos.rotation);
        }
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            
            if (waitObj != null)
                waitObj.SetActive(false);
            
            var spawn = PlayerData.GetInt(player, dataKey);
            if (spawn < 0 || spawn >= positions.Length) return;
            
            var pos = positions[spawn];
            if (pos == null) return;
            
            player.TeleportTo(pos.position, pos.rotation);
        }
    }
}
