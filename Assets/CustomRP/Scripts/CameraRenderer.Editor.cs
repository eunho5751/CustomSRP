#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private static readonly ShaderTagId[] _legacyShaderTagIds = {
        new("Always"),
        new("ForwardBase"),
        new("PrepassBase"),
        new("Vertex"),
        new("VertexLMRGBM"),
        new("VertexLM")
    };
    private static readonly Material _errorMaterial = new(Shader.Find("Hidden/InternalErrorShader"));
    
    private void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    private void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

    private void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        _buffer.name = _sampleName = _camera.name;
        Profiler.EndSample();
    }

    private void DrawUnsupportedShaders(CullingResults cullingResults)
    {
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;

        DrawingSettings drawingSettings = new();
        drawingSettings.overrideMaterial = _errorMaterial;
        drawingSettings.sortingSettings = new SortingSettings(_camera);
        for (int i = 0; i < _legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
        }

        _context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
}
#endif