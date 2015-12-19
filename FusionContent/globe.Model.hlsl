struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct ModelConstData {
	float4x4 	World;
	float4 		ViewPositionTransparency;
	float4 		OverallColor;
};

struct InstancingData {
	float4x4 	World;
};


struct VS_INPUT {
	float3 Position : POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT {
    float4 Position	: SV_POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
	float3 WPos		: TEXCOORD1;
};


cbuffer CBStage		: register(b0) 	{ ConstData Stage : packoffset( c0 ); }
cbuffer PolyStage	: register(b1) 	{ ModelConstData ModelStage; }

Texture2D		DiffuseMap	: register(t0);
SamplerState	Sampler		: register(s0);

StructuredBuffer<InstancingData> InstData : register(t1);


#if 0
$ubershader VERTEX_SHADER PIXEL_SHADER DRAW_COLORED +INSTANCED
$ubershader VERTEX_SHADER PIXEL_SHADER USE_OVERALL_COLOR +INSTANCED
$ubershader VERTEX_SHADER PIXEL_SHADER XRAY +INSTANCED +USE_OVERALL_COLOR
#endif



#ifdef VERTEX_SHADER
VS_OUTPUT VSMain ( VS_INPUT v 

#ifdef INSTANCED
, uint id : SV_InstanceID
#endif

)
{
	VS_OUTPUT output;
	
	float4x4 worldMatrix = ModelStage.World;
	
	#ifdef INSTANCED
		worldMatrix = mul(InstData[id].World, worldMatrix);
	#endif
	
	float4 tempPos 	= mul( float4(v.Position.xyz, 	1), worldMatrix ) + float4(ModelStage.ViewPositionTransparency.xyz, 0);
	float4 normal	= mul( float4(v.Normal.xyz,		0), worldMatrix );
	
	output.Position	= mul(float4(tempPos.xyz, 1), Stage.ViewProj);
	output.Normal 	= normalize(normal.xyz);
	output.Color 	= v.Color;
	output.Tangent 	= v.Tangent;
	output.Binormal = v.Binormal;
	output.TexCoord = v.TexCoord;

	output.WPos = tempPos.xyz;
	
	return output;
}
#endif


#ifdef PIXEL_SHADER
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float4 color = float4(0,0,0,0);

	#ifdef XRAY
		float3 ndir	= normalize(-input.WPos);
		
		float  ndot = abs(dot( ndir, input.Normal ));
		float  frsn	= pow(saturate(1.2f-ndot), 0.5);
		
		#ifdef USE_OVERALL_COLOR
			color = ModelStage.OverallColor;
		#else
			color = input.Color;
		#endif
		
		return frsn*float4(color.xyz, color.a);
	#else
		#ifdef DRAW_COLORED
			color = input.Color;
		#endif
		#ifdef USE_OVERALL_COLOR
			color = ModelStage.OverallColor;
		#endif
		
		float t = dot(normalize(float3(1.0f, 0.0f, 0.0f)), input.Normal);
		float v = 0.5 * (1 + abs(t));
	
		return float4(v * color.rgb, ModelStage.ViewPositionTransparency.a);
	#endif
}
#endif
