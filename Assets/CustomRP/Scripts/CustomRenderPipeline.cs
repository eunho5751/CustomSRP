using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    public CustomRenderPipeline()
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        CameraRenderer renderer = new(this);
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }

    public bool UseDynamicBatching { get; set; }
    public bool UseGPUInstancing { get; set; }
    public bool UseSRPBatcher
    {
        get => GraphicsSettings.useScriptableRenderPipelineBatching;
        set => GraphicsSettings.useScriptableRenderPipelineBatching = value;
    }
}