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
        [Header("Settings:")]
        [SerializeField] private VRCUrl url;
        [SerializeField] private WhitelistManager whitelistManager;

        [Header("Auto Reload:")]
        [SerializeField] private bool autoReload = false;
        [Range(1, 60)]
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
            whitelistManager._UpdateWhitelist(newString);
        }
        private void AutoReload()
        {
            if (!autoReload) return;
            SendCustomEventDelayedSeconds("_LoadString", autoReloadTime * 60);
        }
        
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            loading = false;
            ApplyString(result.Result);
            AutoReload();
        }
        public override void OnStringLoadError(IVRCStringDownload result)
        {
            loading = false;
            AutoReload();
        }
    }
}
