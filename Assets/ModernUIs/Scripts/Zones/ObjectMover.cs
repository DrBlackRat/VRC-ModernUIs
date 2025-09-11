using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class ObjectMover : UdonSharpBehaviour
    {
        [FormerlySerializedAs("menu")]
        [Tooltip("Menu / Object you want to move.")]
        [SerializeField] private Transform moveableTransform;
        [Tooltip("If enabled, the scale of the zone transform will also be applied. Should be left off unless you specifically want the scale to change.")]
        [SerializeField] private bool applyScale;
        

        [Tooltip("Zones the Menu / Object should move to.")]
        [SerializeField] private ObjectZone[] zones;

        private int zoneCounter;

        private int normalZoneId; // used to restore the correct Zone if it was previsuly forced
        private bool preventZoneChange;
        
        private bool constrain;
        private Transform currentTransform;
        private int prevZoneId;

        private void Start()
        {
            if (zones == null || zones.Length == 0) return;
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null) continue;
                zones[i]._SetupObjectZone(this, i);
            }
            SelectZone(0);
        }

        public override void PostLateUpdate()
        {
            if (constrain)
              MoveAndScale();
        }

        /// <summary>
        /// Allows forcing an object to a Zone, even though it would technically be in a different Zone.
        /// </summary>
        /// <param name="id">Int | Zone that the object should be force moved to.</param>
        public void ForceZone(int id)
        {
            preventZoneChange = false;
            SelectZone(id, false);
            preventZoneChange = true;
            Debug.LogError($"Forcing Blocking: {preventZoneChange}");
        }

        /// <summary>
        /// Unforces a Zone if it was previously forced. Moves the object to the actual current Zone.
        /// </summary>
        public void UnforceZone()
        { 
            preventZoneChange = false;
            Debug.LogWarning($"NormalID: {normalZoneId}");
            SelectZone(normalZoneId, false);
            Debug.LogError($"Unforceing Blocking: {preventZoneChange}");
        }
        
        /// <summary>
        /// Send by an Object Zone if a player enters it.
        /// </summary>
        /// <param name="id"> Int | Zone that was entered.</param>
        public void _PlayerEnteredZone(int id)
        {
            zoneCounter++;
            SelectZone(id);
        } 

        /// <summary>
        /// Send by an Object Zone if a player leaves it.
        /// </summary>
        /// <param name="id"> Int | Zone that was left.</param>
        public void _PlayerLeftZone(int id)
        {
            zoneCounter--;
            if (zoneCounter == 0)
            {
                SelectZone(0);
            }
        }

        private void SelectZone(int id, bool saveZoneId = true)
        {
            if (id < 0 || id >= zones.Length || zones[id] == null) return;
            if (saveZoneId) normalZoneId = id;
            
            Debug.LogError($"Select Zone Blocking: {preventZoneChange}");
            if (preventZoneChange) return;
            
            zones[prevZoneId]._ZoneSelected(false);
            prevZoneId = id;
            zones[id]._ZoneSelected(true);
            
            currentTransform = zones[id].ZoneTransform;
            constrain = zones[id].ConstraintMode;
            MoveAndScale();
        }

        private void MoveAndScale()
        {
            moveableTransform.SetPositionAndRotation(currentTransform.position, currentTransform.rotation);
            if (applyScale)
                moveableTransform.localScale = currentTransform.lossyScale; 
        }
    }
}


