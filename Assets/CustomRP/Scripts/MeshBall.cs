using System;
using UnityEngine;
using UnityEngine.Rendering;

using Random = UnityEngine.Random;

public class MeshBall : MonoBehaviour
{
    private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int _metallicId = Shader.PropertyToID("_Metallic");
    private static readonly int _smoothnessId = Shader.PropertyToID("_Smoothness");

    [SerializeField]
    private Mesh _mesh;
    [SerializeField]
    private Material _material;
    [SerializeField]
    private LightProbeProxyVolume _lightProbeVolume;

    private Matrix4x4[] _matrices = new Matrix4x4[1023];

    private MaterialPropertyBlock _block;

    private void Awake()
    {
        _block = new MaterialPropertyBlock();

        var baseColors = new Vector4[1023];
        var metallics = new float[1023];
        var smoothnesses = new float[1023];
        var lightProbes = new SphericalHarmonicsL2[1023];

        for (int i = 0; i< _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, 
                Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f), 
                Vector3.one * Random.Range(0.5f, 1.5f));
            baseColors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
            metallics[i] = Random.value < 0.25f ? 1f : 0f;
            smoothnesses[i] = Random.Range(0.05f, 0.95f);
        }
        _block.SetVectorArray(_baseColorId, baseColors);
        _block.SetFloatArray(_metallicId, metallics);
        _block.SetFloatArray(_smoothnessId, smoothnesses);

        if (_lightProbeVolume != null)
        {
            var positions = new Vector3[1023];
            for (int i = 0; i < _matrices.Length; i++)
            {
                positions[i] = _matrices[i].GetColumn(3);
            }
            LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, lightProbes, null);
            _block.CopySHCoefficientArraysFrom(lightProbes);
        }
    }

    private void Update()
    {
        Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, 1023, _block,
            ShadowCastingMode.On, true, 0, null, _lightProbeVolume ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided, _lightProbeVolume);
    }
}