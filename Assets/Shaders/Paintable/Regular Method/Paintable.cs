using UniRx;
using UnityEngine;
using System;

// Explanations of the goal of this script in PaintManager.cs
public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;


    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;
    Renderer rend;

    public RenderTexture getMask() => maskRenderTexture;
    public RenderTexture getSupport() => supportTexture;
    public Renderer getRenderer() => rend;

    void Start()
    {

        //// ORIGINAL SOLUTION
        // Generate the support texture, mask texture and renderer component of the gameobject that will be used by the PaintManger
        maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
        rend = GetComponent<Renderer>();

        // Seting the mask texture as the MaskTex of the gameobject material shader. This is needed so this shader can interpolate
        // between the intial main texture and this mask texture, giving more weight to the mask where its alpha value is closer to
        // 1. The mask texture basically accumulates all the paints that are done through the TextureMasPainter shader.
        rend.material.SetTexture("_MaskTex", maskRenderTexture);

    }

    void OnDisable()
    {
        maskRenderTexture.Release();
        supportTexture.Release();
    }
}