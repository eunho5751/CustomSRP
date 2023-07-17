using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    private bool _useDynamicBatching;
    [SerializeField]
    private bool _useGPUInstancing;
    [SerializeField]
    private bool _useSRPBatcher;

    protected override RenderPipeline CreatePipeline()
    {
        var pipeline = new CustomRenderPipeline
        {
            UseDynamicBatching = _useDynamicBatching,
            UseGPUInstancing = _useGPUInstancing,
            UseSRPBatcher = _useSRPBatcher
        };
        return pipeline;
    }
}
