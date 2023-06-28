using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        CameraRenderer renderer = new();
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }
}