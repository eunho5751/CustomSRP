﻿using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private static string _sampleName = "Render Camera";
    private static readonly ShaderTagId _unlitShaderTagId = new("SRPDefaultUnlit");
    private static readonly ShaderTagId _litShaderTagId = new("CustomLit");

    private CustomRenderPipeline _pipeline;
    private ScriptableRenderContext _context;
    private Camera _camera;
    private readonly CommandBuffer _buffer = new() { name = _sampleName };

    private readonly Lighting _lighting = new();

    public CameraRenderer(CustomRenderPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;

#if UNITY_EDITOR
        PrepareBuffer();
        PrepareForSceneWindow();
#endif
        if (!_camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParams))
        {
            return;
        }
        var shadowSettings = _pipeline.PipelineAsset.ShadowSettings;
        cullingParams.shadowDistance = Mathf.Min(shadowSettings.MaxDistance, _camera.farClipPlane);
        CullingResults cullingResults = _context.Cull(ref cullingParams);

        _buffer.BeginSample(_sampleName);
        ExecuteBuffer();
        _lighting.Setup(_context, cullingResults, shadowSettings);
        _buffer.EndSample(_sampleName);

        Setup();
        DrawVisibleGeometry(cullingResults, _pipeline.PipelineAsset.UseDynamicBatching, _pipeline.PipelineAsset.UseGPUInstancing);
#if UNITY_EDITOR
        DrawUnsupportedShaders(cullingResults);
        DrawGizmos();
#endif
        _lighting.Cleanup();
        Submit();
    }

    private void Setup()
    {
        _context.SetupCameraProperties(_camera);
        _buffer.ClearRenderTarget(_camera.clearFlags != CameraClearFlags.Nothing, 
            _camera.clearFlags == CameraClearFlags.SolidColor, 
            _camera.clearFlags == CameraClearFlags.SolidColor ? _camera.backgroundColor.linear : _camera.backgroundColor);
        _buffer.BeginSample(_sampleName);
        ExecuteBuffer();
    }

    private void DrawVisibleGeometry(CullingResults cullingResults, bool useDynamicBatching, bool useGPUInstancing)
    {
        // Opaque
        SortingSettings sortingSettings = new(_camera) { criteria = SortingCriteria.CommonOpaque };
        DrawingSettings drawingSettings = new(_unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = 
                PerObjectData.Lightmaps | 
                PerObjectData.LightProbe | 
                PerObjectData.LightProbeProxyVolume |
                PerObjectData.ShadowMask |
                PerObjectData.OcclusionProbe | 
                PerObjectData.OcclusionProbeProxyVolume |
                PerObjectData.ReflectionProbes
        };
        drawingSettings.SetShaderPassName(1, _litShaderTagId);
        FilteringSettings filteringSettings = new(RenderQueueRange.opaque);
        _context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        // Skybox
        if (_camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
        {
            _context.DrawSkybox(_camera);
        }

        // Transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private void Submit()
    {
        _buffer.EndSample(_sampleName);
        ExecuteBuffer();
        _context.Submit();
    }
}