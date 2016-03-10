//-------------------------------------------------------------------------------
//	HBAO shader:
//	http://www.derschmale.com/source/hbao/HBAOFragmentShader.hlsl
//	http://www.derschmale.com/source/hbao/HBAOVertexShader.hlsl
//-------------------------------------------------------------------------------

#if 0
$ubershader SSAO
#endif

#define MaxSamples 128

struct SSAOParams {
	float4x4	View;
	float4x4	Projection;
	float4x4	InverseProjection;
};

cbuffer CBSSAOParams : register(b0) { 
	SSAOParams Params : packoffset(c0); 
};

cbuffer CBRandomDirs : register(b1) {
	float4	RandomDirs[MaxSamples] : packoffset(c0);
}

struct VertexOutput {
	float4	Position : SV_POSITION;
	float4	PostProj : POSITION;
};

SamplerState	SamplerNearestClamp : register(s0);
SamplerState	SamplerLinearClamp  : register(s1);

//-------------------------------------------------------------------------------
//	Utils :
//-------------------------------------------------------------------------------

float DepthToViewZ(float depthValue) {
	return Params.Projection[3][2] / (depthValue + Params.Projection[2][2]);
}

float3 DepthToWorldPos ( float depthValue, float2 postProjCoord )
{
	float4	projPos		=	float4( postProjCoord.xy, depthValue, 1 );
	float4	worldPos	=	mul( projPos, Params.InverseProjection );
			worldPos	/=	worldPos.w;
	return 	worldPos.xyz;
}


float OcclusionFunction ( float originZ, float sampledZ )
{
	// originZ		=	abs(originZ);
	// sampledZ	=	abs(sampledZ);
	
	if ((sampledZ-originZ)>0) {
		return 0;
	}
	
	return exp( -(originZ - sampledZ)*2 );
}

//-------------------------------------------------------------------------------
//	Shaders :
//-------------------------------------------------------------------------------

#ifdef SSAO

Texture2D	Source : register(t0);

VertexOutput VSMain(uint VertexID : SV_VertexID)
{
	VertexOutput output;
	output.Position	=	float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
	output.PostProj	=	float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
	return output;
}





float4 PSMain(VertexOutput input) : SV_Target
{
	float2 position	=	input.Position.xy;
	float2 postProj	=	input.PostProj.xy;
	float depth		=	Source.Load(int3(position.xy, 0)).r;
	float viewZ		=	DepthToViewZ( depth );
	
	float3 worldPos	=	DepthToWorldPos( depth, postProj );
	
	float totalOcclusion	=	0;
	
	for (int i=0; i<MaxSamples; i++) {
		float4 dir 			= 	RandomDirs[i]*0.5f;
		float4 newWorldPos	=	float4(worldPos.xyz + dir.xyz, 1);
		float4 newProjPos	=	mul( newWorldPos, Params.Projection );
		newProjPos	/=	newProjPos.w;
		
		float2 	samplePoint		=	float2( newProjPos.x*0.5f+0.5f, -newProjPos.y*0.5f+0.5f );
		float	sampledDepth	=	Source.SampleLevel( SamplerLinearClamp, samplePoint, 0 ).r;
		float 	sampledViewZ	=	DepthToViewZ(sampledDepth); 
		
		float 	sampledOcc		=	OcclusionFunction( viewZ, sampledViewZ );
		totalOcclusion			+=	sampledOcc;
	}
	
	totalOcclusion /= MaxSamples;
	totalOcclusion = 1 - totalOcclusion;
	totalOcclusion = 2*totalOcclusion;
	
	totalOcclusion	=	clamp( totalOcclusion, 0, 1 );
	
	return 1;//float4( totalOcclusion.xxx, 1 );
}

#endif























