#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

TEXTURE2D(_BaseMap);
TEXTURE2D(_MaskMap);
TEXTURE2D(_EmissionMap);
TEXTURE2D(_DetailMap);
SAMPLER(sampler_BaseMap);
SAMPLER(sampler_DetailMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _DetailMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
	UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionColor)
	UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
	UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
	UNITY_DEFINE_INSTANCED_PROP(float, _Occlusion)
	UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
	UNITY_DEFINE_INSTANCED_PROP(float, _Fresnel)
	UNITY_DEFINE_INSTANCED_PROP(float, _DetailAlbedo)
	UNITY_DEFINE_INSTANCED_PROP(float, _DetailSmoothness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float2 TransformBaseUV(float2 baseUV)
{
	float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
	return baseUV * baseST.xy + baseST.zw;
}

float2 TransformDetailUV(float2 detailUV)
{
    float4 detailST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DetailMap_ST);
    return detailUV * detailST.xy + detailST.zw;
}

float4 GetMask(float2 baseUV)
{
    return SAMPLE_TEXTURE2D(_MaskMap, sampler_BaseMap, baseUV);
}

float4 GetDetail(float2 detailUV)
{
    float4 map = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV);
    return map * 2.0 - 1.0;
}

float4 GetBase(float2 baseUV, float2 detailUV = 0.0)
{
	float4 map = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseUV);
	float4 color = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float detail = GetDetail(detailUV).r * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DetailAlbedo);
    float mask = GetMask(baseUV).b;
    map.rgb = lerp(sqrt(map.rgb), detail < 0.0 ? 0.0 : 1.0, abs(detail) * mask);
    map.rgb *= map.rgb;
	return map * color;
}

float GetOcclusion(float2 baseUV)
{
    float strength = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Occlusion);
	float occlusion = GetMask(baseUV).g;
    return lerp(occlusion, 1.0, strength);
}

float GetCutoff(float2 baseUV)
{
	return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff);
}

float GetMetallic(float2 baseUV)
{
	float metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    metallic *= GetMask(baseUV).r;
    return metallic;
}

float GetSmoothness(float2 baseUV, float2 detailUV = 0.0)
{
	float smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);
    smoothness *= GetMask(baseUV).a;
	
    float detail = GetDetail(detailUV).b * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DetailSmoothness);
    float mask = GetMask(baseUV).b;
    smoothness = lerp(smoothness, detail < 0.0 ? 0.0 : 1.0, abs(detail) * mask);
	
    return smoothness;
}

float GetFresnel(float2 baseuv)
{
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Fresnel);
}

float3 GetEmission(float2 baseUV)
{
	float4 map = SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, baseUV);
	float4 color = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _EmissionColor);
	return map.rgb * color.rgb;
}

#endif