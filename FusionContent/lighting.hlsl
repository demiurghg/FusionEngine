
#if 0
$ubershader
#endif

static const float PI = 3.141592f;

#pragma warning(disable:3557)

/*-----------------------------------------------------------------------------
	Lighting models :
-----------------------------------------------------------------------------*/

#include "brdf.hlsl"

/*-----------------------------------------------------------------------------
	Cook-Torrance lighting model
-----------------------------------------------------------------------------*/

struct LightingParams {
	float4x4	View;
	float4x4	Projection;
	float4x4	InverseViewProjection;

	float4x4	CSMViewProjection0;
	float4x4	CSMViewProjection1;
	float4x4	CSMViewProjection2;
	float4x4	CSMViewProjection3;

	float4		ViewPosition;
	float4		DirectLightDirection;
	float4		DirectLightIntensity;
	float4		ViewportSize;
	
	float4		CSMFilterRadius;	//	for each cascade
	float4		AmbientColor;
	
	float4		Viewport;			//	x,y,w,h,
	float		ShowCSLoadOmni;
	float		ShowCSLoadEnv;
	float		ShowCSLoadSpot;
};


cbuffer CBLightingParams : register(b0) { 
	LightingParams Params : packoffset( c0 ); 
};

struct PS_IN {
    float4 position : SV_POSITION;
	float4 projPos  : TEXCOORD0;
};

struct OMNILIGHT {
	float4	PositionRadius;
	float4	Intensity;
	float4	ExtentMin;
	float4	ExtentMax;
};

struct ENVLIGHT {
	float4	Position;
	float4	Intensity;
	float4	ExtentMin;
	float4	ExtentMax;
	float4  InnerOuterRadius;
};

struct SPOTLIGHT {
	float4x4	ViewProjection;
	float4		PositionRadius;
	float4		IntensityFar;
	float4		ExtentMin;	// x,y, depth
	float4		ExtentMax;	// x,y, depth
	float4		MaskScaleOffset;
	float4		ShadowScaleOffset;
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


float3	ComputeCSM ( float4 worldPos );
float3	ComputeSpotShadow ( float4 worldPos, SPOTLIGHT spot );

/*-----------------------------------------------------------------------------------------------------
	OMNI light
-----------------------------------------------------------------------------------------------------*/
RWTexture2D<float4> hdrTexture  : register(u0); 
RWTexture2D<float4> hdrSSS 		: register(u1); 

//#ifdef __COMPUTE_SHADER__

//	warning X3584: race condition writing to shared memory detected, note that threads 
//	will be writing the same value, but performance may be diminished due to contention.
groupshared uint minDepthInt = 0xFFFFFFFF; 
groupshared uint maxDepthInt = 0;
groupshared uint visibleLightCount = 0; 
groupshared uint visibleLightCountSpot = 0; 
groupshared uint visibleLightCountEnv = 0; 
groupshared uint visibleLightIndices[1024];

#define BLOCK_SIZE_X 16 
#define BLOCK_SIZE_Y 16 
#define OMNI_LIGHT_COUNT 1024
#define SPOT_LIGHT_COUNT 256
#define ENV_LIGHT_COUNT 256
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
	float3 csmFactor	=	ComputeCSM( worldPos );
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
			float3 lightDir	 = position - worldPos.xyz;
			float  falloff	 = LinearFalloff( length(lightDir), radius );
			
			totalLight.xyz	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(normal.xyz, lightIndex), 6).rgb * diffuse.rgb * falloff;

			float3	F = Fresnel(dot(viewDirN, normal.xyz), specular.rgb) * saturate(fresnelDecay*4-3);
			float G = GTerm( specular.w, viewDirN, normal.xyz );

			//F = lerp( F, float3(1,1,1), Fc * pow(fresnelDecay,6) );
			//F = lerp( F, float3(1,1,1), F * saturate(fresnelDecay*4-3) );
			
			totalLight.xyz	+=	EnvMap.SampleLevel( SamplerLinearClamp, float4(reflect(-viewDir, normal.xyz), lightIndex), specular.w*6 ).rgb * F * falloff * G;
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
			
			float3 shadow	 = ComputeSpotShadow( worldPos, light );
			
			totalLight.rgb += shadow * falloff * Lambert ( normal.xyz,  lightDir, intensity, diffuse.rgb );
			totalLight.rgb += shadow * falloff * CookTorrance( normal.xyz, viewDirN, lightDir, intensity, specular.rgb, specular.a );
		}
	}

	//-----------------------------------------------------
	//	Ambient :
	//-----------------------------------------------------
	float4 ssao	=	OcclusionMap.SampleLevel(SamplerLinearClamp, location.xy/float2(width,height), 0 );
	
	totalLight	+=	(diffuse + specular) * Params.AmbientColor * ssao * fresnelDecay;// * pow(normal.y*0.5+0.5, 1);

	hdrTexture[dispatchThreadId.xy] = totalLight;
	hdrSSS[dispatchThreadId.xy] = totalSSS;
}


/*-----------------------------------------------------------------------------------------------------
	Direct Light
-----------------------------------------------------------------------------------------------------*/

float3	ComputeSpotShadow ( float4 worldPos, SPOTLIGHT spot )
{
	float4	projPos	=	mul( worldPos, spot.ViewProjection );
	projPos.xyw /= projPos.w;
	
	if ( abs(projPos.x)>1 || abs(projPos.y)>1 ) {
		return float3(0,0,0);
	}//*/
	
	float2	smSize;
	SpotShadowMap.GetDimensions( smSize.x, smSize.y );
	
	float4  maskSO		=	spot.MaskScaleOffset;
	float4  shadowSO	=	spot.ShadowScaleOffset;
	
	float2	offset	=	1 / smSize;
	float2	smUV	=	projPos.xy * shadowSO.xy + shadowSO.zw;
	float  	z  		= 	projPos.z / spot.IntensityFar.w;
	float  	shadow	=	0;
	
	float radius = Params.CSMFilterRadius.x;
	
	float3 mask	=	SpotMaskAtlas.SampleLevel( SamplerLinearClamp, projPos.xy * maskSO.xy + maskSO.zw, z ).rgb;
	
	for (int i=-1; i<2; i++) {
		for (int j=-1; j<2; j++) {
			float  x   = i/1.0f;
			float  y   = j/1.0f;
			float  sh  = SpotShadowMap.SampleCmpLevelZero( ShadowSampler, smUV + offset * radius * float2(x,y), z );
			shadow += sh / 9;
		}
	}//*/
	

	return shadow * mask;
}


static float2 poisonDisk[16] = {
	float2( 0.6471434f,  0.5442180f),
	float2( 0.6627925f, -0.0145980f),
	float2( 0.2094653f,  0.6861125f),
	float2( 0.01836824f, 0.1938052f),
	float2(-0.5083427f, -0.0543112f),
	float2(-0.1876637f, -0.4905864f),
	float2( 0.2701841f, -0.1667389f),
	float2(-0.5884321f,  0.5500614f),
	
	float2( 0.5244192f, -0.7732284f),
	float2( 0.1206752f, -0.9527515f),
	float2(-0.2352096f,  0.9127740f),
	float2(-0.9525819f,  0.2960428f),
	float2( 0.8872142f, -0.4135098f),
	float2(-0.9452454f, -0.1600218f),
	float2(-0.6495278f, -0.4626486f),
	float2(-0.4085272f, -0.8579809f)
};

float3	ComputeCSM ( float4 worldPos )
{
	float3	colorizer		= float3(1,1,1);
	float	shadowId	= -1;
	float4	smProj;
	float4 	smProj2;

	//	select cascade :
	smProj	   =  mul( worldPos, Params.CSMViewProjection3 );
	smProj.xy /=  smProj.w;	smProj.w   =  1;
	if (abs(smProj.x)<0.99 && abs(smProj.y)<0.99) { colorizer = float3(0,0,4); shadowId = 3; smProj2 = smProj; }

	smProj	   =  mul( worldPos, Params.CSMViewProjection2 );
	smProj.xy /=  smProj.w;	smProj.w   =  1;
	if (abs(smProj.x)<0.99 && abs(smProj.y)<0.99) { colorizer = float3(0,2,0); shadowId = 2; smProj2 = smProj; }
	
	smProj	   =  mul( worldPos, Params.CSMViewProjection1 );
	smProj.xy /=  smProj.w;	smProj.w   =  1;
	if (abs(smProj.x)<0.99 && abs(smProj.y)<0.99) { colorizer = float3(2,0,0); shadowId = 1; smProj2 = smProj; }

	smProj	   =  mul( worldPos, Params.CSMViewProjection0 );
	smProj.xy /=  smProj.w;	smProj.w   =  1;
	if (abs(smProj.x)<0.99 && abs(smProj.y)<0.99) { colorizer = float3(1,1,1); shadowId = 0; smProj2 = smProj; }

	//	compute UVs 
	float2	csmSize;
	CSMTexture.GetDimensions( csmSize.x, csmSize.y );
	
	float2	offset	=	1 / csmSize;
	float2	smUV	=	0.5 * (float2(1, -1) * smProj2.xy + float2(1, 1));
			smUV	=	smUV * float2(0.25,1) + float2( shadowId/4, 0 );
	
	float  z  		= 	smProj2.z;
	float  shadow	=	0;
	
	float radius = Params.CSMFilterRadius.x;
	
	for (int i=0; i<16; i++) {
		float  x   = poisonDisk[i].x;
		float  y   = poisonDisk[i].y;
		float  sh  = CSMTexture.SampleCmpLevelZero( ShadowSampler, smUV + offset * radius * float2(x,y), z );
		shadow += sh / 16;
	}
	
	if ( shadowId==-1 ) {
		shadow = 1;
	}

	#if 0
		return shadow * colorizer;
	#else
		return shadow;
	#endif
}








