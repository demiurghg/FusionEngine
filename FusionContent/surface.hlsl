
struct BATCH {
	float4x4	Projection		;
	float4x4	View			;
	float4x4	World			;
	float4		ViewPos			;
	float4		BiasSlopeFar	;
};

struct LAYERDATA {
	float4	Tiling;
	float4	Offset;
	float2	RoughnessRange;
	float2	GlowNarrowness;

	float	ColorLevel;
	float	AlphaLevel;
	float	SpecularLevel;
	float	EmissionLevel;
	float	BumpLevel;
	float	Displacement;
	float	BlendHardness;
	
	float	Dummy;
};


struct VSInput {
	float3 Position : POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
};

struct PSInput {
	float4 	Position 	: SV_POSITION;
	float4 	Color 		: COLOR;
	float2 	TexCoord 	: TEXCOORD0;
	float3	Tangent 	: TEXCOORD1;
	float3	Binormal	: TEXCOORD2;
	float3	Normal 		: TEXCOORD3;
	float4	ProjPos		: TEXCOORD4;
};

struct GBuffer {
	float4	hdr		 : SV_Target0;
	float4	diffuse	 : SV_Target1;
	float4	specular : SV_Target2;
	float4	normals	 : SV_Target3;
};

cbuffer 		CBBatch 	: 	register(b0) { BATCH     Batch     : packoffset( c0 ); }	
cbuffer 		CBLayer 	: 	register(b1) { LAYERDATA Layers[4] : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Textures[16]		: 	register(t0);

//+DISPLACEMENT_MAPPING !!
#if 0
$ubershader GBUFFER (LAYER0..LAYER1..LAYER2..LAYER3)|TERRAIN|TRIPLANAR_SINGLE|TRIPLANAR_DOUBLE|TRIPLANAR_TRIPLE RIGID|SKINNED
$ubershader SHADOW RIGID|SKINNED
#endif
 
/*-----------------------------------------------------------------------------
	Vertex shader :
-----------------------------------------------------------------------------*/
PSInput VSMain( VSInput input )
{
	PSInput output;

	
	float4 	pos			=	float4( input.Position, 1 );
	float4	wPos		=	mul( pos,  Batch.World 		);
	float4	vPos		=	mul( wPos, Batch.View 		);
	float4	pPos		=	mul( vPos, Batch.Projection );
	float4	normal		=	mul( float4(input.Normal,0),  Batch.World 		);
	float4	tangent		=	mul( float4(input.Tangent,0),  Batch.World 		);
	float4	binormal	=	mul( float4(input.Binormal,0),  Batch.World 	);
	
	output.Position 	= 	pPos;
	output.ProjPos		=	pPos;
	output.Color 		= 	1;
	output.TexCoord		= 	input.TexCoord;
	output.Normal		= 	normalize(normal.xyz);
	output.Tangent 		=  	normalize(tangent.xyz);
	output.Binormal		=  	normalize(binormal.xyz);
	
	return output;
}

 
/*-----------------------------------------------------------------------------
	Pixel shader :
-----------------------------------------------------------------------------*/

struct LAYERPROPS {
	float4	Color;
	float4	Surface;
	float3	Normal;
	float3	Emission;
};

	// float4	Tiling;
	// float4	Offset;
	// float2	RoughnessRange;
	// float2	GlowNarrowness;

	// float	ColorLevel;
	// float	AlphaLevel;
	// float	SpecularLevel;
	// float	EmissionLevel;
	// float	BumpLevel;
	// float	Displacement;
	// float	BlendHardness;

LAYERPROPS ReadLayerUV ( int id, float2 uv, float3x3 tbnMatrix )
{
	LAYERPROPS layer;
	
	LAYERDATA	layerData =	Layers[id];
	
	uv = uv * layerData.Tiling.xy + layerData.Offset.xy;

	layer.Color		=	Textures[id*4+0].Sample( Sampler, uv ).rgba;
	layer.Surface	=	Textures[id*4+1].Sample( Sampler, uv ).rgba;
	layer.Normal	=	Textures[id*4+2].Sample( Sampler, uv ).xyz * 2 - 1;
	layer.Emission	=	Textures[id*4+3].Sample( Sampler, uv ).xyz;
	
	layer.Color.rgb	 *=	layerData.ColorLevel;
	layer.Color.a 	 *=	layerData.AlphaLevel;
	layer.Emission   *=	layerData.EmissionLevel;
	layer.Surface.r	 *=	layerData.SpecularLevel;
	layer.Surface.g  = 	lerp( layerData.RoughnessRange.x, layerData.RoughnessRange.y, layer.Surface.g );
	
	return layer;
}


#ifdef GBUFFER
GBuffer PSMain( PSInput input )
{
	GBuffer output;

	float3x3 tbnToWorld	= float3x3(
			input.Tangent.x,	input.Tangent.y,	input.Tangent.z,	
			input.Binormal.x,	input.Binormal.y,	input.Binormal.z,	
			input.Normal.x,		input.Normal.y,		input.Normal.z		
		);
	
	LAYERPROPS layer = ReadLayerUV( 0, input.TexCoord, tbnToWorld );
	
	//	decode specular :
	float3	white		=	float3(1,1,1);
	float  	roughness	=	layer.Surface.g;
	float3	specular	=	lerp( white, layer.Color.rgb, layer.Surface.b ) * layer.Surface.r;
	float3	diffuse		=	layer.Color.rgb * (1-layer.Surface.r);
			
	float3 worldNormal 	= 	normalize( mul( layer.Normal, tbnToWorld ) );

	output.hdr		=	float4( layer.Emission, 0 );
	output.diffuse	=	float4( diffuse, 1 );
	output.specular =	float4( specular, roughness );
	output.normals	=	float4( worldNormal * 0.5 + 0.5, 1 );
	
	return output;
}
#endif


#ifdef SHADOW
float4 PSMain( PSInput input ) : SV_TARGET0
{
	float z		= input.ProjPos.z / Batch.BiasSlopeFar.z;

	float dzdx	 = ddx(z);
	float dzdy	 = ddy(z);
	float slope = abs(dzdx) + abs(dzdy);

	return z + Batch.BiasSlopeFar.x + slope * Batch.BiasSlopeFar.y;
}
#endif






