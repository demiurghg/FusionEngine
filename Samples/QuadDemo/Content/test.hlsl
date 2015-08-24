/*-----------------------------------------------------------------------------
	Sprite batch shader :
-----------------------------------------------------------------------------*/


struct BATCH {
	float4x4	Transform		;
};

struct VS_IN {
	float3 pos : POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
};


#if 0
$ubershader +USE_VERTEX_COLOR +USE_TEXTURE
#endif

cbuffer 		CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Texture 	: 	register(t0);

 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/

PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;
 
	output.pos = mul( float4(input.pos,1), Batch.Transform);
	output.col = input.col;
	output.tc  = input.tc;
	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{
	float4	color	=	float4(1,1,1,1);
	#ifdef USE_VERTEX_COLOR
		color	=	input.col;
	#endif
	#ifdef USE_TEXTURE
		color	*=	Texture.Sample( Sampler, input.tc );	
	#endif
	
	return color;
}

