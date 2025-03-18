using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MirrorUI : SelectorUI
    {
        private int zoneCounter;
        
        public override void _UpdateSelection(int newButtonId, bool fromStorage, bool skipCheck)
        {
            // Change State
            if (newButtonId == selectedState && !skipCheck)
            {
                prevSelectedState = selectedState;
                selectedState = 0;
            }
            else
            {
                prevSelectedState = selectedState;
                selectedState = newButtonId;
            }

            ChangeExternalSelection(selectedState, false);
            
            if (usePersistence && !fromStorage) PlayerData.SetInt(dataKey, selectedState);
            
            // Button Transition
            selectorUIButtons[prevSelectedState]._UnSelect();
            selectorUIButtons[selectedState]._Select();
            
            // UI Animation
            animateUI = true;
            movementElapsedTime = 0f;
            oldSelectorPos = selectorTransform.position;
            _CustomUpdate();
        }
        
        // Mirror Zone
        public void _ZoneUpdated(bool hasEntered)
        {
            if (hasEntered)
            {
                zoneCounter++;
                if (zoneCounter != 1 || selectedState == 0) return;
                ChangeExternalSelection(selectedState, true);
            }
            else
            {
                zoneCounter--;
                if (zoneCounter != 0 || selectedState == 0) return;
                ChangeExternalSelection(0, true);
            }
        }
    }
}
