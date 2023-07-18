#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define SHADOW_SAMPLER sampler_linear_clamp_compare

struct DirectionalShadowData
{
	float strength;
};

CBUFFER_START(_CustomShadows)
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
float4 _DirectionalShadowData[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER_CMP(SHADOW_SAMPLER);

DirectionalShadowData GetDirectionalShadowData(int tileIndex)
{
	DirectionalShadowData shadowData;
	shadowData.strength = _DirectionalShadowData[tileIndex].x;
	return shadowData;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(int tileIndex, Surface surfaceWS)
{
	DirectionalShadowData data = GetDirectionalShadowData(tileIndex);
	if (data.strength <= 0.0)
	{
		return 1.0f;
	}

	float3 positionSTS = mul(_DirectionalShadowMatrices[tileIndex], float4(surfaceWS.position, 1.0)).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);
	return lerp(1.0, shadow, data.strength);
}

#endif