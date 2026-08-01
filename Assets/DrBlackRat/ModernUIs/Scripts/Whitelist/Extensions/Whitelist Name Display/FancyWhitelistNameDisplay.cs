using System;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        [Tooltip("Text Mesh Pro UGUI component that will have the amount of people on the whitelist displayed.")]
        [SerializeField] private TextMeshProUGUI countDisplay;
        
        [FormerlySerializedAs("whitelistManager")] 
        [SerializeField] private WhitelistGetterBase whitelist;
        
        [Tooltip("If enabled the the users shown will automatically be sorted alphabetically.\n This does come with a slight performance impact, otherwise you can call \"_SortUsers\" manually.")]
        [SerializeField] protected bool autoSort = true;

        private DataDictionary whitelistDisplays = new DataDictionary();
        protected bool collapsed;
        protected bool sortCooldownEnabled = true;

        private void Start()
        {
            whitelist._SetUpConnection((IUdonEventReceiver)this);
            
            SendCustomEventDelayedSeconds(nameof(_EndSortCooldown), 11f);
        }
        
        public void _EndSortCooldown()
        {
            sortCooldownEnabled = false;
            if (autoSort) _SortUsers();
        }
        
        public virtual void _UpdateCollapsed(bool newCollapsed)
        {
            collapsed = newCollapsed;

            var values = whitelistDisplays.GetValues();
            for (int i = 0; i < values.Count; i++)
            {
                ((GameObject)values[i].Reference).SetActive(!collapsed);
            }
        }
        
        public void _WhitelistUpdated()
        {
            var whitelist = this.whitelist._GetUsersAsList();
            
            // Remove if on whitelistedUsers but not on whitelist
            var keys = whitelistDisplays.GetKeys();
            for (var i = 0; i < keys.Count; i++)
            {
                if (whitelist.Contains(keys[i])) continue;
                RemoveUsernameDisplay(keys[i].String);
            }
            
            // Add if not on whitelistedUsers
            for (var i = 0; i < whitelist.Count; i++)
            {
                if (whitelistDisplays.ContainsKey(whitelist[i].String)) continue;
                AddUsernameDisplay(whitelist[i].String);
            }
            
            if (countDisplay != null)
                countDisplay.text = whitelistDisplays.Count.ToString();
        }

        private void AddUsernameDisplay(string username)
        {
            if (whitelistDisplays.ContainsKey(username)) return;
            
            var newUserObj = Instantiate(usernamePrefab, usernameTransform.position, usernameTransform.rotation, usernameTransform);
            newUserObj.transform.localScale = Vector3.one;

            var userDisplay = newUserObj.GetComponent<WhitelistUserDisplay>();
            userDisplay._Setup(username);
            newUserObj.SetActive(!collapsed);
            
            whitelistDisplays.Add(username, newUserObj);
            if (autoSort && !sortCooldownEnabled) _SortUsers();
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
        
        /// <summary>
        /// Sorts the users alphabetically.
        /// </summary>
        public void _SortUsers()
        {
            var userKeys = whitelistDisplays.GetKeys();
            userKeys.Sort();

            for (int i = 0; i < userKeys.Count; i++)
                ((GameObject)whitelistDisplays[userKeys[i]].Reference).transform.SetSiblingIndex(i + 2); // +2 as the first two elements are placeholder objects
        }
        
        
    }
}
