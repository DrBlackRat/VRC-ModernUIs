using DrBlackRat.VRC.ModernUIs.Localization;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Editor
{
    public static class LocalizedComponentCallback
    {
        [PostProcessScene()]
        public static void OnPostProcessScene()
        {
            LanguageSelectorUI languageSelectorUI = Object.FindObjectOfType<LanguageSelectorUI>(true);
            LocalizedComponent[] localizedComponents = Object.FindObjectsOfType<LocalizedComponent>(true);

            languageSelectorUI.localizedComponents = localizedComponents;
        }
    }
}
