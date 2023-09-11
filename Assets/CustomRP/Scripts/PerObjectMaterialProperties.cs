using UnityEngine;

public class PerObjectMaterialProperties : MonoBehaviour
{
    private readonly static int _baseColorId = Shader.PropertyToID("_BaseColor");
    private readonly static int _cutoffId = Shader.PropertyToID("_Cutoff");
    private readonly static int _metallicId = Shader.PropertyToID("_Metallic");
    private readonly static int _smoothnessId = Shader.PropertyToID("_Smoothness");
    private readonly static int _emissionColorId = Shader.PropertyToID("_EmissionColor");
    private static MaterialPropertyBlock _block;

    [SerializeField]
    private Color _baseColor = Color.white;
    [SerializeField, Range(0f, 1f)]
    private float _cutoff = 0.5f;
    [SerializeField, Range(0f, 1f)]
    private float _metallic = 0f;
    [SerializeField, Range(0f, 1f)]
    private float _smoothness = 0.5f;
    [SerializeField, ColorUsage(false, true)]
    private Color _emissionColor = Color.black;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        _block ??= new MaterialPropertyBlock();
        _block.SetColor(_baseColorId, _baseColor);
        _block.SetFloat(_cutoffId, _cutoff);
        _block.SetFloat(_metallicId, _metallic);
        _block.SetFloat(_smoothnessId, _smoothness);
        _block.SetColor(_emissionColorId, _emissionColor);
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}
