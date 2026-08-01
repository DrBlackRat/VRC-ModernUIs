
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(Collider))]
    public class SimpleAudioZone : UdonSharpBehaviour
    {
        [Header("Default Audio Settings:")] 
        [Tooltip("The default gain that should be applied once unmuted. (How loud their voice will be.)")] 
        [SerializeField] public float defaultGain = 15f;
        [Tooltip("The default near distance in meters that should be applied once unmuted. (How close to the player their max volume is reached.)")] 
        [SerializeField] private float defaultNearDistance = 0f;
        [Tooltip("The default far distance in meters that should be applied once unmuted. (How far away from the player they can still be heard.)")] 
        [SerializeField] private float defaultFarDistance = 25f;
        
        [Header("In Zone Audio Settings:")]
        [Tooltip("The gain that should be applied when a user is in the zone. (How loud their voice will be.)")] 
        [SerializeField] private float inZoneGain = 15f;
        [Tooltip("The near distance in meters that should be applied when a user is in the zone. (How close to the player their max volume is reached.)")] 
        [SerializeField] private float inZoneNearDistance = 0f;
        [Tooltip("The far distance in meters that should be applied when a user is in the zone. (How far away from the player they can still be heard.)")] 
        [SerializeField] private float inZoneFarDistance = 25f;
        
        private DataList inZonePlayerIDs = new DataList();
        private Collider zoneCollider;
        private bool hasInitialized;
        
        private void Start()
        {
            zoneCollider = GetComponent<Collider>();
            hasInitialized = true;
        }
        
        private void OnEnable()
        {
            if (!hasInitialized) return;
            
            zoneCollider.enabled = false;
            SendCustomEventDelayedFrames(nameof(_EnableCollider), 1);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            inZonePlayerIDs.Remove(player.playerId);
        }
        
        private void OnDisable()
        {
            ApplyDefaultAudioAll();
            inZonePlayerIDs.Clear();
        }
        
        #region Triggers & Colliders

        public void _EnableCollider()
        {
            if (!isActiveAndEnabled)
                return;
            
            zoneCollider.enabled = true;
        }
        
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            var playerID = player.playerId;
            if (inZonePlayerIDs.Contains(playerID)) return;
            
            inZonePlayerIDs.Add(playerID);
            ApplyZoneAudio(player);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            var playerID = player.playerId;
            if (!inZonePlayerIDs.Remove(playerID)) return;
            ApplyDefaultAudio(player);
        }
        #endregion
        

        #region Audio Management
        private void ApplyZoneAudio(VRCPlayerApi player)
        {
            player.SetVoiceGain(inZoneGain);
            player.SetVoiceDistanceFar(inZoneFarDistance);
            player.SetVoiceDistanceNear(inZoneNearDistance);
        }

        private void ApplyDefaultAudio(VRCPlayerApi player)
        {
            player.SetVoiceGain(defaultGain);
            player.SetVoiceDistanceFar(defaultFarDistance);
            player.SetVoiceDistanceNear(defaultNearDistance);
        }

        private void ApplyDefaultAudioAll()
        {
            for (int i = 0; i < inZonePlayerIDs.Count; i++)
            {
                ApplyDefaultAudio(VRCPlayerApi.GetPlayerById(inZonePlayerIDs[i].Int));
            }
        }
        #endregion
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            gameObject.layer = 0;
        }
#endif
    }
}
