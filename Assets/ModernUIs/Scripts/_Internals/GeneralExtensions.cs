using VRC.SDKBase;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class GeneralExtensions
    {
        /// <summary>
        /// Retrieves a player with the specified display name.
        /// </summary>
        /// <param name="displayName">The display name of the player to search for.</param>
        /// <returns>The <see cref="VRCPlayerApi"/> object of the player if found, otherwise null.</returns>
        public static VRCPlayerApi GetPlayerByDisplayName(string displayName)
        {
            var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
            VRCPlayerApi.GetPlayers(players);
            
            foreach (var player in players)
            {
                if (!player.IsValid()) continue;
                if (player.displayName == displayName) return player;
            }
            
            return null;
        }
    }
}