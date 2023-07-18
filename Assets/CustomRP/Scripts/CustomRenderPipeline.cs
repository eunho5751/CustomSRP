using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    public CustomRenderPipeline(CustomRenderPipelineAsset pipelineAsset)
    {
        PipelineAsset = pipelineAsset;

        GraphicsSettings.useScriptableRenderPipelineBatching = pipelineAsset.UseSRPBatcher;
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

    public CustomRenderPipelineAsset PipelineAsset { get; }
}