
using System;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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
        [Tooltip("Whether to remove duplicates from the combined whitelist. This is slow, so only use it when really necessary!")]
        [SerializeField] private bool removeDuplicates = true;
        
        [FormerlySerializedAs("roleIds")] [Tooltip("IDs of Discord Roles that should be added the to whitelist.")]
        [SerializeField] private string[] discordRoleIds;
        [FormerlySerializedAs("roleNames")] [Tooltip("Names of Discord Roles that should be added the to whitelist.\nPreferably use Discord Role IDs as they will never change.")]
        [SerializeField] private string[] discordRoleNames;
        
        [Tooltip("IDs of the VRChat Group Roles that should be added to the whitelist.")]
        [SerializeField] private string[] groupRoleIds;
        [Tooltip("Names of the VRChat Group Roles that should be added to the whitelist.\nPreferably use Group Role IDs as they will never change.")]
        [SerializeField] private string[] groupRoleNames;
        
        private BasicWhitelist whitelist;

        private void Start()
        {
            whitelist = gameObject.GetComponent<BasicWhitelist>();
        }

        public override void OnDataLoaded()
        {
            DataList allMembers = new DataList();
            
            // Discord
            foreach (var roleId in discordRoleIds)
            {
                if (downloader.TryGetGuildMembersByRoleId(roleId, out DataList members))
                {
                    members.Contains("");
                    allMembers.AddRange(members);
                }
            }
            foreach (var roleName in discordRoleNames)
            {
                if (downloader.TryGetGuildMembersByRoleName(roleName, out DataList members))
                {
                    members.Contains("");
                    allMembers.AddRange(members);
                }
            }

            // VRChat Groups
            foreach (var groupId in groupRoleIds)
            {
                if (downloader.TryGetGroupMembersByRoleId(groupId, out DataList members))
                {
                    members.Contains("");
                    allMembers.AddRange(members);
                }
            }
            foreach (var groupName in groupRoleNames)
            {
                if (downloader.TryGetGroupMembersByRoleName(groupName, out DataList members))
                {
                    members.Contains("");
                    allMembers.AddRange(members);
                }
            }
            
            if (removeDuplicates)
                allMembers = allMembers.RemoveDuplicates();
            
            whitelist._ReplaceWhitelist(allMembers, null, null, true);
        }
    }
}
