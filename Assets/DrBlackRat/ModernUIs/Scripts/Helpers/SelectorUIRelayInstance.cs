
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.SelectorUI
{
    /// <summary>
    /// Instance of a Selector UI Relay. (In Game)
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorUIRelayInstance : UdonSharpBehaviour
    {
        [HideInInspector]
        public UdonBehaviour[][] udonBehaviours;
        [HideInInspector]
        public string[] selectionIdVariableNames;
        [HideInInspector]
        public string[] selectionChangedEventNames;
        
        #region Public "API"
        [HideInInspector]
        public int selectionId;
        public void _SelectionChanged()
        {
            for (int i = 0; i < udonBehaviours.Length; i++)
            {
                UdonBehaviour[] optionUdonBehaviours = udonBehaviours[i];
                foreach (var optionUdonBehaviour in optionUdonBehaviours)
                {
                    if (optionUdonBehaviour == null)
                    {
                        continue;
                    }
                    
                    optionUdonBehaviour.SetProgramVariable(selectionIdVariableNames[i], selectionId);
                    optionUdonBehaviour.SendCustomEvent(selectionChangedEventNames[i]);
                }
            }
        }
        #endregion
    }
}
