
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    public class WhitelistButton : WhitelistButtonBase
    {
        [Tooltip("Udon Behaviours to send the Event Names to when the button got pressed.")]
        [SerializeField] private UdonBehaviour[] udonBehaviours;
        [Tooltip("Name of the event to send once the button got pressed.")]
        [SerializeField] private string pressedEventName;
        protected override void WhitelistButtonPressed()
        {
            if (udonBehaviours != null && udonBehaviours.Length != 0)
            {
                foreach (var udonBehaviour in udonBehaviours)
                {
                    if (udonBehaviour == null) continue;
                    udonBehaviour.SendCustomEvent(pressedEventName);
                }
            }
        }
    }
}
