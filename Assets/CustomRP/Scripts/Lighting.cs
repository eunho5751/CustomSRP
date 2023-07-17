using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string _bufferName = "Lighting";
    private const int _maxDirLightCount = 4;

    private static readonly int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static readonly int _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static readonly int _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static readonly Vector4[] _dirLightColors = new Vector4[_maxDirLightCount];
    private static readonly Vector4[] _dirLightDirections = new Vector4[_maxDirLightCount];

    private readonly CommandBuffer _buffer = new() { name = _bufferName };

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
        _buffer.BeginSample(_bufferName);
        SetupLights(cullingResults);
        _buffer.EndSample(_bufferName);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private void SetupLights(CullingResults cullingResults)
    {
        var visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(i, in visibleLight);
                dirLightCount++;
                if (dirLightCount >= _maxDirLightCount)
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
    }
}