using UniRx;
using UnityEngine;
using System;

// Explanations of the goal of this script in PaintManager.cs
public class PaintableARDKMesh : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;


    RenderTexture maskRenderTexture;
    RenderTexture supportTexture;
    Renderer rend;

    public Renderer getRenderer() => rend;

    void Start()
    {
        // 
        rend = GetComponent<Renderer>();
    }

    void OnDisable()
    {
        maskRenderTexture.Release();
        supportTexture.Release();
    }
}