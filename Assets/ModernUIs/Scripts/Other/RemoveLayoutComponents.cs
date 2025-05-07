using UnityEngine;
using UnityEngine.Serialization;

namespace DrBlackRat.VRC.ModernUIs.Utils
{
    public class RemoveLayoutComponents : MonoBehaviour
    {
        // This is just a marker for the OnPostProcessScene script
        [Tooltip("If enabled all Layout Components on this Game Object and it's children will be removed in Play Mode / Build. This can improve performance, but also break things." +
                 "\nIncludes: ContentSizeFitter, VerticalLayoutGroup, HorizontalLayoutGroup, GirLayoutGroup and LayoutElement")]
        public bool removeLayoutComponents = true;
    }
}
