using DrBlackRat.VRC.ModernUIs.Localization;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class LocalizedComponentCallback
    {
        [PostProcessScene(-10)]
        public static void OnPostProcessScene()
        {
            LanguageSelectorUI[] languageSelectorUIs = Object.FindObjectsOfType<LanguageSelectorUI>(true);
            LocalizedComponent[] localizedComponents = Object.FindObjectsOfType<LocalizedComponent>(true);
            CustomLocalizedComponent[] customLocalizedComponents = Object.FindObjectsOfType<CustomLocalizedComponent>(true);
            PlayerNameReplacer[] playerNameReplacers = Object.FindObjectsOfType<PlayerNameReplacer>(true);

            if (languageSelectorUIs != null && languageSelectorUIs.Length != 0)
            {
                if (languageSelectorUIs.Length > 1)
                {
                    MUIDebug.LogError("You can only have one Language Selector UI per scene!");
                }
                else
                {
                    languageSelectorUIs[0].localizedComponents = localizedComponents;
                    languageSelectorUIs[0].customLocalizedComponents = customLocalizedComponents;
                    languageSelectorUIs[0].playerNameReplacers = playerNameReplacers;
                }
            }
                
        }
    }
}
