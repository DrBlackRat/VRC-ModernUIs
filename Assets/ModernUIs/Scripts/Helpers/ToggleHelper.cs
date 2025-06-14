
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Helpers
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ToggleHelper : UdonSharpBehaviour
    {
        [Tooltip("Game Objects to toggle on / off")]
        [SerializeField] private GameObject[] toggleObjects;
        [Tooltip("ID at which the Game Objects should be turned on. Should be left at 1 for a simple two state Selector UI Toggle.")]
        [SerializeField] private int enabledId = 1;

        [HideInInspector] public int selectionId;

        public void _SelectionChanged()
        {
            bool state = selectionId == enabledId;

            if (toggleObjects != null && toggleObjects.Length != 0)
            {
                foreach (var toggleObject in toggleObjects)
                {
                    if (toggleObject == null) continue;
                    toggleObject.SetActive(state);
                }
            }
        }
    }
}

