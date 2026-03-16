using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer.Utilities;


namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistStringLoader : UdonSharpBehaviour
    {
        [Tooltip("URL of the whitelist to load. Should be formatted as one line per display name.")] [FormerlySerializedAs("url")]
        [SerializeField] private VRCUrl url;
        [FormerlySerializedAs("whitelistManager")] [Tooltip("Whitelist that should be updated.")]
        [SerializeField] private WhitelistSetterBase whitelist;
        
        [Range(0, 60)] [Tooltip("Time in seconds to wait before starting to load the whitelist.")]
        [SerializeField] private float initialDelay = 0f;
        
        [Tooltip("If enabled, the whitelist will be redownloaded after a certain amount of time.")]
        [SerializeField] private bool autoReload = true;
        [Range(1, 60)]
        [Tooltip("Time after which the whitelist will be downloaded again in minutes.")]
        [SerializeField] private int autoReloadTime = 10;
        [Tooltip("If enabled the system will try to downloaded the whitelist again after 20s if it errored out.")]
        [SerializeField] private bool autoReloadOnError = true;
        
        private bool loading;
        private string prevString;
        
        private const string DEBUG_PREFIX = "String Loader | ";

        private void Start()
        {
            SendCustomEventDelayedSeconds(nameof(_LoadString), initialDelay);
        }
        
        public void _LoadString()
        {
            if (loading)
            {
                MUIDebug.LogWarning($"{DEBUG_PREFIX}Already loading this list.");
                return;
            }
            loading = true;
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }
        
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            MUIDebug.Log($"{DEBUG_PREFIX}Whitelist from {url} loaded successfully!");
            loading = false;
            ApplyString(result.Result);
            AutoReload();
        }
        
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            loading = false;
            if (autoReloadOnError)
            {
                MUIDebug.LogError($"{DEBUG_PREFIX}Couldn't load whitelist from {url} because {result.Error}! Tying again in 20s.");
                SendCustomEventDelayedSeconds(nameof(_LoadString), 20);
            }
            else
            {
                MUIDebug.LogError($"{DEBUG_PREFIX}Couldn't load whitelist from {url} because {result.Error}!");
                AutoReload();
            }
        }
        
        private void ApplyString(string newString)
        {
            if (newString == prevString) return;
            prevString = newString;
            
            var users = newString.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            whitelist._ReplaceWhitelist(users);
        }
        
        private void AutoReload()
        {
            if (!autoReload) return;
            MUIDebug.Log($"{DEBUG_PREFIX}Auto Reload Enabled: Next update in {autoReloadTime} minutes.");
            SendCustomEventDelayedSeconds(nameof(_LoadString), autoReloadTime * 60);
        }
    }
}
