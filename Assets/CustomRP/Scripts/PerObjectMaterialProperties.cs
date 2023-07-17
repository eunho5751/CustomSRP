using UnityEngine;

public class PerObjectMaterialProperties : MonoBehaviour
{
    private readonly static int _baseColorId = Shader.PropertyToID("_BaseColor");
    private readonly static int _cutoffId = Shader.PropertyToID("_Cutoff");
    private static MaterialPropertyBlock _block;

    [SerializeField]
    private Color _baseColor = Color.white;
    [SerializeField, Range(0f, 1f)]
    private float _cutoff = 0.5f;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        _block ??= new MaterialPropertyBlock();
        _block.SetColor(_baseColorId, _baseColor);
        _block.SetFloat(_cutoffId, _cutoff);
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}
