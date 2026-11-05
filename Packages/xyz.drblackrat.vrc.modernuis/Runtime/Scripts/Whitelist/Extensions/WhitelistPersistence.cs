
using System;
using DrBlackRat.VRC.ModernUIs;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistPersistence : UdonSharpBehaviour
    {
        [Tooltip(
            "Data Key that will be used to save / load this whitelist, everything using Persistence should have a different Data Key.")]
        [SerializeField]
        private string dataKey = "CHANGE THIS";

        [Range(0f, 30f)]
        [Tooltip("Persistence Load delay, useful if you need to wait for the Admin Whitelist to finish updating etc.")]
        [SerializeField]
        private float loadDelay = 5f;

        [Tooltip(
            "Whitelist to save data from and restore data to. It will only restore for first player and if it is empty!")]
        [SerializeField]
        private WhitelistSetterBase whitelist;

        [Tooltip("Admin Whitelist which is allowed to restore & save the data. If empty, everyone can save.")]
        [SerializeField]
        private WhitelistGetterBase adminWhitelist;

        [Tooltip("Button that is used to save the whitelist.")] [SerializeField]
        private Button saveButton;

        [Tooltip("Game Object that is enabled if you are allowed to save.")] [SerializeField]
        private GameObject allowedText;

        [Tooltip("Game Object that is enabled if you aren't allowed to save.")] [SerializeField]
        private GameObject disallowedText;

        private string serializedUsers;

        private bool hasAccess;
        private VRCPlayerApi localPlayer;

        void Start()
        {
            // Reset
            UpdateAccess(false, true);

            // Setup
            localPlayer = Networking.LocalPlayer;

            if (adminWhitelist == null)
            {
                UpdateAccess(true);
            }
            else
            {
                adminWhitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_AdminWhitelistUpdated));
            }

            whitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_WhitelistUpdated));

        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            // Needs to be local, have access, be the first who joined and requires an empty whitelist.
            if (player != localPlayer) return;
            SendCustomEventDelayedSeconds(nameof(_DelayedLoad), loadDelay);
        }

        public void _DelayedLoad()
        {
            if (!hasAccess || !localPlayer.IsOwner(gameObject) || whitelist._GetCount() != 0) return;
            if (PlayerData.TryGetString(localPlayer, dataKey, out string result))
            {
                if (VRCJson.TryDeserializeFromJson(result, out DataToken resultList))
                {
                    MUIDebug.Log("Whitelist Persistence | Loading Data!");
                    whitelist._ReplaceWhitelist(resultList.DataList);
                }
            }
        }

        #region Whitelist Updates

        public void _WhitelistUpdated()
        {
            var users = whitelist._GetUsersAsList();
            if (VRCJson.TrySerializeToJson(users, JsonExportType.Minify, out DataToken result))
            {
                serializedUsers = result.String;
            }
        }

        public void _AdminWhitelistUpdated()
        {
            UpdateAccess(adminWhitelist._IsPlayerWhitelisted(localPlayer));
        }

        #endregion

        private void UpdateAccess(bool newHasAccess, bool skipSameCheck = false)
        {
            if (newHasAccess == hasAccess && !skipSameCheck) return;
            hasAccess = newHasAccess;

            saveButton.interactable = hasAccess;
            allowedText.SetActive(hasAccess);
            disallowedText.SetActive(!hasAccess);
        }


        public void _SaveData()
        {
            if (!hasAccess || serializedUsers == null) return;

            MUIDebug.Log("Whitelist Persistence | Saving Data!");
            PlayerData.SetString(dataKey, serializedUsers);
        }
    }
}
