
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Economy;
using VRC.SDKBase;
using VRC.Udon;

public class ProductTextureSwap : UdonSharpBehaviour
{
    [SerializeField] private UdonProduct product;
    [SerializeField] private Material[] materials;
    [SerializeField] private string property;
    [SerializeField] private Texture texture;

    private Texture[] prevTexture;

    private void Start()
    {
        prevTexture = new Texture[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            prevTexture[i] = materials[i].GetTexture(property);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetTexture(property, prevTexture[i]);
        }
    }

    public override void OnPurchaseConfirmed(IProduct result, VRCPlayerApi player, bool purchased)
    {
        if (!player.isLocal) return;
            
        if (product == null) return;
        if (result.ID != product.ID) return;

        for (int i = 0; i < materials.Length; i++)
        {
            prevTexture[i] = materials[i].GetTexture(property);
            materials[i].SetTexture(property, texture);
        }
    }

    public override void OnPurchaseExpired(IProduct result, VRCPlayerApi player)
    {
        if (!player.isLocal) return;
            
        if (product == null) return;
        if (result.ID != product.ID) return;
        
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetTexture(property, prevTexture[i]);
        }
    }
}
