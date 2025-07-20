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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class WhitelistStringLoader : UdonSharpBehaviour
    {
        [SerializeField] private VRCUrl url;
        [FormerlySerializedAs("whitelistManager")] 
        [SerializeField] private WhitelistSetterBase whitelist;
        [Tooltip("If enabled the entire Whitelist will be replaced instead of just adding new users to it.")]
        [SerializeField] private bool replaceWhitelist;
        
        [Tooltip("If enabled, only the master will load and update the string for everyone. This means if they aren't the master they can't spoof this list externally, but if they are they can spoof it for everyone.")]
        [SerializeField] private bool masterMode;
        
        [Tooltip("If enabled, the whitelist will be redownloaded after a certain amount of time.")]
        [SerializeField] private bool autoReload = true;
        [Range(1, 60)]
        [Tooltip("Time after which the whitelist will be downloaded again in minutes.")]
        [SerializeField] private int autoReloadTime = 10;
        [Tooltip("If enabled the system will try to downloaded the whitelist again after 20s if it errored out.")]
        [SerializeField] private bool autoReloadOnError = true;
        
        private bool loading = false;
        private string prevString;

        [UdonSynced]
        private string netString;
        private VRCPlayerApi localPlayer;
        
        private const string DEBUG_PREFIX = "String Loader | ";

        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            if (!masterMode || localPlayer.IsOwner(gameObject))
            {
                _LoadString();
            }
        }
        
        public override void OnDeserialization()
        {
            ApplyString(netString, true);
        }
        
        public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
        {

            if (newOwner.IsOwner(gameObject) || newOwner.isMaster)
            {
                MUIDebug.Log($"{DEBUG_PREFIX}{requester.displayName} has made {newOwner.displayName} the new Network Owner.");
                return true; 
            }
            else 
            {
                MUIDebug.LogError($"{DEBUG_PREFIX}{requester.displayName} has tried to make {newOwner.displayName} the Network Owner, but isn't allowed to!"); 
                return false;
            }
        }
        
        public override void OnOwnershipTransferred(VRCPlayerApi newOwner)
        {
            if (masterMode && localPlayer == newOwner)
            {
                MUIDebug.Log($"{DEBUG_PREFIX}You are now the owner.");
                _LoadString();
            }
        }
        
        public void _LoadString()
        {
            if (loading || (masterMode && !localPlayer.IsOwner(gameObject)))
            {
                MUIDebug.LogWarning($"{DEBUG_PREFIX}You are either already loading this list, or are doing something nasty!");
                return;
            }
            loading = true;
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }
        
        private void ApplyString(string newString, bool fromNet)
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

            if (!fromNet)
            {
                if (!localPlayer.IsOwner(gameObject))
                    Networking.SetOwner(localPlayer, gameObject);
                
                netString = newString;
                RequestSerialization();
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
            ApplyString(result.Result, false);
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
