
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.SelectorUI;
using UnityEngine.Serialization;
using VRC.SDK3.Persistence;

namespace DrBlackRat.VRC.ModernUIs.Localization
{
    public class LanguageSelectorUI : SelectorUI.SelectorUI
    {
        [Tooltip("VRChat Language Codes based of the RFC 5646 syntax that are linked with each option on the Selector UI, same order as buttons will be used.")]
        [SerializeField] protected string[] linkedLanguageCodes;
        public string[] LinkedLanguageCodes => linkedLanguageCodes;

        // Set by callback script
        [HideInInspector] public LocalizedComponent[] localizedComponents;
        [HideInInspector] public CustomLocalizedComponent[] customLocalizedComponents;
        [HideInInspector] public PlayerNameReplacer[] playerNameReplacers;
        
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            base.OnPlayerRestored(player);
            
            if (!persistenceLoaded)
                ApplyMenuLanguage();
        }

        /// <summary>
        /// Grabs the current selected language from a VRC Player API and applies it.
        /// </summary>
        private void ApplyMenuLanguage()
        {
            if (linkedLanguageCodes == null || linkedLanguageCodes.Length == 0) return;
            
            string menuLanguage = VRCPlayerApi.GetCurrentLanguage();
            for (int i = 0; i < linkedLanguageCodes.Length; i++)
            {
                if (linkedLanguageCodes[i] == menuLanguage)
                {
                    if (i < selectorUIButtons.Length)
                    {
                        UpdateSelection(i, true, false, false);
                    }
                    else
                    {
                        MUIDebug.LogWarning($"Language Selector UI | Could not apply Linked Language Code \"{linkedLanguageCodes[i]}\" as there is no available state for this index.", gameObject);
                    }
                }
            }
        }

        protected override void ChangeExternalSelection(int id, bool skipBehaviour)
        {
            foreach (var localizedComponent in localizedComponents)
            {
                if (localizedComponent == null) continue;
                localizedComponent.ApplyLocalization(id);
            }

            foreach (var customLocalizedComponent in customLocalizedComponents)
            {
                if (customLocalizedComponent == null) continue;
                customLocalizedComponent.ApplyLocalization(id);
            }

            foreach (var playerNameReplacer in playerNameReplacers)
            {
                if (playerNameReplacer == null) continue;
                if (!playerNameReplacer.ReplaceOnLocalizationChange) continue;
                
                playerNameReplacer.ReplacePlayerName();
            }
        }
    }
}

