using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string _bufferName = "Lighting";

    private static readonly int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static readonly int _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static readonly int _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static readonly Vector4[] _dirLightColors = new Vector4[LightSettings.MaxDirectionalLightCount];
    private static readonly Vector4[] _dirLightDirections = new Vector4[LightSettings.MaxDirectionalLightCount];

    private readonly CommandBuffer _buffer = new() { name = _bufferName };
    private ScriptableRenderContext _context;
    private CullingResults _cullingResults;

    private readonly Shadows _shadows = new();

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _context = context;
        _cullingResults = cullingResults;

        _buffer.BeginSample(_bufferName);
        _shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        _shadows.Render();
        _buffer.EndSample(_bufferName);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    public void Cleanup()
    {
        _shadows.Cleanup();
    }

    private void SetupLights()
    {
        var visibleLights = _cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(i, in visibleLight);
                dirLightCount++;
                if (dirLightCount >= LightSettings.MaxDirectionalLightCount)
                    break;
            }
        }

        _buffer.SetGlobalInt(_dirLightCountId, dirLightCount);
        _buffer.SetGlobalVectorArray(_dirLightColorsId, _dirLightColors);
        _buffer.SetGlobalVectorArray(_dirLightDirectionsId, _dirLightDirections);
    }

    private void SetupDirectionalLight(int index, in VisibleLight visibleLight)
    {
        _dirLightColors[index] = visibleLight.finalColor;
        _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        _shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }
}