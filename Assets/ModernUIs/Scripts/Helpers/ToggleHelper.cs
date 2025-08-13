
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Helpers
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ToggleHelper : UdonSharpBehaviour
    {
        [FormerlySerializedAs("toggleObjects")]
        [Tooltip("Game Objects to toggle on / off")]
        [SerializeField] private GameObject[] gameObjects;

        [SerializeField] private Collider[] colliders;
        
        
        [Tooltip("ID at which everything should be turned on. Should be left at 1 for a simple two state Selector UI Toggle.")]
        [SerializeField] private int enabledId = 1;

        [HideInInspector] public int selectionId;

        public void _SelectionChanged()
        {
            bool state = selectionId == enabledId;

            if (gameObjects != null && gameObjects.Length != 0)
            {
                foreach (var toggleObject in gameObjects)
                {
                    if (toggleObject == null) continue;
                    toggleObject.SetActive(state);
                }
            }

            if (colliders != null && colliders.Length != 0)
            {
                foreach (var toggleCollider in colliders)
                {
                    if (toggleCollider == null) continue;
                    toggleCollider.enabled = state;
                }
            }
        }
    }
}

