#if 0
$ubershader FXAA|COPY|DOWNSAMPLE_4|OVERLAY_ADDITIVE
$ubershader (STRETCH_RECT..TO_CUBE_FACE)|(DOWNSAMPLE_2_4x4..TO_CUBE_FACE)
$ubershader GAUSS_BLUR_3x3 PASS1|PASS2
$ubershader GAUSS_BLUR PASS1|PASS2 +BILATERAL
$ubershader LINEARIZE_DEPTH|RESOLVE_AND_LINEARIZE_DEPTH_MSAA
$ubershader PREFILTER_ENVMAP POSX|POSY|POSZ|NEGX|NEGY|NEGZ
$ubershader FILL_ALPHA_ONE
#endif


//-------------------------------------------------------------------------------
#if defined(COPY) || defined(OVERLAY_ADDITIVE)

Texture2D	Source : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	return Source.Load(int3(position.xy, 0));
}

#endif

//-------------------------------------------------------------------------------
#ifdef FILL_ALPHA_ONE

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	return float4(0,0,0,1);
}

#endif

//-------------------------------------------------------------------------------
#ifdef STRETCH_RECT

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float2 uv : TEXCOORD0;
};


PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
	output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
	output.position.zw = 1.0f;

	output.uv 	=	output.position.xy * float2(0.5f, -0.5f) + 0.5f;

	#ifdef TO_CUBE_FACE
		output.uv 	= 	output.position.xy * float2(-0.5f, -0.5f) + 0.5f;
	#endif  

	return output;
}


float4 PSMain(PS_IN input) : SV_Target
{
	return Source.SampleLevel(SamplerLinearClamp, input.uv, 0);
}

#endif

//-------------------------------------------------------------------------------
#ifdef DOWNSAMPLE_2_4x4

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float4 uv0 : TEXCOORD0;
	float4 uv1 : TEXCOORD1;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position = float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);

#ifdef TO_CUBE_FACE
	float2 uv0 = output.position.xy * float2(-0.5f, -0.5f) + 0.5f;
#else
	float2 uv0 = output.position.xy * float2(0.5f, -0.5f) + 0.5f;
#endif  

	float texWidth, texHeight;
	Source.GetDimensions(texWidth, texHeight);

	float2 texelSize = float2(1.0f/texWidth, 1.0f/texHeight);

	output.uv0.xy = uv0 + texelSize * float2(-1.0f, -1.0f);
	output.uv0.zw = uv0 + texelSize * float2( 1.0f, -1.0f);
	output.uv1.xy = uv0 + texelSize * float2(-1.0f,  1.0f);
	output.uv1.zw = uv0 + texelSize * float2( 1.0f,  1.0f);

	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
	float4 sample0 = Source.SampleLevel(SamplerLinearClamp, input.uv0.xy, 0);
	float4 sample1 = Source.SampleLevel(SamplerLinearClamp, input.uv0.zw, 0);
	float4 sample2 = Source.SampleLevel(SamplerLinearClamp, input.uv1.xy, 0);
	float4 sample3 = Source.SampleLevel(SamplerLinearClamp, input.uv1.zw, 0);

	return 0.25f*(sample0 + sample1 + sample2 + sample3);
}

#endif

//-------------------------------------------------------------------------------
#ifdef DOWNSAMPLE_4

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
  PS_IN output;
  output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
  output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
  output.position.zw = 1.0f;

  float texWidth, texHeight;
  Source.GetDimensions(texWidth, texHeight);

  float2 texelSize1 = float2(1.0f / texWidth, 1.0f / texHeight);
  float2 texelSize2 = float2(texelSize1.x, -texelSize1.y);
  float2 uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

  output.uv0 = float4(uv + texelSize1, uv - texelSize1);
  output.uv1 = float4(uv + texelSize2, uv - texelSize2);

  return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
  float4 sample0 = Source.SampleLevel(SamplerLinearClamp, input.uv0.xy, 0);
  float4 sample1 = Source.SampleLevel(SamplerLinearClamp, input.uv0.zw, 0);
  float4 sample2 = Source.SampleLevel(SamplerLinearClamp, input.uv1.xy, 0);
  float4 sample3 = Source.SampleLevel(SamplerLinearClamp, input.uv1.zw, 0);

	return 0.25f * (sample0 + sample1 + sample2 + sample3);
}

#endif

//-------------------------------------------------------------------------------
#ifdef FXAA

Texture2D		Texture : register(t0);
SamplerState	SamplerLinearClamp : register(s0);

struct VSOutput {
    float4 position : SV_POSITION;
  	float4 uv : TEXCOORD0;
};

VSOutput VSMain(uint VertexID : SV_VertexID)
{
	float texWidth, texHeight;
	Texture.GetDimensions(texWidth, texHeight);
	
	VSOutput output;
	output.position = float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);

	output.uv.xy = output.position.xy * float2(0.5f, -0.5f) + 0.5f;
	output.uv.z = 1.0f / texWidth;
	output.uv.w = 1.0f / texHeight;

	return output;
}

#define FXAA_PC 1
#define FXAA_HLSL_4 1
#define FXAA_QUALITY__SUBPIX 1.0f
#define FXAA_CONSOLE__EDGE_SHARPNESS 2.0
//#define FXAA_CONSOLE__EDGE_SHARPNESS 8.0
#define FXAA_QUALITY__EDGE_THRESHOLD 1/16.0f
#include "fxaa39.fxi"

float4 PSMain( VSOutput input) : SV_Target
{
	float2 dudv = input.uv.zw;
	float2 uv   = input.uv.xy + dudv;

	FxaaTex fxaaTex;
	fxaaTex.smpl = SamplerLinearClamp;
	fxaaTex.tex = Texture;
	
	float4 fxaaImage = FxaaPixelShader( uv, float4(uv-dudv/2, uv+dudv/2), fxaaTex, dudv, float4(2 * dudv, 0.5 * dudv) );

	return float4(fxaaImage.rgb, 1);
}

#endif


//-------------------------------------------------------------------------------
#ifdef PREFILTER_ENVMAP

SamplerState	SamplerLinearClamp : register(s0);
TextureCube 	Source : register(t0);
	
cbuffer CBuffer : register(b0) {
	 float4	Roughness;	// = roughness, 1/texelSize, 0, 0
};



struct PS_IN {
    float4 position 	  : SV_POSITION;
  	float3 cubeTexCoord   : TEXCOORD0;
};


float radicalInverse_VdC(uint bits) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

//The ith point xi is then computed by         
float2 Hammersley(uint i, uint N)
{
	return float2(float(i)/float(N), radicalInverse_VdC(i));
}

float3 ImportanceSampleGGX( float2 E, float Roughness, float3 N )
{
	float m = Roughness * Roughness;

	float Phi = 2 * 3.1415 * E.x;
	float CosTheta = sqrt( (1 - E.y) / ( 1 + (m*m - 1) * E.y ) );
	float SinTheta = sqrt( 1 - CosTheta * CosTheta );

	float3 H;
	H.x = SinTheta * cos( Phi );
	H.y = SinTheta * sin( Phi );
	H.z = CosTheta;

	float3 UpVector = abs(N.z) < 0.999 ? float3(0,0,1) : float3(1,0,0);
	float3 TangentX = normalize( cross( UpVector, N ) );
	float3 TangentY = cross( N, TangentX );
	// tangent to world space
	return TangentX * H.x + TangentY * H.y + N * H.z;
}


PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
	output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
	output.position.zw = 1.0f;
	output.cubeTexCoord = 0;

	#ifdef POSX
		output.cubeTexCoord.z = -((VertexID == 0) ? 3.0f : -1.0f);
		output.cubeTexCoord.y =  ((VertexID == 2) ? 3.0f : -1.0f);
		output.cubeTexCoord.x = 1.0f;
	#endif
	#ifdef POSY
		output.cubeTexCoord.x =  ((VertexID == 0) ? 3.0f : -1.0f);
		output.cubeTexCoord.z = -((VertexID == 2) ? 3.0f : -1.0f);
		output.cubeTexCoord.y = 1.0f;
	#endif
	#ifdef POSZ
		output.cubeTexCoord.x =  ((VertexID == 0) ? 3.0f : -1.0f);
		output.cubeTexCoord.y =  ((VertexID == 2) ? 3.0f : -1.0f);
		output.cubeTexCoord.z = 1.0f;
	#endif

	#ifdef NEGX
		output.cubeTexCoord.z =  ((VertexID == 0) ? 3.0f : -1.0f);
		output.cubeTexCoord.y =  ((VertexID == 2) ? 3.0f : -1.0f);
		output.cubeTexCoord.x = -1.0f;
	#endif
	#ifdef NEGY
		output.cubeTexCoord.x =  ((VertexID == 0) ? 3.0f : -1.0f);
		output.cubeTexCoord.z =  ((VertexID == 2) ? 3.0f : -1.0f);
		output.cubeTexCoord.y = -1.0f;
	#endif
	#ifdef NEGZ
		output.cubeTexCoord.x = -((VertexID == 0) ? 3.0f : -1.0f);
		output.cubeTexCoord.y =  ((VertexID == 2) ? 3.0f : -1.0f);
		output.cubeTexCoord.z = -1.0f;
	#endif

	return output;
}


float Beckmann( float3 N, float3 H, float roughness)
{
	float 	m		=	roughness * roughness;
	float	cos_a	=	dot(N,H);
	float	sin_a	=	sqrt(abs(1 - cos_a * cos_a)); // 'abs' to avoid negative values
	return	exp( -(sin_a*sin_a) / (cos_a*cos_a) / (m*m) ) / (3.1415927 * m*m * cos_a * cos_a * cos_a * cos_a );
}

float4 PSMain(PS_IN input) : SV_Target
{
#if 1	
	float weight = 0;
	float3 result = 0;
	
	
	float3 N		= normalize(input.cubeTexCoord);
	float3 upVector = abs(N.z) < 0.71 ? float3(0,0,1) : float3(1,0,0);
	float3 tangentX = normalize( cross( upVector, N ) );
	float3 tangentY = cross( N, tangentX );
	
	//	this "magic" code enlarge sampling kernel for each miplevel.
	float dxy	=	Roughness.y;// * (1+ (Roughness - 0.14286f)*6);
	
	//	11 steps is perfect number of steps to pick every texel 
	//	of cubemap with initial size 256x256 and get all important 
	//	samples of Beckmann distrubution.
	for (float x=-5; x<=5; x+=1 ) {
		for (float y=-5; y<=5; y+=1 ) {
			float3 H = normalize(N + tangentX * x * dxy + tangentY * y * dxy);
			float d = Beckmann(H,N,Roughness.x);
			weight += d;
			result.rgb += Source.SampleLevel(SamplerLinearClamp, H, 0).rgb * d;
		}
	}
	return float4(result/weight, 1);//*/

#else
	float4 result = 0;
	float weight = 0;
	int count = 91;
	int rand = input.position.x * 17846 + input.position.y * 14734;
	
	for (int i=0; i<count; i++) {
		float2 E = Hammersley(i+rand,count);
		float3 H = ImportanceSampleGGX( E, Roughness, input.cubeTexCoord );
		float3 N = input.cubeTexCoord;

		result.rgb += Source.SampleLevel(SamplerLinearClamp, H, 0).rgb;// * saturate(dot(N,H));
	}
	return result / count;
#endif
}

#endif

//-------------------------------------------------------------------------------
#ifdef GAUSS_BLUR_3x3

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float2 uv : TEXCOORD0;
};

PS_IN VSMain(uint VertexID : SV_VertexID)
{
  PS_IN output;
  output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
  output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
  output.position.zw = 1.0f;

  float texWidth, texHeight;
  Source.GetDimensions(texWidth, texHeight);

  float2 uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

  #ifdef PASS2
     output.uv = uv + float2( 0.5f / texWidth, -0.5f / texHeight);
  #endif
  #ifdef PASS1
     output.uv = uv + float2(-0.5f / texWidth,  0.5f / texHeight);
  #endif

  return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
  return Source.SampleLevel(SamplerLinearClamp, input.uv, 0);
}

#endif

//-------------------------------------------------------------------------------
#ifdef GAUSS_BLUR

static const int MaxBlurTaps = 33;

cbuffer GaussWeightsCB : register(b0) {
  float4 Weights[MaxBlurTaps];
};

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);
Texture2D DepthSource 	: register(t1);
Texture2D NormalsSource : register(t2);

struct PS_IN {
    float4 position : SV_POSITION;
  	float2 uv : TEXCOORD0;
    float2 texelSize : TEXCCORD1;
};


PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
	output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
	output.position.zw = 1.0f;

	float texWidth, texHeight;
	Source.GetDimensions(texWidth, texHeight);

	output.uv = output.position.xy * float2(0.5f, -0.5f) + 0.5f;

	#ifdef PASS2
		output.texelSize = float2(0.0f, 1.0f / texHeight);
	#endif
	#ifdef PASS1
		output.texelSize = float2(1.0f / texWidth, 0.0f);
	#endif

	return output;
}


float diffWeight( float3 color1, float3 color2 )
{
	float diff = length( color1 - color2 );
	return exp( - diff * diff * 20 );
}



float4 PSMain(PS_IN input) : SV_Target
{
	#ifdef BILATERAL
	
		float4 color = Source.SampleLevel(SamplerLinearClamp, input.uv, 0) * Weights[0].x;
		float4 addColor = float4(0, 0, 0, 0);
		float normalizationTerm = 0;
		float weight = 0;
		float3	normalW = normalize( (NormalsSource.Sample( SamplerLinearClamp, input.uv ).xyz)*2 - 1 );

		[unroll]
		for (int i = 1; i < 33; i++) {

			float2 locationTexCoord = input.uv + input.texelSize * Weights[i].w;
			float4 otherColor = Source.SampleLevel(SamplerLinearClamp, locationTexCoord, Weights[i].y);
			float3 otherNormalW = normalize( (NormalsSource.Sample( SamplerLinearClamp, locationTexCoord ).xyz)*2 - 1 );

			weight = Weights[i].x * diffWeight( normalW, otherNormalW );

			addColor += otherColor * weight;
			normalizationTerm += weight;
		}
		normalizationTerm += Weights[0].x;
		return (color + addColor)/normalizationTerm;
		
	#else
		float4 color = Source.SampleLevel(SamplerLinearClamp, input.uv, 0) * Weights[0].x;

		[unroll]
		for (int i = 1; i < 33; i++) {
			color += Source.SampleLevel(SamplerLinearClamp, input.uv + input.texelSize * Weights[i].w, Weights[i].y) * Weights[i].x;
		}

		return color;
	#endif

}

#endif

//-------------------------------------------------------------------------------
#ifdef LINEARIZE_DEPTH

cbuffer ConstantBuffer : register(b0)
{
	float linearizeDepthA;
	float linearizeDepthB;
};

Texture2D<float> Depth : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float PSMain(float4 position : SV_POSITION) : SV_Target
{
	float depth = Depth.Load(int3(position.xy, 0)).x;
	return 1.0f / (depth * linearizeDepthA + linearizeDepthB);
}

#endif

//----------------------------------------------------------------------------------
#ifdef RESOLVE_AND_LINEARIZE_DEPTH_MSAA

Texture2DMS<float> Depth : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float PSMain(float4 position : SV_POSITION) : SV_Target
{
	return 0;
	//return ConvertToViewDepth( Depth.Load(int2(position.xy), 0) );
}

#endif