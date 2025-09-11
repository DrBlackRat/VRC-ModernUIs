using UnityEngine;
using UnityEngine.UI;

namespace DrBlackRat.VRC.ModernUIs
{
    public class RemoveLayoutComponents : MonoBehaviour
    {
        // This is just a marker for the OnPostProcessScene script
        [Tooltip("If enabled all Layout Components on this Game Object and it's children will be removed in Play Mode / Build. This can improve performance, but also break things.")]
        public bool removeLayoutComponents = true;

        [Tooltip("Layout Group Components that should not be removed.")]
        public LayoutGroup[] layoutGroupsToIgnore;
        [Tooltip("Layout Elements that should not be removed.")]
        public LayoutElement[] layoutElementsToIgnore;
        [Tooltip("Content Size Fitters that should not be removed.")]
        public ContentSizeFitter[] contentSizeFittersToIgnore;

    }
}
