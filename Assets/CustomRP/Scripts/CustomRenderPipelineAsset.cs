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
    
    [Space]

    [SerializeField]
    private ShadowSettings _shadowSettings;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(this);
    }

    public bool UseDynamicBatching
    {
        get => _useDynamicBatching;
        set => _useDynamicBatching = value;
    }

    public bool UseGPUInstancing
    {
        get => _useGPUInstancing;
        set => _useGPUInstancing = value;
    }

    public bool UseSRPBatcher
    {
        get => _useSRPBatcher;
        set => _useSRPBatcher = value;
    }

    public ShadowSettings ShadowSettings => _shadowSettings;
}
