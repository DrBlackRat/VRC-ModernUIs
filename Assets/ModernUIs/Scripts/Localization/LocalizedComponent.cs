using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace DrBlackRat.VRC.ModernUIs.Localization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedComponent : UdonSharpBehaviour
    {
        [Tooltip("Text Mesh Pro UGUI component to change the text of.")]
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        [TextArea(1, 8)] [Tooltip("Text for each language, same order as on the Language Selector UI will be used.")]
        [SerializeField] private string[] texts;

        [Tooltip("Unity UI Image component to change the texture of.")]
        [SerializeField] private Image uiImage;
        [Tooltip("Sprites for each language, same order as on the Language Selector UI will be used.")]
        [SerializeField] private Sprite[] sprites;

        [Tooltip("Renderer component to change the material of.")]
        [SerializeField] private Renderer materialRenderer;
        [Tooltip("Materials for each language, same order as on the Language Selector UI will be used.")]
        [SerializeField] private Material[] materials;

        [Tooltip("Audio Source Component to change the audio clip of.")] 
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Audio Clips for each language, same order as on the Language Selector UI will be used.")]
        [SerializeField] private AudioClip[] audioClips;

        [Tooltip("Game Objects of which one is enabled for each language, same order as on the Language Selector UI will be used.")]
        [SerializeField] private GameObject[] gameObjects;

        /// <summary>
        /// Swaps out everything that can be localized to the provided index.
        /// </summary>
        /// <param name="index">Index of the language that should be applied.</param>
        public void ApplyLocalization(int index)
        {
            if (index < 0) return;

            if (textMeshProUGUI != null)
            {
                if (texts != null && index < texts.Length && !string.IsNullOrEmpty(texts[index]))
                {
                    textMeshProUGUI.text = texts[index];
                }
                else
                {
                    MUIDebug.LogWarning($"Localized Component | Missing text for Index: {index} on {gameObject.name}!", gameObject);
                }
            }

            if (uiImage != null)
            {
                if (sprites != null && index < sprites.Length && sprites[index] != null)
                {
                    uiImage.sprite = sprites[index];
                }
                else
                {
                    MUIDebug.LogWarning($"Localized Component | Missing sprite for Index: {index} on {gameObject.name}!", gameObject);
                }
            }

            if (materialRenderer != null)
            {
                if (materials != null && index < materials.Length && materials[index] != null)
                {
                    materialRenderer.material = materials[index];
                }
                else
                {
                    MUIDebug.LogWarning($"Localized Component | Missing material for Index: {index} on {gameObject.name}!", gameObject);
                }
            }

            if (audioSource != null)
            {
                if (audioClips != null && index < audioClips.Length && audioClips[index] != null)
                {
                    audioSource.clip = audioClips[index];
                }
                else
                {
                    MUIDebug.LogWarning($"Localized Component | Missing audio clip for Index: {index} on {gameObject.name}!", gameObject);
                }
            }

            if (gameObjects != null && gameObjects.Length != 0)
            {
                if (index < gameObjects.Length && gameObjects[index] != null)
                {
                    foreach (var gameObject in gameObjects)
                    {
                        gameObject.SetActive(false);
                    }
                    gameObjects[index].SetActive(true);
                }
                else
                {
                    MUIDebug.LogWarning($"Localized Component | Missing game object for Index: {index} on {gameObject.name}!", gameObject);
                }
            }
        }

        #region Grabbing Defaults
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void Reset()
        {
            LanguageSelectorUI languageSelector = FindObjectOfType<LanguageSelectorUI>();
            int length = Math.Max(languageSelector.LinkedLanguageCodes.Length, 1);
            
            if (textMeshProUGUI == null)
            {
                textMeshProUGUI = GetComponent<TextMeshProUGUI>();
                if (textMeshProUGUI != null &&  (texts == null || texts.Length == 0))
                {
                    texts = new string[length];
                    texts[0] = textMeshProUGUI.text;
                }
            }

            if (uiImage == null)
            {
                uiImage = GetComponent<Image>();
                if (uiImage != null && (sprites == null || sprites.Length == 0))
                {
                    sprites = new Sprite[length];
                    sprites[0] = uiImage.sprite;
                }
            }

            if (materialRenderer == null)
            {
                materialRenderer = GetComponent<Renderer>();
                if (materialRenderer != null && (materials == null || materials.Length == 0))
                {
                    materials = new Material[length];
                    materials[0] = materialRenderer.sharedMaterial;
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource != null && (audioClips == null || audioClips.Length == 0))
                {
                    audioClips = new AudioClip[length];
                    audioClips[0] = audioSource.clip;
                }
            }
        }
#endif
        #endregion
    }
}

