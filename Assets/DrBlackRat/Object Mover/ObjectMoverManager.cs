
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.General
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)]
    public class ObjectMoverManager : UdonSharpBehaviour
    {
        [Header("Object")]
        [Tooltip("Menu / Object you want to move.")]
        [SerializeField] private Transform moveableTransform;
        [Tooltip("If enabled, the scale of the zone transform will also be applied. Should be left off unless you specifically want the scale to change.")]
        [SerializeField] private bool applyScale;
        
        [Header("Object Areas")]
        [Tooltip("Zones the Menu / Object should move to.")]
        [SerializeField] private ObjectArea[] areas;

        private int zoneCounter;

        private int normalZoneId; // used to restore the correct Zone if it was previsuly forced
        private bool preventZoneChange;
        
        private bool constrain;
        private Transform currentTransform;
        private int prevZoneId;

        private void Start()
        {
            if (areas == null || areas.Length == 0) return;
            for (int i = 0; i < areas.Length; i++)
            {
                if (areas[i] == null) continue;
                areas[i]._SetupObjectZone(this, i);
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
        }

        /// <summary>
        /// Unforces a Zone if it was previously forced. Moves the object to the actual current Zone.
        /// </summary>
        public void UnforceZone()
        { 
            preventZoneChange = false;
            SelectZone(normalZoneId, false);
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
            if (id < 0 || id >= areas.Length || areas[id] == null) return;
            if (saveZoneId) normalZoneId = id;
            
            if (preventZoneChange) return;
            areas[prevZoneId]._ZoneSelected(false);
            prevZoneId = id;
            areas[id]._ZoneSelected(true);
            
            currentTransform = areas[id].ZoneTransform;
            constrain = areas[id].ConstraintMode;
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
