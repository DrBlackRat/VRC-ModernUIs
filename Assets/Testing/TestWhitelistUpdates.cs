
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs;

public class TestWhitelistUpdates : UdonSharpBehaviour
{
    [TextArea]
    public string names;

    public WhitelistManager manager;

    public void _ButtonPressed()
    {
        manager._UpdateWhitelist(names);
    }
}
