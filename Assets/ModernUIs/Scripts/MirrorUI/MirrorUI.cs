﻿using System;
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
