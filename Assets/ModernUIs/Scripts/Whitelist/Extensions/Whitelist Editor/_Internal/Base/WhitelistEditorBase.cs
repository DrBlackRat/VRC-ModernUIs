
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class WhitelistEditorBase : UdonSharpBehaviour
    {
        [Tooltip("Color of text on instantiated prefabs, e.g. Whitelisted Users")]
        [SerializeField] protected Color prefabTextColor;
        [Tooltip("Color of the background on instantiated prefabs, e.g. Whitelisted Users")]
        [SerializeField] protected Color prefabBackgroundColor;
        
        public Color PrefabTextColor
        {
            get => prefabTextColor;
            set => prefabTextColor = value;
        }

        public Color PrefabBackgroundColor
        {
            get => prefabBackgroundColor;
            set => prefabBackgroundColor = value;
        }


        
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
