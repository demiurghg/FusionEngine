//-------------------------------------------------------------------------------
//	HBAO shader:
//	http://www.derschmale.com/source/hbao/HBAOFragmentShader.hlsl
//	http://www.derschmale.com/source/hbao/HBAOVertexShader.hlsl
//-------------------------------------------------------------------------------

#if 0
$ubershader SSAO
#endif

struct SSAOParams {
	float4x4	View;
	float4x4	Projection;
	float4x4	InverseViewProjection;
};

cbuffer CBSSAOParams : register(b0) { 
	SSAOParams Params : packoffset(c0); 
};

cbuffer CBRandomDirs : register(b1) {
	float4	RandomDirs[32] : packoffset(c0);
}

//-------------------------------------------------------------------------------
//	Utils :
//-------------------------------------------------------------------------------

float DepthToViewZ(float depthValue) {
	return Params.Projection[3][2] / (depthValue + Params.Projection[2][2]);
}

//-------------------------------------------------------------------------------
//	Shaders :
//-------------------------------------------------------------------------------

#ifdef SSAO

Texture2D	Source : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	float depth	=	Source.Load(int3(position.xy, 0)).r;
	float viewZ	=	DepthToViewZ( depth );
	return 1;
}

#endif

