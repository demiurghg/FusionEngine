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
    float4 Position	: SV_POSITION	;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
};


cbuffer CBStage		: register(b0) 	{ ConstData Stage : packoffset( c0 ); }
cbuffer PolyStage	: register(b1) 	{ ModelConstData ModelStage; }

Texture2D		DiffuseMap		: register(t0);
SamplerState	Sampler			: register(s0);


#if 0
$ubershader VERTEX_SHADER PIXEL_SHADER DRAW_COLORED
#endif



#ifdef VERTEX_SHADER
VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT output;
	
	float4 tempPos 	= mul( float4(v.Position.xzy, 	1), ModelStage.World ) + float4(ModelStage.ViewPositionTransparency.xyz, 0);
	float4 normal	= mul( float4(v.Normal,			0), ModelStage.World );
	
	output.Position	= mul(float4(tempPos.xyz, 1), Stage.ViewProj);
	output.Normal 	= normalize(normal.xyz);
	output.Color 	= v.Color;
	output.Tangent 	= v.Tangent;
	output.Binormal = v.Binormal;
	output.TexCoord = v.TexCoord;

	return output;
}
#endif


#ifdef PIXEL_SHADER
float4 PSMain ( VS_INPUT input ) : SV_Target
{
	#ifdef DRAW_COLORED
	float4 color = input.Color;
	#endif

	float t = dot(normalize(float3(1.0f, 0.0f, 0.0f)), input.Normal);
	float v = 0.5 * (1 + abs(t));
 
	return float4(v * color.rgb, ModelStage.ViewPositionTransparency.a);
}
#endif
