using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;


namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FancyWhitelistNameDisplay : UdonSharpBehaviour
    {
        [Tooltip("Prefab that will be instantiated for each user on the whitelist.")]
        [SerializeField] private GameObject usernamePrefab;
        [Tooltip("Transform of which the position and rotation will be used for the Prefab, as well as be it's parent.")]
        [SerializeField] private Transform usernameTransform;

        [SerializeField] private WhitelistManager whitelistManager;

        private DataDictionary whitelistDisplays = new DataDictionary();

        private void Start()
        {
            whitelistManager._SetUpConnection((IUdonEventReceiver)this);
        }
        
        public void _WhitelistUpdated()
        {
            // Add if not on whitelistedUsers
            var whitelist = whitelistManager._GetUsersAsList();
            for (var i = 0; i < whitelist.Count; i++)
            {
                if (whitelistDisplays.ContainsKey(whitelist[i].String)) continue;
                AddUsernameDisplay(whitelist[i].String);
            }
            
            // Remove if on whitelistedUsers but not on whitelist
            var keys = whitelistDisplays.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                if (whitelist.Contains(keys[i])) continue;
                RemoveUsernameDisplay(keys[i].String);
            }
        }

        private void AddUsernameDisplay(string username)
        {
            if (whitelistDisplays.ContainsKey(username)) return;
            
            var newUserObj = Instantiate(usernamePrefab, usernameTransform.position, usernameTransform.rotation, usernameTransform);
            newUserObj.transform.localScale = Vector3.one;

            var tmpComponent = newUserObj.GetComponentInChildren<TextMeshProUGUI>();
            tmpComponent.text = username;
            
            whitelistDisplays.Add(username, newUserObj);
        }
        
        private void RemoveUsernameDisplay(string username)
        {
            if (!whitelistDisplays.ContainsKey(username)) return;
            
            if (whitelistDisplays.TryGetValue(username, TokenType.Reference, out DataToken value))
            {
                var gameObject = (GameObject)value.Reference;
                Destroy(gameObject);
                whitelistDisplays.Remove(username);
            } 
            
        }
    }
}
