using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private struct ShadowedDirectionalLight
    {
        public int VisibleLightIndex;
        public float SlopeScaleBias;
        public float NearPlaneOffset;
    }

    private const string _bufferName = "Shadows";
    private static readonly int _dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static readonly int _dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static readonly int _dirShadowDataId = Shader.PropertyToID("_DirectionalShadowData");
    private static readonly int _cascadeCountId = Shader.PropertyToID("_CascadeCount");
    private static readonly int _cascadeCullingSphereId = Shader.PropertyToID("_CascadeCullingSpheres");
    private static readonly int _cascadeDataId = Shader.PropertyToID("_CascadeData");
    private static readonly int _shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

    private readonly CommandBuffer _buffer = new() { name = _bufferName };
    private ScriptableRenderContext _context;
    private CullingResults _cullingResults;
    private ShadowSettings _shadowSettings;

    private static readonly ShadowedDirectionalLight[] _shadowedDirectionalLights = new ShadowedDirectionalLight[ShadowSettings.MaxShadowedDirectionalLightCount];
    private static readonly Vector4[] _dirLightShadowData = new Vector4[LightSettings.MaxDirectionalLightCount];
    private static readonly Matrix4x4[] _dirShadowMatrices = new Matrix4x4[ShadowSettings.MaxShadowedDirectionalLightCount * ShadowSettings.MaxCascades];
    private static readonly Vector4[] _cascadeCullingSpheres = new Vector4[ShadowSettings.MaxCascades];
    private static readonly Vector4[] _cascadeData = new Vector4[ShadowSettings.MaxCascades];
    private int _shadowedDirectionalLightCount;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _context = context;
        _cullingResults = cullingResults;
        _shadowSettings = shadowSettings;

        _shadowedDirectionalLightCount = 0;
    }

    public void Cleanup()
    {
        _buffer.ReleaseTemporaryRT(_dirShadowAtlasId);
        ExecuteBuffer();
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (light.shadows == LightShadows.None ||
            light.shadowStrength == 0f ||
            !_cullingResults.GetShadowCasterBounds(visibleLightIndex, out _))
            return;
        
        if (_shadowedDirectionalLightCount < ShadowSettings.MaxShadowedDirectionalLightCount)
        {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount] = new()
            {
                VisibleLightIndex = visibleLightIndex,
                SlopeScaleBias = light.shadowBias,
                NearPlaneOffset = light.shadowNearPlane
            };
            _dirLightShadowData[_shadowedDirectionalLightCount] = new()
            {
                x = light.shadowStrength,
                y = light.shadowNormalBias,
            };
            _shadowedDirectionalLightCount++;
        }
    }

    public void Render()
    {
        RenderDirectionalShadows();
    }

    private void RenderDirectionalShadows()
    {
        if (_shadowedDirectionalLightCount <= 0)
        {
            /*
             *  Not claiming a texture will lead to problems for WebGL 2.0 because it binds textures and samplers together. 
             *  When a material with our shader is loaded while a texture is missing it will fail, 
             *  because it'll get a default texture which won't be compatible with a shadow sampler.
             */
            _buffer.GetTemporaryRT(_dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            return;
        }

        int atlasSize = (int)_shadowSettings.Directional.AtlasSize;
        _buffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        _buffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        _buffer.ClearRenderTarget(true, false, Color.clear);

        _buffer.BeginSample(_bufferName);
        ExecuteBuffer();

        int tiles = _shadowedDirectionalLightCount * _shadowSettings.Directional.CascadeCount;
        int split = tiles <= 1 ? 1 : (tiles <= 4 ? 2 : 4);
        int tileSize = atlasSize / split;
        for (int i = 0; i < _shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }
        _buffer.SetGlobalMatrixArray(_dirShadowMatricesId, _dirShadowMatrices);
        _buffer.SetGlobalVectorArray(_dirShadowDataId, _dirLightShadowData);
        _buffer.SetGlobalInt(_cascadeCountId, _shadowSettings.Directional.CascadeCount);
        _buffer.SetGlobalVectorArray(_cascadeCullingSphereId, _cascadeCullingSpheres);
        _buffer.SetGlobalVectorArray(_cascadeDataId, _cascadeData);

        float cascadeFade = 1f - _shadowSettings.Directional.CascadeFade;
        cascadeFade = 1f / (1f - cascadeFade * cascadeFade);
        _buffer.SetGlobalVector(_shadowDistanceFadeId, new Vector4(1f / _shadowSettings.MaxDistance, 1f / _shadowSettings.DistanceFade, cascadeFade));

        _buffer.EndSample(_bufferName);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = _shadowedDirectionalLights[index];
        var shadowDrawingSettings = new ShadowDrawingSettings(_cullingResults, light.VisibleLightIndex, BatchCullingProjectionType.Orthographic);
        var shadowSettings = _shadowSettings.Directional;

        for (int i = 0; i < shadowSettings.CascadeCount; i++)
        {
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.VisibleLightIndex, i, shadowSettings.CascadeCount, shadowSettings.CascadeRatio, tileSize, light.NearPlaneOffset, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
            
            // Culling spheres of all lights are equivalent.
            if (index == 0)
            {
                var cullingSphere = splitData.cullingSphere;
                SetCascadeData(i, cullingSphere, tileSize);
            }

            int tileIndex = index * shadowSettings.CascadeCount + i;
            Vector2 tileOffset = new(tileIndex % split, tileIndex / split);
            _dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, tileOffset, split);
            shadowDrawingSettings.splitData = splitData;
            _buffer.SetViewport(new Rect(tileOffset * tileSize, new Vector2(tileSize, tileSize)));
            _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            _buffer.SetGlobalDepthBias(0f, light.SlopeScaleBias);
            ExecuteBuffer();
            _context.DrawShadows(ref shadowDrawingSettings);
            _buffer.SetGlobalDepthBias(0f, 0f);
        }
    }

    private void SetCascadeData(int index, Vector4 cullingSphere, int tileSize)
    {
        _cascadeData[index].x = 1f / (cullingSphere.w * cullingSphere.w);
        _cascadeData[index].y = (2f * cullingSphere.w / tileSize) * 1.4142136f;

        cullingSphere.w *= cullingSphere.w;
        _cascadeCullingSpheres[index] = cullingSphere;
    }

    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
}