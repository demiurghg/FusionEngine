
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
	float3	Diffuse		;
	float	Alpha		;
	float3 	Specular	;
	float 	Roughness	;
	float3	Normal		;
	float3	Emission	;
};	

LAYERPROPS OverlayLayers ( LAYERPROPS layerA, LAYERPROPS layerB )
{
	LAYERPROPS layer;
	float factor = layerB.Alpha;
	
	layer.Diffuse	=	lerp( layerA.Diffuse	, layerB.Diffuse	, factor );
	layer.Alpha		=	lerp( layerA.Alpha		, layerB.Alpha		, factor );
	layer.Specular	=	lerp( layerA.Specular	, layerB.Specular	, factor );
	layer.Roughness	=	lerp( layerA.Roughness	, layerB.Roughness	, factor );
	layer.Normal	=	lerp( layerA.Normal		, layerB.Normal		, factor );
	layer.Emission	=	lerp( layerA.Emission	, layerB.Emission	, factor );
	
	return layer;
}
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
	
	float4	color		=	Textures[id*4+0].Sample( Sampler, uv ).rgba;
	float4	surface		=	Textures[id*4+1].Sample( Sampler, uv ).rgba;
	float4	normalMap	=	Textures[id*4+2].Sample( Sampler, uv ).rgba * 2 - 1;
	float4	emission	=	Textures[id*4+3].Sample( Sampler, uv ).rgba;
	
	float3 nonmetal		=	float3(80,80,80)/256.0f;

	layer.Diffuse		=	color.rgb * (1-surface.r);
	layer.Alpha			=	color.a;
	layer.Specular		=	lerp(nonmetal, color.rgb, surface.b) * surface.r;
	layer.Roughness		=	surface.g;
	layer.Normal		=	normalMap.xyz;
	layer.Emission		=	emission.rgb;
	
	layer.Diffuse	 	*=	layerData.ColorLevel;
	layer.Alpha 	 	*=	layerData.AlphaLevel;
	layer.Emission   	*=	layerData.EmissionLevel;
	layer.Specular	 	*=	layerData.SpecularLevel;
	layer.Roughness 	= 	lerp( layerData.RoughnessRange.x, layerData.RoughnessRange.y, layer.Roughness );
	
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
		
	LAYERPROPS layer;
	layer.Diffuse = 0;
	layer.Alpha = 0;
	layer.Specular = 0;
	layer.Roughness = 0;
	layer.Normal = 0;
	layer.Emission = 0;
	
	#ifdef LAYER0
		layer 	= ReadLayerUV( 0, input.TexCoord, tbnToWorld );
	#endif
	#ifdef LAYER1
		layer 	= OverlayLayers( layer, ReadLayerUV( 1, input.TexCoord, tbnToWorld ) );
	#endif
	#ifdef LAYER2
		layer 	= OverlayLayers( layer, ReadLayerUV( 2, input.TexCoord, tbnToWorld ) );
	#endif
	#ifdef LAYER3
		layer 	= OverlayLayers( layer, ReadLayerUV( 3, input.TexCoord, tbnToWorld ) );
	#endif
	
	float3 worldNormal 	= 	normalize( mul( layer.Normal, tbnToWorld ) );
	//FitNormalToGBuffer(worldNormal);

	output.hdr		=	float4( layer.Emission, 0 );
	output.diffuse	=	float4( layer.Diffuse, 1 );
	output.specular =	float4( layer.Specular, layer.Roughness );
	output.normals	=	float4( worldNormal * 0.5f + 0.5f, 1 );
	
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






