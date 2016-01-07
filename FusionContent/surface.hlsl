
struct BATCH {
	float4x4	Projection		;
	float4x4	View			;
	float4x4	World			;
	float4		ViewPos			;
	float4		BiasSlopeFar	;
};


struct MATERIAL {
	float ColorLevel;
	float SpecularLevel;
	float EmissionLevel;
	float RoughnessMinimum;
	float RoughnessMaximum;
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
	float3 	WorldPos	: TEXCOORD5;
};

struct GBuffer {
	float4	hdr		 	: SV_Target0;
	float4	diffuse	 	: SV_Target1;
	float4	specular 	: SV_Target2;
	float4	normals	 	: SV_Target3;
	float4	scattering	: SV_Target4;
};

cbuffer 		CBBatch 	: 	register(b0) { BATCH    Batch     : packoffset( c0 ); }	
cbuffer 		CBLayer 	: 	register(b1) { MATERIAL Material : packoffset( c0 ); }	
SamplerState	Sampler		: 	register(s0);
Texture2D		Textures[16]		: 	register(t0);

#if 0
$ubershader GBUFFER RIGID|SKINNED
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
	output.WorldPos		=	wPos.xyz;
	
	return output;
}

 
/*-----------------------------------------------------------------------------
	Pixel shader :
-----------------------------------------------------------------------------*/

struct SURFACE {
	float3 	Diffuse;
	float3 	Specular;
	float	Roughness;
	float3	Normal;
	float3	Emission;
};

//	https://www.marmoset.co/toolbag/learn/pbr-theory	
//	This means that in theory conductors will not show any evidence of diffuse light. 
//	In practice however there are often oxides or other residues on the surface of a 
//	metal that will scatter some small amounts of light.	
	
SURFACE MaterialCombiner ( float2 uv )
{
	SURFACE surface;
	
	MATERIAL mtrl =	Material;
	
	//uv = uv * layerData.Tiling.xy + layerData.Offset.xy;
	
	float4 color		=	Textures[0].Sample( Sampler, uv ).rgba;
	float4 surfMap		=	Textures[1].Sample( Sampler, uv ).rgba;
	float4 normalMap	=	Textures[2].Sample( Sampler, uv ).rgba * 2 - 1;
	float4 emission		=	Textures[3].Sample( Sampler, uv ).rgba;
	
	float3 metalS		=	color.rgb * (surfMap.r);
	float3 nonmetalS	=	float3(0.31,0.31,0.31) * surfMap.r;
	float3 metalD		=	color.rgb * (1-surfMap.r);
	float3 nonmetalD	=	color.rgb * (1-surfMap.r*0.31);// * 0.31;

	surface.Diffuse		=	lerp(nonmetalD, metalD, surfMap.b);
	surface.Specular	=	lerp(nonmetalS, metalS, surfMap.b);
	surface.Roughness	=	surfMap.g;
	surface.Normal		=	normalMap.xyz;
	surface.Emission	=	emission.rgb;

	surface.Diffuse		*=	Material.ColorLevel;
	surface.Emission 	*=	Material.EmissionLevel;
	surface.Specular	*=	Material.SpecularLevel;
	surface.Roughness 	= 	lerp( Material.RoughnessMinimum, Material.RoughnessMaximum, surface.Roughness );
	
	return surface;
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
		
	SURFACE surface;
	
	surface.Diffuse	=	0.5;
	surface.Specular	=	0.0;
	surface.Roughness = 0.1f;
	surface.Normal	= float3(0,0,1);
	surface.Emission = 0;

	surface	=	MaterialCombiner( input.TexCoord );
	
	//	NB: Multiply normal length by local normal projection on surface normal.
	//	Shortened normal will be used as Fresnel decay (self occlusion) factor.
	float3 worldNormal 	= 	normalize( mul( surface.Normal, tbnToWorld ).xyz ) * (0.5+0.5*surface.Normal.z);
	
	//	Use sRGB texture for better 
	//	diffuse/specular intensity distribution
	output.hdr			=	float4( surface.Emission, 0 );
	output.diffuse		=	float4( surface.Diffuse, 1 );
	output.specular 	=	float4( surface.Specular, surface.Roughness );
	output.normals		=	float4( worldNormal * 0.5f + 0.5f, 1 );
	output.scattering	=	0;//float4( float3(0.85,0.85,1.00) * 0.3, 0.33f );
	
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






