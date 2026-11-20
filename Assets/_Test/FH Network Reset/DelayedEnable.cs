using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.Testing
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DelayedEnable : UdonSharpBehaviour
    {
        public GameObject target;
        public bool state = true;
        public float delay = 5f;
        void Start()
        {
            VRCTween.DelayedSetActive(target, state, delay);
        }
    }
}
