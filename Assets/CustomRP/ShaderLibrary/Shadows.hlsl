#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4
#define SHADOW_SAMPLER sampler_linear_clamp_compare

struct PerFragmentShadowData
{
	int cascadeIndex;
	float strength;
};

struct DirectionalShadowData
{
	float strength;
	float normalBias;
};

CBUFFER_START(_CustomShadows)
	float4 _ShadowDistanceFade;
	int _CascadeCount;
	float4 _CascadeData[MAX_CASCADE_COUNT];
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
	float4 _DirectionalShadowData[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

TEXTURE2D(_DirectionalShadowAtlas);
SAMPLER_CMP(SHADOW_SAMPLER);

float GetFadedShadowStrength(float depth, float scale, float fade)
{
	return saturate((1.0 - depth * scale) * fade);
}

PerFragmentShadowData GetPerFragmentShadowData(Surface surfaceWS)
{
	PerFragmentShadowData data;
	data.strength = GetFadedShadowStrength(surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
	
	for (int i = 0; i < _CascadeCount; i++)
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
		if (distSqr < sphere.w)
		{
			if (i == _CascadeCount - 1)
			{
				data.strength = GetFadedShadowStrength(distSqr, _CascadeData[i].x, _ShadowDistanceFade.z);
			}
			break;
		}
	}
		
	if (i == _CascadeCount)
	{
		data.strength = 0;
	}
	data.cascadeIndex = i;
	return data;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex)
{
	DirectionalShadowData shadowData;
	shadowData.strength = _DirectionalShadowData[lightIndex].x;
	shadowData.normalBias = _DirectionalShadowData[lightIndex].y;
	return shadowData;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float GetDirectionalShadowAttenuation(int lightIndex, Surface surfaceWS)
{
	DirectionalShadowData dirShadowData = GetDirectionalShadowData(lightIndex);
	if (dirShadowData.strength <= 0.0)
	{
		return 1.0f;
	}
	
	PerFragmentShadowData perFragShadowData = GetPerFragmentShadowData(surfaceWS);
	dirShadowData.strength *= perFragShadowData.strength;

	float3 normalBias = surfaceWS.normal * (dirShadowData.normalBias * _CascadeData[perFragShadowData.cascadeIndex].y);
	int tileIndex = lightIndex * _CascadeCount + perFragShadowData.cascadeIndex;
	float3 positionSTS = mul(_DirectionalShadowMatrices[tileIndex], float4(surfaceWS.position + normalBias, 1.0)).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);
	return lerp(1.0, shadow, dirShadowData.strength);
}

#endif