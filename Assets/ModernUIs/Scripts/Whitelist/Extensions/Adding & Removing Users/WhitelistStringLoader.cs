using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;


namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistStringLoader : UdonSharpBehaviour
    {
        [SerializeField] private VRCUrl url;
        [SerializeField] private WhitelistManager whitelistManager;
        
        [Tooltip("If enabled, the whitelist will be redownloaded after a certain amount of time.")]
        [SerializeField] private bool autoReload = false;
        [Range(1, 60)]
        [Tooltip("Time after which the whitelist will be downloaded again in minutes.")]
        [SerializeField] private int autoReloadTime = 10;
        
        private bool loading = false;

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
            whitelistManager._AddUsers(newString.Split(new string[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries));
        }
        private void AutoReload()
        {
            if (!autoReload) return;
            SendCustomEventDelayedSeconds("_LoadString", autoReloadTime * 60);
        }
        
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            MUIDebug.Log($"Whitelist from {url} loaded successfully!");
            loading = false;
            ApplyString(result.Result);
            AutoReload();
        }
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            MUIDebug.LogError($"Couldn't load whitelist from {url} because {result.Error}!");
            loading = false;
            AutoReload();
        }
    }
}
