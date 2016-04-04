/*-----------------------------------------------------------------------------
	Sprite batch shader :
-----------------------------------------------------------------------------*/

#if 0
$ubershader OPAQUE|ALPHA_BLEND|ALPHA_BLEND_PREMUL|ADDITIVE|SCREEN|MULTIPLY|NEG_MULTIPLY
#endif

struct BATCH {
	float4x4	Transform		;
	float4		ClipRectangle	;
	float4		MasterColor		;
};

struct VS_IN {
	float3 pos : POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
	int    id  : FRAME;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 tc  : TEXCOORD;
	int    id  : TEXCOORD1;
};

struct FRAME {
	float left;
	float top;
	float right;
	float bottom;
	float4 color;
};


cbuffer 		CBBatch 	: 	register(b0) { BATCH Batch : packoffset( c0 ); }	
cbuffer 		CBClipRects	: 	register(b1) { FRAME Frames[1024] : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Texture 	: 	register(t0);

 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/

PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;

	output.pos = mul( float4(input.pos,1), Batch.Transform);
	output.col = input.col * Batch.MasterColor;
	output.tc  = input.tc;
	output.id  = input.id;

	return output;
}


float4 PSMain( PS_IN input ) : SV_Target
{
	float2 	vpos	=	input.pos.xy;
	float4	tex		=	Texture.Sample( Sampler, input.tc );	
	
	FRAME  frame 	= 	Frames[ input.id ];
	float  clipVal 	= 	1;
	
	if ( vpos.x < frame.left || vpos.x > frame.right ) {
		clipVal = -1;
	}
	
	if ( vpos.y < frame.top || vpos.y > frame.bottom ) {
		clipVal = -1;
	}
	
	clip( clipVal );//*/
	
	return input.col * tex * frame.color;
}

