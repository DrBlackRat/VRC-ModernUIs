using UdonSharp;
using VRC.SDK3.Data;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using VRC.SDKBase;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AllPlayersWhitelist : WhitelistGetterBase
    {
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);

            if (!Utilities.IsValid(player) ||
                !player.IsValid())
            {
                return;
            }
            
            whitelist.Add(player.displayName);
            WhitelistUpdated();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            base.OnPlayerLeft(player);
            
            if (!Utilities.IsValid(player) ||
                !player.IsValid())
            {
                return;
            }
            
            whitelist.Remove(player.displayName);
            WhitelistUpdated();
        }

        #region Get Added By Overrides
        public override DataList _GetUsersAddedBy(string username)
        {
            return username == localName ? whitelist.DeepClone() : new DataList();
        }

        public override int _GetUsersAddedByCount(string username)
        {
            return username == localName ? whitelist.Count : 0;
        }

        public override string _GetWhoAdded(string username)
        {
            return whitelist.Contains(username) ? localName : null;
        }
        #endregion
    }
}
