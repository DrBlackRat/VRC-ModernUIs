using System;
using DrBlackRat.VRC.ModernUIs.Helpers;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistToggler : UdonSharpBehaviour
    {
        [FormerlySerializedAs("whitelistManager")]
        [Tooltip("Whitelist Manager that is storing info on which user is whitelisted.")]
        [SerializeField] private WhitelistGetterBase whitelist;
        [Tooltip("If enabled this Whitelist Toggler will act as turned on if a whitelisted player is present in the instance. If disabled the local player needs to be whitelisted.")]
        [SerializeField] private bool allowIfWhitelistedPlayerPresent;

        [Tooltip("Toggle Helpers to allow user to toggle more things using a whitelist.")]
        [SerializeField] private ToggleHelper[] toggleHelpers;
        
        [Tooltip("Objects that should only be enabled for whitelisted users.")]
        [SerializeField] private GameObject[] whitelistedObjs;
        [Tooltip("Objects that should be enabled for everyone, but disabled for whitelisted users.")]
        [SerializeField] private GameObject[] notWhitelistedObjs;

        [Tooltip("Canvases that should only be enabled for whitelisted users.")]
        [SerializeField] private Canvas[] whitelistedCanvases;
        [Tooltip("Canvases that should be enabled for everyone, but disabled for whitelisted users.")]
        [SerializeField] private Canvas[] notWhitelistedCanvases;
        
        [Tooltip("Canvas groups that should have their alpha set to 1 for whitelisted users and 0 for everyone else.")]
        [SerializeField] private CanvasGroup[] whitelistedCanvasGroups;
        [Tooltip("Canvas groups that should have their alpha set to 1 for everyone, but 0 for whitelisted users.")]
        [SerializeField] private CanvasGroup[] notWhitelistedCanvasGroups;

        [Tooltip("\"Selectables\" or Buttons, Toggles, Sliders etc. that should only be interactable for whitelisted users." +
                 "\nDO NOT USE FOR MODERN UIs")]
        [SerializeField] private Selectable[] whitelistedSelectables;
        [Tooltip("\"Selectables\" or Buttons, Toggles, Sliders etc. that should be interactable for everyone, but be NOT interactable for whitelisted users." +
                 "\nDO NOT USE FOR MODERN UIs")]
        [SerializeField] private Selectable[] notWhitelistedSelectables;
        
        [Tooltip("Renderer component to change the material of.")]
        [SerializeField] private Renderer materialRenderer;
        [Tooltip("Materials applied when whitelisted.")]
        [SerializeField] private Material[] whitelistedMaterials;
        [Tooltip("Materials applied when not whitelisted.")]
        [SerializeField] private Material[] notWhitelistedMaterials;
        
        private void Start()
        {
            whitelist._SetUpConnection((IUdonEventReceiver)this);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (allowIfWhitelistedPlayerPresent) _WhitelistUpdated();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (allowIfWhitelistedPlayerPresent) _WhitelistUpdated();
        }
        
        public void _WhitelistUpdated()
        {
            var isWhitelisted = allowIfWhitelistedPlayerPresent ? whitelist._IsWhitelistPlayerPresent() : whitelist._IsPlayerWhitelisted(Networking.LocalPlayer);

            // Toggle Helper
            if (toggleHelpers != null && toggleHelpers.Length != 0)
            {
                foreach (var toggleHelper in toggleHelpers)
                {
                    if (toggleHelper == null) continue;
                    toggleHelper._Toggle(isWhitelisted);
                }
            }
            
            
            // Game Objects
            if (whitelistedObjs != null && whitelistedObjs.Length != 0)
            {
                foreach (var obj in whitelistedObjs)
                {
                    if (obj == null) continue;
                    obj.SetActive(isWhitelisted);
                }
            }
            if (notWhitelistedObjs != null && notWhitelistedObjs.Length != 0)
            {
                foreach (var obj in notWhitelistedObjs)
                {
                    if (obj == null) continue;
                    obj.SetActive(!isWhitelisted);
                }  
            }
            
            // Canvases
            if (whitelistedCanvases != null && whitelistedCanvases.Length != 0)
            {
                foreach (var canvas in whitelistedCanvases)
                {
                    if (canvas == null) continue;
                    canvas.enabled = isWhitelisted;
                }
            }
            if (notWhitelistedCanvases != null && notWhitelistedCanvases.Length != 0)
            {
                foreach (var canvas in notWhitelistedCanvases)
                {
                    if (canvas == null) continue;
                    canvas.enabled = !isWhitelisted;
                }
            }
            
            // Selectables / UI Buttons etc.
            if (whitelistedSelectables != null && whitelistedSelectables.Length != 0)
            {
                foreach (var selectable in whitelistedSelectables)
                {
                    if (selectable == null) continue;
                    selectable.interactable = isWhitelisted;
                }
            }
            if (notWhitelistedSelectables != null && notWhitelistedSelectables.Length != 0)
            {
                foreach (var selectable in notWhitelistedSelectables)
                {
                    if (selectable == null) continue;
                    selectable.interactable = !isWhitelisted;
                }
            }

            // Canvas Groups
            if (whitelistedCanvasGroups != null && whitelistedCanvasGroups.Length != 0)
            {
                foreach (var canvasGroup in whitelistedCanvasGroups)
                {
                    if (canvasGroup == null) continue;
                    canvasGroup.alpha = isWhitelisted ? 1 : 0 ;
                }
            }
            if (notWhitelistedCanvasGroups != null && notWhitelistedCanvasGroups.Length != 0)
            {
                foreach (var canvasGroup in notWhitelistedCanvasGroups)
                {
                    if (canvasGroup == null) continue;
                    canvasGroup.alpha = isWhitelisted ? 0 : 1;
                }
            }

            if (materialRenderer != null)
                materialRenderer.materials = isWhitelisted ? whitelistedMaterials : notWhitelistedMaterials;
        }
    }
}


