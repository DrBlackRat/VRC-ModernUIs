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

        public void _Setup(string username)
        {
            text.text = username;
        }
    }
}

