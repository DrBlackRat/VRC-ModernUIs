
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeController : UdonSharpBehaviour
    {
        [HideInInspector] public float gain;
        [HideInInspector] public float farDistance;
        private VRCPlayerApi[] players = new VRCPlayerApi[0];

        public void _GainUpdated()
        {
            if (players == null || players.Length == 0) return;
            foreach (var player in players)
            {
                player.SetVoiceGain(gain);
            }
        }

        public void _FarUpdated()
        {
            if (players == null || players.Length == 0) return;
            foreach (var player in players)
            {
                player.SetVoiceDistanceFar(farDistance);
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            
            player.SetVoiceGain(gain);
            player.SetVoiceDistanceFar(farDistance);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
        }
    }
}

