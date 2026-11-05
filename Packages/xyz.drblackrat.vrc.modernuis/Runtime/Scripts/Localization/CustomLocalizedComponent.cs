
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Localization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CustomLocalizedComponent : UdonSharpBehaviour
    {
        [Tooltip("Udon Behaviours to change the data of. \nNote that it will apply the same data on all of them.")]
        [SerializeField] private UdonBehaviour[] udonBehaviours;
        [Tooltip("Name of the event that will be send to the Udon Behavior once the data was changed.")]
        [SerializeField] private string localizationChangedEvent = "_LocalizationChanged"; 
        
        [Tooltip("Name of the public string variable that should be changed.")]
        [SerializeField] private string stringVariableName;
        [TextArea(1, 3)] [Tooltip("Strings for each language, same order as on the Language Selector UI will be used")]
        [SerializeField] private string[] strings;

        [Tooltip("Name of the public sprite variable that should be changed.")]
        [SerializeField] private string spriteVariableName;
        [Tooltip("Sprites for each language, same order as on the Language Selector UI will be used")]
        [SerializeField] private Sprite[] sprites;
        
        /// <summary>
        /// Swaps out everything that can be localized to the provided index.
        /// </summary>
        /// <param name="index">Index of the language that should be applied.</param>
        public void ApplyLocalization(int index)
        {
            if (index < 0 || udonBehaviours == null || udonBehaviours.Length == 0) return;

            var applyString = false;
            if (!string.IsNullOrEmpty(stringVariableName))
            {
                if (strings != null && index < strings.Length && !string.IsNullOrEmpty(strings[index]))
                {
                    applyString = true;
                }
                else
                {
                    MUIDebug.LogWarning($"Custom Localized Component | Missing string for Index: {index} on {gameObject.name}!", gameObject);
                }
            }

            var applySprite = false;
            if (!string.IsNullOrEmpty(spriteVariableName))
            {
                if (sprites != null && index < sprites.Length && sprites[index] != null)
                {
                    applySprite = true;
                }
                else
                {
                    MUIDebug.LogWarning($"Custom Localized Component | Missing sprite for Index: {index} on {gameObject.name}!", gameObject);
                }
            }

            foreach (var udonBehaviour in udonBehaviours)
            {
                if (udonBehaviour == null) continue;
                
                if (applyString)
                    udonBehaviour.SetProgramVariable(stringVariableName, strings[index]);
                if (applySprite)
                    udonBehaviour.SetProgramVariable(spriteVariableName, sprites[index]);
                
                udonBehaviour.SendCustomEvent(localizationChangedEvent);
            }
        }
    }
}
