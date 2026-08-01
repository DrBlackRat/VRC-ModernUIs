
using DrBlackRat.VRC.ModernUIs;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ForceZoneTest : UdonSharpBehaviour
{
    public ObjectMover objectMover;
    public int forceId;
    
    public void ForceZone()
    {
        objectMover.ForceZone(forceId);
    }
    
    public void UnforceZone()
    {
        objectMover.UnforceZone();
    }
}
