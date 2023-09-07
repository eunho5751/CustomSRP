using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor _editor;
    private UnityEngine.Object[] _materials;
    private MaterialProperty[] _properties;
    private bool _showPresets;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUI.BeginChangeCheck();
        {
            base.OnGUI(materialEditor, properties);

            _editor = materialEditor;
            _materials = materialEditor.targets;
            _properties = properties;

            EditorGUILayout.Space();

            _showPresets = EditorGUILayout.Foldout(_showPresets, "Presets", true);
            if (_showPresets)
            {
                OpaquePreset();
                ClipPreset();
                FadePreset();
                TransparentPreset();
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            SetShadowCasterPass();
        }
    }

    private bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            _editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }

    private void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            Shadows = ShadowMode.On;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    private void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            Shadows = ShadowMode.Clip;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    private void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            Shadows = ShadowMode.Dither;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    private void TransparentPreset()
    {
        if (HasProperty("_PremultiplyAlpha") && PresetButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            Shadows = ShadowMode.Dither;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    private void SetShadowCasterPass()
    {
        MaterialProperty shadows = FindProperty("_Shadows", _properties, false);
        if (shadows == null || shadows.hasMixedValue)
            return;

        bool enabled = shadows.floatValue < (float)ShadowMode.Off;
        foreach (Material m in _materials)
        {
            m.SetShaderPassEnabled("ShadowCaster", enabled);
        }
    }

    private bool HasProperty(string name)
    {
        return FindProperty(name, _properties, false) != null;
    }

    private bool SetProperty(string name, float value)
    {
        var prop = FindProperty(name, _properties, false);
        if (prop != null)
        {
            prop.floatValue = value;
            return true;
        }
        return false;
    }

    private void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
    }

    private void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            Array.ForEach(_materials, m => (m as Material).EnableKeyword(keyword));
        }
        else
        {
            Array.ForEach(_materials, m => (m as Material).DisableKeyword(keyword));
        }
    }

    private bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }

    private bool PremultiplyAlpha
    {
        set => SetProperty("_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    private BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    private BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    private bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    private ShadowMode Shadows
    {
        set
        {
            if (SetProperty("_Shadows", (float)value))
            {
                SetKeyword("_SHADOWS_CLIP", value == ShadowMode.Clip);
                SetKeyword("_SHADOWS_DITHER", value == ShadowMode.Dither);
            }
        }
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach (Material m in _materials)
            {
                m.renderQueue = (int)value;
            }
        }
    }
}