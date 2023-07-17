using System;
using UnityEngine;

using Random = UnityEngine.Random;

public class MeshBall : MonoBehaviour
{
    private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField]
    private Mesh _mesh;
    [SerializeField]
    private Material _material;

    private Matrix4x4[] _matrices = new Matrix4x4[1023];
    private Vector4[] _baseColors = new Vector4[1023];

    private MaterialPropertyBlock _block;

    private void Awake()
    {
        for (int i = 0; i< _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, 
                Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f), 
                Vector3.one * Random.Range(0.5f, 1.5f));
            _baseColors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
        }

        _block = new MaterialPropertyBlock();
        _block.SetVectorArray(_baseColorId, _baseColors);
    }

    private void Update()
    {
        Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, 1023, _block);
    }
}