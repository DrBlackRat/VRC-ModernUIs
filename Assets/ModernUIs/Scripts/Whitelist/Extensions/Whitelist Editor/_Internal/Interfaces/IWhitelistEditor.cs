
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class IWhitelistEditor : UdonSharpBehaviour
    {
        // "Interface" for WhitelistEditor
        // Not technically an interface but this is all we got in udon
        
        public virtual void _Add(string username)
        {
            Debug.LogError("THIS CLASS SHOULD BE IMPLEMENTED FIRST");
        }
        
        public virtual void _Remove(string username)
        {
            Debug.LogError("THIS CLASS SHOULD BE IMPLEMENTED FIRST");
        }
    }
}
