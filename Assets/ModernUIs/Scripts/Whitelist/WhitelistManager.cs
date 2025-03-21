using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs
{
    [DefaultExecutionOrder(-10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistManager : UdonSharpBehaviour
    {
        [Header("Whitelist:")] 
        [TextArea] [Tooltip("Display Name of each User you would want to be on the whitelist. Use a new line for each user.")]
        [SerializeField] private string allowedUsers;

        private string[] whitelistNames;
        private UdonBehaviour[] connectedBehaviours;


        private void Start()
        {
            _UpdateWhitelist(allowedUsers);
        }

        public void _SetUpConnection(UdonBehaviour behaviour)
        {
            connectedBehaviours = connectedBehaviours.Add(behaviour);
        }
        
        public void _UpdateWhitelist(string whitelist)
        {
            allowedUsers = whitelist;
            whitelistNames = whitelist.Split(new string[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            MUIDebug.Log($"Whitelist Updated");
            
            if (connectedBehaviours == null) return;
            foreach (var behaviour in connectedBehaviours)
            {
                if (behaviour == null) continue;
                behaviour.SendCustomEvent(nameof(WhitelistSelectorUI._WhitelistUpdated));
            }
        }

        public bool _IsPlayerWhitelisted(VRCPlayerApi playerApi)
        {
            foreach (var name in whitelistNames)
            {
                if (name == playerApi.displayName) return true;
            }
            return false;
        }
    }
}
