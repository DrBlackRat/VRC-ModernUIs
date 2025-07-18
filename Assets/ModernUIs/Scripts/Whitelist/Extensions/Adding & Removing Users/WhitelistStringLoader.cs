using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;


namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistStringLoader : UdonSharpBehaviour
    {
        [SerializeField] private VRCUrl url;
        [FormerlySerializedAs("whitelistManager")] 
        [SerializeField] private WhitelistSetterBase whitelist;
        [Tooltip("If enabled the entire Whitelist will be replaced instead of just adding new users to it.")]
        [SerializeField] private bool replaceWhitelist;
        
        [Tooltip("If enabled, the whitelist will be redownloaded after a certain amount of time.")]
        [SerializeField] private bool autoReload = true;
        [Range(1, 60)]
        [Tooltip("Time after which the whitelist will be downloaded again in minutes.")]
        [SerializeField] private int autoReloadTime = 10;
        [Tooltip("If enabled the system will try to downloaded the whitelist again after 20s if it errored out.")]
        [SerializeField] private bool autoReloadOnError = true;
        
        private bool loading = false;
        private string prevString;
        
        private const string DEBUG_PREFIX = "String Loader | ";

        private void Start()
        {
            _LoadString();
        }
        public void _LoadString()
        {
            if (loading) return;
            loading = true;
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }
        private void ApplyString(string newString)
        {
            if (newString == prevString) return;
            prevString = newString;
            
            var users = newString.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (replaceWhitelist)
            {
                whitelist._ReplaceWhitelist(users);
            }
            else
            {
                whitelist._AddUsers(users);
            }
            
        }
        private void AutoReload()
        {
            if (!autoReload) return;
            MUIDebug.Log($"{DEBUG_PREFIX}Auto Reload Enabled: Next update in {autoReloadTime} minutes.");
            SendCustomEventDelayedSeconds("_LoadString", autoReloadTime * 60);
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
                SendCustomEventDelayedSeconds("_LoadString", 20);
            }
            else
            {
                MUIDebug.LogError($"{DEBUG_PREFIX}Couldn't load whitelist from {url} because {result.Error}!");
                AutoReload();
            }
        }
    }
}
