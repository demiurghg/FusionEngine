
#if 0
$ubershader SOLIDLIGHTING|PARTICLES
#endif

static const float PI = 3.141592f;

#pragma warning(disable:3557)

/*-----------------------------------------------------------------------------
	Lighting headers :
-----------------------------------------------------------------------------*/

#include "brdf.fxi"
#include "lighting.fxi"
#include "shadows.fxi"
#include "particles.fxi"

/*-----------------------------------------------------------------------------
	Cook-Torrance lighting model
-----------------------------------------------------------------------------*/

cbuffer CBLightingParams : register(b0) { 
	LightingParams Params : packoffset( c0 ); 
};


SamplerState			SamplerNearestClamp : register(s0);
SamplerState			SamplerLinearClamp : register(s1);
SamplerComparisonState	ShadowSampler	: register(s2);

Texture2D 			GBufferDiffuse 		: register(t0);
Texture2D 			GBufferSpecular 	: register(t1);
Texture2D 			GBufferNormalMap 	: register(t2);
Texture2D 			GBufferScatter 		: register(t3);
Texture2D 			GBufferDepth 		: register(t4);
Texture2D 			CSMTexture	 		: register(t5);
Texture2D 			SpotShadowMap 		: register(t6);
Texture2D 			SpotMaskAtlas		: register(t7);
StructuredBuffer<OMNILIGHT>	OmniLights	: register(t8);
StructuredBuffer<SPOTLIGHT>	SpotLights	: register(t9);
StructuredBuffer<ENVLIGHT>	EnvLights	: register(t10);
Texture2D 			OcclusionMap		: register(t11);
TextureCubeArray	EnvMap				: register(t12);
StructuredBuffer<PARTICLE> Particles	: register(t13);


float DepthToViewZ(float depthValue) {
	return Params.Projection[3][2] / (depthValue + Params.Projection[2][2]);
}

/*-----------------------------------------------------------------------------------------------------
	OMNI light
-----------------------------------------------------------------------------------------------------*/
RWTexture2D<float4> hdrTexture  : register(u0); 
RWTexture2D<float4> hdrSSS 		: register(u1); 
RWStructuredBuffer<float4> ParticleLighting : register(u2);

//#ifdef __COMPUTE_SHADER__

//	warning X3584: race condition writing to shared memory detected, note that threads 
//	will be writing the same value, but performance may be diminished due to contention.
groupshared uint minDepthInt = 0xFFFFFFFF; 
groupshared uint maxDepthInt = 0;
groupshared uint visibleLightCount = 0; 
groupshared uint visibleLightCountSpot = 0; 
groupshared uint visibleLightCountEnv = 0; 
groupshared uint visibleLightIndices[1024];


#define OMNI_LIGHT_COUNT 1024
#define SPOT_LIGHT_COUNT 256
#define ENV_LIGHT_COUNT 256


#ifdef SOLIDLIGHTING
#define BLOCK_SIZE_X 16 
#define BLOCK_SIZE_Y 16 
[numthreads(BLOCK_SIZE_X,BLOCK_SIZE_Y,1)] 
void CSMain( 
	uint3 groupId : SV_GroupID, 
	uint3 groupThreadId : SV_GroupThreadID, 
	uint  groupIndex: SV_GroupIndex, 
	uint3 dispatchThreadId : SV_DispatchThreadID) 
{
	//-----------------------------------------------------
	float width			=	Params.Viewport.z;
	float height		=	Params.Viewport.w;

	int3 location		=	int3( dispatchThreadId.x, dispatchThreadId.y, 0 );

	// WARNING : this reduces performance :
	//location.xy		=	min( location.xy, int2(width-1,height-1) );

	float4	diffuse 	=	GBufferDiffuse 	.Load( location );
	float4	specular  	=	GBufferSpecular .Load( location );
	float4	normal	 	=	GBufferNormalMap.Load( location ) * 2 - 1;
	float	depth 	 	=	GBufferDepth 	.Load( location ).r;
	float4	scatter 	=	GBufferScatter 	.Load( location );
	
	//	add half pixel to prevent visual detachment of ssao effect:
	float4 	ssao		=	OcclusionMap	.SampleLevel(SamplerLinearClamp, (location.xy + float2(0.5,0.5))/float2(width,height), 0 );
	
	
	float fresnelDecay	=	(length(normal.xyz) * 2 - 1);// * (1-0.5*specular.a);
	
	normal.xyz			=	normalize(normal.xyz);

	float4	projPos		=	float4( location.x/(float)width*2-1, location.y/(float)height*(-2)+1, depth, 1 );

	//float4	projPos		=	float4( input.projPos.xy / input.projPos.w, depth, 1 );
	float4	worldPos	=	mul( projPos, Params.InverseViewProjection );
			worldPos	/=	worldPos.w;
			
	float3	viewDir		=	Params.ViewPosition.xyz - worldPos.xyz;
	float3	viewDirN	=	normalize( viewDir );
	
	float4	totalLight	=	0;
	float4	totalSSS	=	float4( 0,0,0, scatter.w );
	
	//-----------------------------------------------------
	//	Direct light :
	//-----------------------------------------------------
	float3 csmFactor	=	ComputeCSM( worldPos, Params, ShadowSampler, CSMTexture, true );
	float3 lightDir		=	-normalize(Params.DirectLightDirection.xyz);
	float3 lightColor	=	Params.DirectLightIntensity.rgb;
	
	// if (worldPos.y<0) {
		// lightColor = lerp(lightColor, float3(0,0,0), pow(saturate(-worldPos.y/8),0.5) );
	// }

	float3 diffuseTerm	=	Lambert	( normal.xyz,  lightDir, lightColor, float3(1,1,1) );
	float3 diffuseTerm2	=	Lambert	( normal.xyz,  lightDir, lightColor, float3(1,1,1), 1 );
	totalLight.xyz		+=	csmFactor.rgb * diffuseTerm * diffuse.rgb;
	totalLight.xyz		+=	csmFactor.rgb * CookTorrance( normal.xyz,  viewDirN, lightDir, lightColor, specular.rgb, specular.a );
	
	totalSSS.rgb		+=	csmFactor.rgb * diffuseTerm2 * scatter.rgb;

	//-----------------------------------------------------
	//	Common tile-related stuff :
	//-----------------------------------------------------
	
	GroupMemoryBarrierWithGroupSync();

	uint depthInt = asuint(depth); 

	InterlockedMin(minDepthInt, depthInt); 
	InterlockedMax(maxDepthInt, depthInt); 
	GroupMemoryBarrierWithGroupSync(); 
	
	float minGroupDepth = asfloat(minDepthInt); 
	float maxGroupDepth = asfloat(maxDepthInt);	
	

	//-----------------------------------------------------
	//	OMNI LIGHTS :
	//-----------------------------------------------------
	
	if (1) {
		uint lightCount = OMNI_LIGHT_COUNT;
		
		uint threadCount = BLOCK_SIZE_X * BLOCK_SIZE_Y; 
		uint passCount = (lightCount+threadCount-1) / threadCount;
		
		for (uint passIt = 0; passIt < passCount; passIt++ ) {
		
			uint lightIndex = passIt * threadCount + groupIndex;
			
			OMNILIGHT ol = OmniLights[lightIndex];
			
			float3 tileMin = float3( groupId.x*BLOCK_SIZE_X,    		  groupId.y*BLOCK_SIZE_Y,    			minGroupDepth);
			float3 tileMax = float3( groupId.x*BLOCK_SIZE_X+BLOCK_SIZE_X, groupId.y*BLOCK_SIZE_Y+BLOCK_SIZE_Y, 	maxGroupDepth);
			
			if ( ol.ExtentMax.x > tileMin.x && tileMax.x > ol.ExtentMin.x 
			  && ol.ExtentMax.y > tileMin.y && tileMax.y > ol.ExtentMin.y 
			  && ol.ExtentMax.z > tileMin.z && tileMax.z > ol.ExtentMin.z ) 
			{
				uint offset; 
				InterlockedAdd(visibleLightCount, 1, offset); 
				visibleLightIndices[offset] = lightIndex;
			}
		}
		
		GroupMemoryBarrierWithGroupSync();
				
		totalLight.rgb += visibleLightCount * float3(0.5, 0.0, 0.0) * Params.ShowCSLoadOmni;
		
		for (uint i = 0; i < visibleLightCount; i++) {
		
			uint lightIndex = visibleLightIndices[i];
			OMNILIGHT light = OmniLights[lightIndex];

			float3 intensity = light.Intensity.rgb;
			float3 position	 = light.PositionRadius.rgb;
			float  radius    = light.PositionRadius.w;
			float3 lightDir	 = position - worldPos.xyz;
			float  falloff	 = LinearFalloff( length(lightDir), radius );
			
			totalLight.rgb += falloff * Lambert ( normal.xyz,  lightDir, intensity, diffuse.rgb );
			totalLight.rgb += falloff * CookTorrance( normal.xyz, viewDirN, lightDir, intensity, specular.rgb, specular.a );
		}
	}
	
	//-----------------------------------------------------
	//	ENVIRONMENT LIGHTS / RADIANCE CACHE :
	//-----------------------------------------------------
	
	GroupMemoryBarrierWithGroupSync();

	if (1) {
		uint lightCount = ENV_LIGHT_COUNT;
		
		uint threadCount = BLOCK_SIZE_X * BLOCK_SIZE_Y; 
		uint passCount = (lightCount+threadCount-1) / threadCount;
		
		for (uint passIt = 0; passIt < passCount; passIt++ ) {
		
			uint lightIndex = passIt * threadCount + groupIndex;
			
			ENVLIGHT el = EnvLights[lightIndex];
			
			float3 tileMin = float3( groupId.x*BLOCK_SIZE_X,    		  groupId.y*BLOCK_SIZE_Y,    			minGroupDepth);
			float3 tileMax = float3( groupId.x*BLOCK_SIZE_X+BLOCK_SIZE_X, groupId.y*BLOCK_SIZE_Y+BLOCK_SIZE_Y, 	maxGroupDepth);
			
			if ( el.ExtentMax.x > tileMin.x && tileMax.x > el.ExtentMin.x 
			  && el.ExtentMax.y > tileMin.y && tileMax.y > el.ExtentMin.y 
			  && el.ExtentMax.z > tileMin.z && tileMax.z > el.ExtentMin.z ) 
			{
				uint offset; 
				InterlockedAdd(visibleLightCountEnv, 1, offset); 
				visibleLightIndices[offset] = lightIndex;
			}
		}
		
		GroupMemoryBarrierWithGroupSync();
				
		totalLight.rgb += visibleLightCountEnv * float3(0.0, 0.5, 0.0) * Params.ShowCSLoadEnv;

		for (uint i = 0; i < visibleLightCountEnv; i++) {
		
			uint lightIndex = visibleLightIndices[i];
			ENVLIGHT light = EnvLights[lightIndex];

			float3 intensity = light.Intensity.rgb;
			float3 position	 = light.Position.rgb;
			float  radius    = light.InnerOuterRadius.y;
			float3 lightDir	 = position.xyz - worldPos.xyz;
			float  falloff	 = LinearFalloff( length(lightDir), radius );
			
			totalLight.xyz	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(normal.xyz, lightIndex), 6).rgb * diffuse.rgb * falloff * ssao.rgb;

			float3	F = Fresnel(dot(viewDirN, normal.xyz), specular.rgb) * saturate(fresnelDecay*4-3);
			float G = GTerm( specular.w, viewDirN, normal.xyz );

			//F = lerp( F, float3(1,1,1), Fc * pow(fresnelDecay,6) );
			//F = lerp( F, float3(1,1,1), F * saturate(fresnelDecay*4-3) );
			
			totalLight.xyz	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(reflect(-viewDir, normal.xyz), lightIndex), specular.w*6 ).rgb * F * falloff * G * ssao.rgb;
		}
	}

	//-----------------------------------------------------
	//	SPOT LIGHTS :
	//-----------------------------------------------------
	
	GroupMemoryBarrierWithGroupSync();
	
	if (1) {
		uint lightCount = SPOT_LIGHT_COUNT;
		uint lightIndex = groupIndex;
		
		SPOTLIGHT ol = SpotLights[lightIndex];
		
		float3 tileMin = float3( groupId.x*BLOCK_SIZE_X,    		  groupId.y*BLOCK_SIZE_Y,    			minGroupDepth);
		float3 tileMax = float3( groupId.x*BLOCK_SIZE_X+BLOCK_SIZE_X, groupId.y*BLOCK_SIZE_Y+BLOCK_SIZE_Y, 	maxGroupDepth);
		
		if ( lightIndex < 16 
		  && ol.ExtentMax.x > tileMin.x && tileMax.x > ol.ExtentMin.x 
		  && ol.ExtentMax.y > tileMin.y && tileMax.y > ol.ExtentMin.y 
		  && ol.ExtentMax.z > tileMin.z && tileMax.z > ol.ExtentMin.z )
		{
			uint offset; 
			InterlockedAdd(visibleLightCountSpot, 1, offset); 
			visibleLightIndices[offset] = lightIndex;
		}
		
		GroupMemoryBarrierWithGroupSync();
				
		totalLight.rgb += visibleLightCountSpot * float3(0, 0.0, 0.5)  * Params.ShowCSLoadSpot;

		for (uint i = 0; i < visibleLightCountSpot; i++) {
		
			uint lightIndex = visibleLightIndices[i];
			SPOTLIGHT light = SpotLights[lightIndex];

			float3 intensity = light.IntensityFar.rgb;
			float3 position	 = light.PositionRadius.rgb;
			float  radius    = light.PositionRadius.w;
			float3 lightDir	 = position - worldPos.xyz;
			float  falloff	 = LinearFalloff( length(lightDir), radius );
			
			float3 shadow	 = ComputeSpotShadow( worldPos, light, ShadowSampler, SamplerLinearClamp, SpotShadowMap, SpotMaskAtlas, Params.CSMFilterRadius.x );
			
			totalLight.rgb += shadow * falloff * Lambert ( normal.xyz,  lightDir, intensity, diffuse.rgb );
			totalLight.rgb += shadow * falloff * CookTorrance( normal.xyz, viewDirN, lightDir, intensity, specular.rgb, specular.a );
		}
	}

	//-----------------------------------------------------
	//	Ambient :
	//-----------------------------------------------------
	
	//totalLight	+=	(diffuse + specular) * Params.AmbientColor * ssao * fresnelDecay;// * pow(normal.y*0.5+0.5, 1);

	hdrTexture[dispatchThreadId.xy] = totalLight;
	hdrSSS[dispatchThreadId.xy] = totalSSS;
}
#endif

/*-----------------------------------------------------------------------------------------------------
	Direct Light
-----------------------------------------------------------------------------------------------------*/

#ifdef PARTICLES
#define BLOCK_SIZE 256
[numthreads(BLOCK_SIZE,1,1)] 
void CSMain( 
	uint3 groupId : SV_GroupID, 
	uint3 groupThreadId : SV_GroupThreadID, 
	uint  groupIndex: SV_GroupIndex, 
	uint3 dispatchThreadId : SV_DispatchThreadID) 
{
		int id = dispatchThreadId.x;

	#if 0
		float4	worldPos	=	float4(Particles[id].Position, 1);
		
		float3 csmFactor	=	ComputeCSM( worldPos, Params, ShadowSampler, CSMTexture, false );
		float3 lightDir		=	-normalize(Params.DirectLightDirection.xyz);
		float3 lightColor	=	Params.DirectLightIntensity.rgb;
		
		float3 totalPrtLight	=	lightColor * csmFactor;

		
		for (int i=0; i<ENV_LIGHT_COUNT; i++) {
			ENVLIGHT light = EnvLights[i];

			float3 intensity = light.Intensity.rgb;
			float3 position	 = light.Position.rgb;
			float  radius    = light.InnerOuterRadius.y;
			float3 lightDir	 = position.xyz - worldPos.xyz;
			float  falloff	 = LinearFalloff( length(lightDir), radius );
			
			float3 	envFactor	=	0;
					envFactor	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(float3(1,0,0), i), 6).rgb;
					envFactor	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(float3(0,1,0), i), 6).rgb;
					envFactor	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(float3(0,0,1), i), 6).rgb;
					envFactor	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(float3(-1,0,0), i), 6).rgb;
					envFactor	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(float3(0,-1,0), i), 6).rgb;
					envFactor	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(float3(0,0,-1), i), 6).rgb;
							  
			totalPrtLight.xyz	+=	envFactor * falloff / 6;
		}
		
		ParticleLighting[id] = 	float4( totalPrtLight / 4 / 3.14f, 1 );

	#else
		ParticleLighting[id]	=	float4(1,1,1,1);
	#endif
	
}

#endif







