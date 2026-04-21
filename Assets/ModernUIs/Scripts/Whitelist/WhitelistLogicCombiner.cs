using System;
using UdonSharp;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistLogicCombiner : WhitelistGetterBase
    {
        [Tooltip("Logic Mode that should be used to combine / evaluate the Whitelists." +
                 "\nRequire All: Requires the user to be on all provided whitelists." +
                 "\nRequire Exactly One: Requires the user to only be on one whitelist and not any more." + 
                 "\nSubtract: Requires exactly 2 whitelists, will subtract the last from the first whitelist.")]
        [SerializeField] private WhitelistLogicMode logicMode;
        [Tooltip("Whitelists that should be combined / evaluated using the selected Logic Mode.")]
        [SerializeField] protected WhitelistGetterBase[] whitelists;
        
        protected override void Start()
        {
            base.Start();

            if (logicMode == WhitelistLogicMode.Subtract && whitelists.Length != 2)
            {
                MUIDebug.LogError($"Whitelist Logic Combiner requires exactly 2 whitelists in subtract mode.", gameObject);
                return;   
            }
            
            foreach (var whitelist in whitelists)
            {
                whitelist._SetUpConnection((IUdonEventReceiver)this);
            }
        }
        
        public void _WhitelistUpdated()
        {
            if (whitelists == null || whitelists.Length == 0) return;
            whitelist.Clear();

            if (whitelists.Length == 1)
            {
                whitelist = whitelists[0]._GetUsersAsList();
                WhitelistUpdated();
                return;
            }

            switch (logicMode)
            {
                case WhitelistLogicMode.RequireAll:
                    RequireAll();
                    break;
                case WhitelistLogicMode.RequireExactlyOne:
                    RequireExactlyOne();
                    break;
                case WhitelistLogicMode.Subtract:
                    Subtract();
                    break;
                default:
                    MUIDebug.LogError($"Whitelist Logic Combiner: Unknown Logic Mode {logicMode}!", gameObject);
                    return;
            }
        }

        private void RequireAll()
        {
            if (whitelists == null || whitelists.Length < 2) return;
            
            var firstList = whitelists[0]._GetUsersAsList();
            for (int i = 0; i < firstList.Count; i++) // Goes through all users in first whitelist
            {
                name = firstList[i].String;
                var isOnAll = true;
                for (int j = 1; j < whitelists.Length; j++) // Checks if user is on all whitelists
                {
                    if (whitelists[j]._IsPlayerWhitelisted(name)) continue;
                    isOnAll = false;
                    break;
                }
                
                if (isOnAll)
                    whitelist.Add(name);
            }
            WhitelistUpdated();
        }
        
        private void RequireExactlyOne()
        {
            if (whitelists == null || whitelists.Length < 2) return;
            
            DataList usersChecked = new DataList();
            for (int i = 0; i < whitelists.Length; i++)
            {
                var users = whitelists[i]._GetUsersAsList();
                for (int j = 0; j < users.Count; j++)
                {
                    if (usersChecked.Contains(users[j])) continue;
                    usersChecked.Add(users[j]);
                    
                    var count = 1;
                    for (int k = 0; k < whitelists.Length; k++)
                    {
                        if (k == i) continue;
                        if (whitelists[k]._IsPlayerWhitelisted(users[j].String)) count++;
                        if (count > 1) break;
                    }
                    if (count == 1) whitelist.Add(users[j]);
                }
            }
            WhitelistUpdated();
        }

        private void Subtract()
        {
            if (whitelists == null || whitelists.Length != 2) return;
            
            var firstList = whitelists[0]._GetUsersAsList();
            var secondList = whitelists[1]._GetUsersAsList();

            for (int i = 0; i < firstList.Count; i++)
            {
                var userToken = firstList[i];
                if (secondList.Contains(userToken))
                    continue;
                whitelist.Add(userToken);
            }
            WhitelistUpdated();
        }
        
        public override DataList _GetUsersAddedBy(string username)
        {
            if (username == localName) 
                return whitelist.DeepClone();
            return new DataList();
        }

        public override int _GetUsersAddedByCount(string username)
        {
            if (username == localName) 
                return whitelist.Count;
            return 0;
        }

        public override string _GetWhoAdded(string username)
        {
            if (whitelist.Contains(username))
                return localName;
            return null;
        }
    }

    public enum WhitelistLogicMode
    {
        RequireAll,     // AND
        RequireExactlyOne, // XOR
        Subtract
    }
}
