
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRCLinking.Modules;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(BasicWhitelist))]
    public class VRCLinkingToWhitelistBridge : VrcLinkingModuleBase
    {
        [SerializeField] private string[] roleIds;
        
        private BasicWhitelist whitelist;

        private void Start()
        {
            whitelist = gameObject.GetComponent<BasicWhitelist>();
        }

        public override void OnDataLoaded()
        {
            DataList allMembers = new DataList();
            foreach (var roleId in roleIds)
            {
                if (downloader.TryGetGuildMembersByRoleId(roleId, out DataList members))
                {
                    members.Contains("");
                    allMembers.AddRange(members);
                }
            }
            whitelist._ReplaceWhitelist(allMembers);
        }
    }
}
