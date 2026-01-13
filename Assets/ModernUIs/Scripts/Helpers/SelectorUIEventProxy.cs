using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.Hideout
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUIEventProxy : UdonSharpBehaviour
    {
        [Tooltip("Udon Behaviour that will have the Events send to.")]
        [SerializeField] private UdonBehaviour proxyBehaviour;
        [Tooltip("Events that will be send to the Proxy Behaviour. \nSame order as Selector UI Buttons will be used. Make sure the amount of events is the same as the amount of Selector Buttons.")]
        [SerializeField] private string[] proxyEvents;

        [HideInInspector] public int selectionId;

        public void _SelectionChanged()
        {
            if (selectionId < 0 || selectionId > proxyEvents.Length) return;
            proxyBehaviour.SendCustomEvent(proxyEvents[selectionId]);
        }
    }
}

