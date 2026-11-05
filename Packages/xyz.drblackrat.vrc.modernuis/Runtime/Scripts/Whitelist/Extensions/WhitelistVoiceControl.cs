using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistVoiceControl : UdonSharpBehaviour
    {
        [SerializeField]
        [Tooltip("Whitelist which should be used to change the voice of players.")]
        private WhitelistGetterBase whitelist;
        [SerializeField]
        [Tooltip("If enabled, the Whitelist Voice Control will log all changes.")]
        private bool debugLogging = false;
        
        // Whitelisted Players
        [SerializeField]
        [Tooltip("The gain of the whitelisted players voice.")]
        private float whitelistedVoiceGain = 15f;
        [SerializeField]
        [Tooltip("The distance in meters from a whitelisted players head at which their voice starts to fall off.")]
        private float whitelistedNearDistance = 0f;
        [SerializeField]
        [Tooltip("The distance in meters from a whitelisted players head at which their voice is no longer heard.")]
        private float whitelistedFarDistance = 25f;
        [SerializeField]
        [Tooltip("The radius in meters of a whitelisted players \"voice bubble\", if set above 0 their voice will sound like it's coming from a larger source.")]
        private float whitelistedVolumetricRadius = 0f;
        [SerializeField]
        [Tooltip("If enabled, the whitelisted players voice will be low-passed when they are far away.")]
        private bool whitelistedLowpass = true;
        
        // NOT Whitelisted Players (Should be VRChat's Default Settings)
        [SerializeField]
        [Tooltip("The gain of the NOT whitelisted players voice.")]
        private float notWhitelistedVoiceGain = 15f;
        [SerializeField]
        [Tooltip("The distance in meters from a NOT whitelisted players head at which their voice starts to fall off.")]
        private float notWhitelistedNearDistance = 0f;
        [SerializeField]
        [Tooltip("The distance in meters from a NOT whitelisted players head at which their voice is no longer heard.")]
        private float notWhitelistedFarDistance = 25f;
        [SerializeField]
        [Tooltip("The radius in meters of a NOT whitelisted players \\\"voice bubble\\\", if set above 0 their voice will sound like it's coming from a larger source.")]
        private float notWhitelistedVolumetricRadius = 0f;
        [SerializeField]
        [Tooltip("If enabled, the NOT whitelisted players voice will be low-passed when they are far away.")]
        private bool notWhitelistedLowpass = true;
        
        private DataList _prevWhitelistedUsers = new DataList();
        private DataList _currentWhitelistedUsers = new DataList();
        private DataDictionary _playerCache = new DataDictionary(); // Key: DisplayName, Value: VRCPlayerApi
        private bool _hasInitialized;

        #region Unity, VRChat & ModernUIs Events
        private void Start()
        {
            FetchAllPlayers();
            
            whitelist._SetUpConnection(this, nameof(_OnWhitelistUpdated));
            _hasInitialized = true;
        }

        private void OnEnable()
        {
            if (_hasInitialized)
            {
                return;
            }
            
            FetchAllPlayers();
            for (int i = 0; i < _currentWhitelistedUsers.Count; i++)
            {
                if (_playerCache.TryGetValue(_currentWhitelistedUsers[i], TokenType.Reference, out DataToken playerAPIToken))
                {
                    SetWhitelistedVoice((VRCPlayerApi)playerAPIToken.Reference);
                }
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _currentWhitelistedUsers.Count; i++)
            {
                if (_playerCache.TryGetValue(_currentWhitelistedUsers[i], TokenType.Reference, out DataToken playerAPIToken))
                {
                    SetNotWhitelistedVoice((VRCPlayerApi)playerAPIToken.Reference);
                }
            }
            
            _playerCache.Clear();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            var displayNameToken = (DataToken)player.displayName;
            _playerCache.SetValue(displayNameToken, new DataToken(player));

            if (_currentWhitelistedUsers.Contains(displayNameToken))
            {
                SetWhitelistedVoice(player);
            }
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            _playerCache.Remove(player.displayName);
        }

        public void _OnWhitelistUpdated()
        {
            _prevWhitelistedUsers = _currentWhitelistedUsers;
            _currentWhitelistedUsers = whitelist._GetUsersAsList();
            UpdatePlayerVoices();
        }
        #endregion

        private void FetchAllPlayers()
        {
            var allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            foreach (var player in allPlayers)
            {
                _playerCache.SetValue((DataToken)player.displayName, new DataToken(player));
            }
        }

        #region Voice Adjustments
        private void UpdatePlayerVoices()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            for (int i = 0; i < _prevWhitelistedUsers.Count; i++)
            {
                var displayNameToken = _prevWhitelistedUsers[i];
                if (_currentWhitelistedUsers.Contains(displayNameToken))
                {
                    continue;
                }

                if (_playerCache.TryGetValue(displayNameToken, TokenType.Reference, out DataToken playerAPIToken))
                {
                    SetNotWhitelistedVoice((VRCPlayerApi)playerAPIToken.Reference);
                }
            }

            for (int j = 0; j < _currentWhitelistedUsers.Count; j++)
            {
                var displayNameToken = _currentWhitelistedUsers[j];
                if (_prevWhitelistedUsers.Contains(displayNameToken))
                {
                    continue;
                }

                if (_playerCache.TryGetValue(displayNameToken, TokenType.Reference, out DataToken playerAPIToken))
                {
                    SetWhitelistedVoice((VRCPlayerApi)playerAPIToken.Reference);
                }
            }
        }

        private void SetWhitelistedVoice(VRCPlayerApi player)
        {
            if (debugLogging)
            {
                MUIDebug.Log($"Whitelist Voice Control: {player.displayName} set to whitelisted.", gameObject);
            }
            
            player.SetVoiceGain(whitelistedVoiceGain);
            player.SetVoiceDistanceNear(whitelistedNearDistance);
            player.SetVoiceDistanceFar(whitelistedFarDistance);
            player.SetVoiceVolumetricRadius(whitelistedVolumetricRadius);
            player.SetVoiceLowpass(whitelistedLowpass);
        }
        
        private void SetNotWhitelistedVoice(VRCPlayerApi player)
        {
            if (debugLogging)
            {
                MUIDebug.Log($"Whitelist Voice Control: {player.displayName} set to NOT whitelisted.", gameObject);
            }
            
            player.SetVoiceGain(notWhitelistedVoiceGain);
            player.SetVoiceDistanceNear(notWhitelistedNearDistance);
            player.SetVoiceDistanceFar(notWhitelistedFarDistance);
            player.SetVoiceVolumetricRadius(notWhitelistedVolumetricRadius);
            player.SetVoiceLowpass(notWhitelistedLowpass);
        }
        #endregion
    }
}
