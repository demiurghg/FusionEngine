struct BATCH {
	float4x4	Projection		;
	float4x4	View			;
	float4x4	World			;
	float4		ViewPos			;
	float4		BiasSlopeFar	;
	float4		Color;
};


struct MATERIAL {
	float 	ColorLevel;
	float 	SpecularLevel;
	float 	EmissionLevel;
	float 	RoughnessMinimum;
	float 	RoughnessMaximum;
	float	DirtLevel;
};


struct VSInput {
	float3 Position : POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
#ifdef SKINNED
    int4   BoneIndices  : BLENDINDICES0;
    float4 BoneWeights  : BLENDWEIGHTS0;
#endif	
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

cbuffer 		CBBatch 		: 	register(b0) { BATCH    Batch      : packoffset( c0 ); }	
cbuffer 		CBLayer 		: 	register(b1) { MATERIAL Material   : packoffset( c0 ); }	
cbuffer 		CBLayer 		: 	register(b2) { float4   UVMods[16] : packoffset( c0 ); }	
cbuffer 		CBBatch 		: 	register(b3) { float4x4 Bones[128] : packoffset( c0 ); }	
SamplerState	Sampler			: 	register(s0);
SamplerState	SamplerPoint	: 	register(s1);
Texture2D		Textures[4]		: 	register(t0);

#ifdef _UBERSHADER
$ubershader GBUFFER RIGID|SKINNED
$ubershader SHADOW RIGID|SKINNED
$ubershader FEEDBACK RIGID|SKINNED
#endif


 
/*-----------------------------------------------------------------------------
	Vertex shader :
	Note on prefixes:
		s - means skinned
		w - means world
		v - means view
		p - means projected
-----------------------------------------------------------------------------*/

float4x3 ToFloat4x3 ( float4x4 m )
{
	return float4x3( m._m00_m10_m20_m30, 
					 m._m01_m11_m21_m31, 
					 m._m02_m11_m22_m32 );
}

float4x4 AccumulateSkin( float4 boneWeights, int4 boneIndices )
{
	float4x4 result = boneWeights.x * Bones[boneIndices.x];
	result = result + boneWeights.y * Bones[boneIndices.y];
	result = result + boneWeights.z * Bones[boneIndices.z];
	result = result + boneWeights.w * Bones[boneIndices.w];
	// float4x3 result = boneWeights.x * ToFloat4x3( Bones[boneIndices.x] );
	// result = result + boneWeights.y * ToFloat4x3( Bones[boneIndices.y] );
	// result = result + boneWeights.z * ToFloat4x3( Bones[boneIndices.z] );
	// result = result + boneWeights.w * ToFloat4x3( Bones[boneIndices.w] );
	return result;
}

float4 TransformPosition( int4 boneIndices, float4 boneWeights, float3 inputPos )
{
	float4 position = 0; 
	
	float4x4 xform  = AccumulateSkin(boneWeights, boneIndices); 
	position = mul( float4(inputPos,1), xform );
	
	return position;
}


float4 TransformNormal( int4 boneIndices, float4 boneWeights, float3 inputNormal )
{
    float4 normal = 0;

	float4x4 xform  = AccumulateSkin(boneWeights, boneIndices); 
	normal = mul( float4(inputNormal,0), xform );
	
	return float4(normal.xyz,0);	// force w to zero
}



PSInput VSMain( VSInput input )
{
	PSInput output;

	#if RIGID
		float4 	pos			=	float4( input.Position, 1 );
		float4	wPos		=	mul( pos,  Batch.World 		);
		float4	vPos		=	mul( wPos, Batch.View 		);
		float4	pPos		=	mul( vPos, Batch.Projection );
		float4	normal		=	mul( float4(input.Normal,0),  Batch.World 		);
		float4	tangent		=	mul( float4(input.Tangent,0),  Batch.World 		);
		float4	binormal	=	mul( float4(input.Binormal,0),  Batch.World 	);
	#endif
	#if SKINNED
		float4 	sPos		=	TransformPosition	( input.BoneIndices, input.BoneWeights, input.Position	);
		float4  sNormal		=	TransformNormal		( input.BoneIndices, input.BoneWeights, input.Normal	);
		float4  sTangent	=	TransformNormal		( input.BoneIndices, input.BoneWeights, input.Tangent	);
		float4  sBinormal	=	TransformNormal		( input.BoneIndices, input.BoneWeights, input.Binormal	);
		
		float4	wPos		=	mul( sPos, Batch.World 		);
		float4	vPos		=	mul( wPos, Batch.View 		);
		float4	pPos		=	mul( vPos, Batch.Projection );
		float4	normal		=	mul( sNormal,  Batch.World 	);
		float4	tangent		=	mul( sTangent,  Batch.World 	);
		float4	binormal	=	mul( sBinormal,  Batch.World 	);
	#endif
	
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

//	Blend mode refernce:
//	http://www.deepskycolors.com/archivo/2010/04/21/formulas-for-Photoshop-blending-modes.html	

	
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
	surface.Emission 	*=	(Material.EmissionLevel * Batch.Color);
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
	
	surface.Diffuse		=	0.5;
	surface.Specular	=	0.0;
	surface.Roughness	= 	0.1f;
	surface.Normal		= 	float3(0,0,1);
	surface.Emission 	= 	0;

	surface.Diffuse		=	Textures[0].Sample( Sampler, input.TexCoord ).rgba;
	surface.Diffuse		=	Textures[1].Sample( SamplerPoint, input.TexCoord ).rgba;
	
	//	NB: Multiply normal length by local normal projection on surface normal.
	//	Shortened normal will be used as Fresnel decay (self occlusion) factor.
	float3 worldNormal 	= 	normalize( mul( surface.Normal, tbnToWorld ).xyz ) * (0.5+0.5*surface.Normal.z);
	worldNormal	=	input.Normal;
	
	//	Use sRGB texture for better 
	//	diffuse/specular intensity distribution
	output.hdr			=	float4( surface.Emission, 0 );
	output.diffuse		=	float4( surface.Diffuse, 1 );
	output.specular 	=	float4( 0,0,0,0.5 );
	output.normals		=	float4( worldNormal * 0.5f + 0.5f, 1 );
	output.scattering	=	0;//float4( float3(0.85,0.85,1.00) * 0.3, 0.33f );
	
	return output;
}
#endif

static const int VT_PAGE_COUNT = 128;
static const int VT_PAGE_SIZE  = 128;
static const int VT_MAX_MIP    = 6;

//
//	https://www.opengl.org/discussion_boards/showthread.php/171485-Texture-LOD-calculation-(useful-for-atlasing)
//
float MipLevel( float2 uv )
{
	float2 dx = ddx( uv * VT_PAGE_SIZE*VT_PAGE_COUNT );
	float2 dy = ddy( uv * VT_PAGE_SIZE*VT_PAGE_COUNT );
	float d = max( dot( dx, dx ), dot( dy, dy ) );

	// Clamp the value to the max mip level counts
	const float rangeClamp = pow(2, (VT_MAX_MIP - 1) * 2);
	d = clamp(d, 1.0, rangeClamp);

	float mipLevel = 0.5 * log2(d);
	mipLevel = floor(mipLevel);   

	return mipLevel;
}

#ifdef FEEDBACK
uint4 PSMain( PSInput input ) : SV_TARGET0
{
	int pageX	=	(int)floor( input.TexCoord.x * VT_PAGE_COUNT );
	int pageY	=	(int)floor( input.TexCoord.y * VT_PAGE_COUNT );
	int	mip		=	(int)MipLevel( input.TexCoord.xy );
	
	return uint4( pageX>>mip, pageY>>mip, mip, 4096 );
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



