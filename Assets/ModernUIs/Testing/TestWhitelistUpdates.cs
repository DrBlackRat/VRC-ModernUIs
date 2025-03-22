
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs;

public class TestWhitelistUpdates : UdonSharpBehaviour
{
    public string name;

    public WhitelistManager manager;

    public void _AddUser()
    {
        manager._AddUser(name);
    }
    
    public void _RemoveUser()
    {
        manager._RemoveUser(name);
    }
}
