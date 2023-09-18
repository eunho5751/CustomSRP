#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#ifdef _DIRECTIONAL_PCF3
	#define DIRECTIONAL_FILTER_SAMPLES 4
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif _DIRECTIONAL_PCF5
	#define DIRECTIONAL_FILTER_SAMPLES 9
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif _DIRECTIONAL_PCF7
	#define DIRECTIONAL_FILTER_SAMPLES 16
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4
#define SHADOW_SAMPLER sampler_linear_clamp_compare

struct ShadowMask
{
	bool always;
    bool distance;
    float4 shadows;
};

struct PerFragmentShadowData
{
	int cascadeIndex;
    float cascadeBlend;
	float strength;
};

struct DirectionalShadowData
{
	int tileIndex;
	float strength;
	float normalBias;
};

CBUFFER_START(_CustomShadows)
	float4 _ShadowAtlasSize;
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
    data.cascadeBlend = 1.0;
    data.strength = GetFadedShadowStrength(surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
	
	for (int i = 0; i < _CascadeCount; i++)
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
		if (distSqr < sphere.w)
		{
            float fade = GetFadedShadowStrength(distSqr, _CascadeData[i].x, _ShadowDistanceFade.z);
			
			if (i == _CascadeCount - 1)
			{
                data.strength *= fade;
            }
			else
            {
                data.cascadeBlend = fade;
            }
			
			break;
		}
	}
		
	if (i == _CascadeCount)
	{
		data.strength = 0;
	}
	#if defined(_CASCADE_BLEND_DITHER)
		float4 clipPos = TransformWorldToHClip(surfaceWS.position);
		clipPos.xy /= clipPos.w;
		clipPos.xy = clipPos.xy * 0.5f + 0.5f;
		float2 ss = clipPos.xy * _ScreenParams.xy;
		float dither = InterleavedGradientNoise(ss, 0); 
		if (i != _CascadeCount && data.cascadeBlend < dither)
		{
			i += 1;
		}
	#endif
	#if !defined(_CASCADE_BLEND_SOFT)
		data.cascadeBlend = 1.0;
	#endif
	data.cascadeIndex = i;
	return data;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex)
{
	DirectionalShadowData shadowData;
	shadowData.tileIndex = _DirectionalShadowData[lightIndex].w;
	shadowData.strength = _DirectionalShadowData[lightIndex].x;
	shadowData.normalBias = _DirectionalShadowData[lightIndex].y;
	return shadowData;
}

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

float FilterDirectionalShadow(float3 positionSTS)
{
	#if defined(DIRECTIONAL_FILTER_SETUP)
			float weights[DIRECTIONAL_FILTER_SAMPLES];
			float2 positions[DIRECTIONAL_FILTER_SAMPLES];
			float4 size = _ShadowAtlasSize.yyxx;
			DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
			float shadow = 0;
			for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++)
			{
				shadow += weights[i] * SampleDirectionalShadowAtlas(float3(positions[i].xy, positionSTS.z));
			}
			return shadow;
	#else
		return SampleDirectionalShadowAtlas(positionSTS);
	#endif
}

float GetCascadedShadow(DirectionalShadowData dirShadowData, PerFragmentShadowData perFragShadowData, Surface surfaceWS)
{
    float3 normalBias = surfaceWS.normal * (dirShadowData.normalBias * _CascadeData[perFragShadowData.cascadeIndex].y);
	int tileIndex = dirShadowData.tileIndex + perFragShadowData.cascadeIndex;
    float3 positionSTS = mul(_DirectionalShadowMatrices[tileIndex], float4(surfaceWS.position + normalBias, 1.0)).xyz;
    float shadow = FilterDirectionalShadow(positionSTS);
    if (perFragShadowData.cascadeBlend < 1.0)
    {
        normalBias = surfaceWS.normal * (dirShadowData.normalBias * _CascadeData[perFragShadowData.cascadeIndex + 1].y);
        positionSTS = mul(_DirectionalShadowMatrices[tileIndex + 1], float4(surfaceWS.position + normalBias, 1.0)).xyz;
        shadow = lerp(FilterDirectionalShadow(positionSTS), shadow, perFragShadowData.cascadeBlend);
    }
    return shadow;
}

float GetBakedShadow(ShadowMask shadowMask)
{
    float shadow = 1.0;
	if (shadowMask.always || shadowMask.distance)
    {
        shadow = shadowMask.shadows.r;
    }
    return shadow;
}

float GetBakedShadow(ShadowMask shadowMask, float strength)
{
	if (shadowMask.always || shadowMask.distance)
    {
        return lerp(1.0, GetBakedShadow(shadowMask), strength);
    }
    return 1.0;
}

float MixBakedAndRealtimeShadows(ShadowMask shadowMask, float shadow, float fragShadowStrength, float lightShadowStrength)
{
    float baked = GetBakedShadow(shadowMask);
	if (shadowMask.always)
	{
		shadow = lerp(1.0, shadow, fragShadowStrength);
		shadow = min(baked, shadow);
		return lerp(1.0, shadow, lightShadowStrength);
	}
    else if (shadowMask.distance)
    {
        shadow = lerp(baked, shadow, fragShadowStrength);
        return lerp(1.0, shadow, lightShadowStrength);
    }
    return lerp(1.0, shadow, lightShadowStrength * fragShadowStrength);
}

float GetDirectionalShadowAttenuation(int lightIndex, Surface surfaceWS, ShadowMask shadowMask)
{
	#if !defined(_RECEIVE_SHADOWS)
		return 1.0;
	#endif
	
	DirectionalShadowData dirShadowData = GetDirectionalShadowData(lightIndex);
    PerFragmentShadowData perFragShadowData = GetPerFragmentShadowData(surfaceWS);
	if (dirShadowData.strength * perFragShadowData.strength <= 0.0)
	{
        return GetBakedShadow(shadowMask, abs(dirShadowData.strength));
    }
	else
    {
        float shadow = GetCascadedShadow(dirShadowData, perFragShadowData, surfaceWS);
        shadow = MixBakedAndRealtimeShadows(shadowMask, shadow, perFragShadowData.strength, dirShadowData.strength);
        return shadow;
    }
}

#endif