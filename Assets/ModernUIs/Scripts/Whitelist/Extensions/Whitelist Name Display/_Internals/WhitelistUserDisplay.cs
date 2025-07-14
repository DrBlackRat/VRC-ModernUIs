using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUIs.Whitelist
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WhitelistUserDisplay : UdonSharpBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image background;

        public void _Setup(string username, Color textColor, Color backgroundColor)
        {
            text.text = username;
            text.color = textColor;
            background.color = backgroundColor;
        }
    }
}

