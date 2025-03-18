using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TabButton : SelectorButton
    {
        private TabsUI tabsUI;
        private int buttonId;
        public void Setup(Color newNormalColor, Color newSelectedColor, AnimationCurve newSmoothingCurve, float newMovementDuration, TabsUI newTabsUI, int newButtonId)
        {
            normalColor = newNormalColor;
            selectedColor = newSelectedColor;
            smoothingCurve = newSmoothingCurve;
            movementDuration = newMovementDuration;
            
            // Tab UI
            tabsUI = newTabsUI;
            buttonId = newButtonId;
            
            // Reset 
            UpdateUIState();
            UpdateUI(1f);
        }

        public void _ButtonPressed()
        {
            tabsUI._UpdateTab(buttonId, false);
        }
    }
}
