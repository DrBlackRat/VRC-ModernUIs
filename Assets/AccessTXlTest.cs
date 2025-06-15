
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Texel;


public class AccessTXlTest : UdonSharpBehaviour
{
    public AccessControlDynamicUserList list;
    public bool remove;

    public void AddLocal()
    {
        list._AddPlayer(Networking.LocalPlayer);
    }

    public void RemoveLocal()
    {
        list._RemovePlayer(Networking.LocalPlayer);
    }

    public override void Interact()
    {
        if (remove)
        {
            RemoveLocal();
        }
        else
        {
            AddLocal();
        }
    }
}
