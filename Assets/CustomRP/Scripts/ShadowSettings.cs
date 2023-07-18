using System;
using UnityEngine;

[Serializable]
public class ShadowSettings
{
    public const int MaxShadowedDirectionalLightCount = 4;

    public enum ShadowMapSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    [Serializable]
    public class DirectionalSettings
    {
        public ShadowMapSize AtlasSize = ShadowMapSize._1024;
    }

    [Min(0f)]
    public float MaxDistance = 100f;
    [SerializeField]
    private DirectionalSettings _directional;

    public DirectionalSettings Directional => _directional;
}
