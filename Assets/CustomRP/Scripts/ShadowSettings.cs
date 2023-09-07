using System;
using UnityEngine;

[Serializable]
public class ShadowSettings
{
    public const int MaxShadowedDirectionalLightCount = 4;
    public const int MaxCascades = 4;

    public enum ShadowMapSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    public enum FilterMode
    {
        PCF2x2,
        PCF3x3,
        PCF5x5,
        PCF7x7,
    }

    public enum CascadeBlendMode
    {
        Hard,
        Soft,
        Dither
    }

    [Serializable]
    public class DirectionalSettings
    {
        public ShadowMapSize AtlasSize = ShadowMapSize._1024;
        public FilterMode Filter = FilterMode.PCF2x2;
        public CascadeBlendMode CascadeBlend = CascadeBlendMode.Hard;

        [Range(1, 4)]
        public int CascadeCount = 4;
        [Range(0f, 1f)]
        public float CascadeRatio1 = 0.1f;
        [Range(0f, 1f)]
        public float CascadeRatio2 = 0.25f;
        [Range(0f, 1f)]
        public float CascadeRatio3 = 0.5f;
        [Range(0.001f, 1f)]
        public float CascadeFade = 0.1f;

        public Vector3 CascadeRatio => new(CascadeRatio1, CascadeRatio2, CascadeRatio3);
    }

    [Min(0.001f)]
    public float MaxDistance = 100f;
    [Range(0.001f, 1f)]
    public float DistanceFade = 0.1f;
    [SerializeField]
    private DirectionalSettings _directional;

    public DirectionalSettings Directional => _directional;
}
