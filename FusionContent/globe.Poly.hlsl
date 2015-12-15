
double dDiv(double a, double b) // 
{
	double r = double(1.0f/float(b));

	r = r * (2.0 - b*r);
	r = r * (2.0 - b*r);

	return a*r;
}


double sine_limited(double x) {
  double r = x, mxx = -x*x;
  // Change dDiv to multiply to constants 
  r += (x *= dDiv(mxx, 6.0	)); // i=3
  r += (x *= dDiv(mxx, 20.0	)); // i=5
  r += (x *= dDiv(mxx, 42.0	)); // i=7
  r += (x *= dDiv(mxx, 72.0	)); // i=9
  r += (x *= dDiv(mxx, 110.0)); // i=11

  return r;
}

// works properly only for x >= 0
double sine_positive(double x) {
	double PI		=	3.141592653589793;
	double PI2		=	2.0*PI;
	double PI_HALF	=	0.5*PI;
	
	
	if (x <= PI_HALF) {
	  return sine_limited(x);
	} else if (x <= PI) {
	  return sine_limited(PI - x);
	} else if (x <= PI2) {
	  return -sine_limited(x - PI);
	} else {
	  return sine_limited(x - PI2*floor(float(dDiv(x,PI2))));
	}
}

double sine(double x) {
	return x < 0.0 ? -sine_positive(-x) : sine_positive(x);
}

double cosine(double x) {
	double PI=3.141592653589793;
	double PI_HALF=0.5*PI;
	return sine(PI_HALF - x);
}


double3 SphericalToDecart(double2 pos, double r)
{
	double3 res = double3(0,0,0);

	double sinX = sine(pos.x);
	double cosX = cosine(pos.x);
	double sinY = sine(pos.y);
	double cosY = cosine(pos.y);

	res.z = r*cosY*cosX;
	res.x = r*cosY*sinX;
	res.y = r*sinY;

	//res.z = r*cosine(pos.y)*cosine(pos.x);
	//res.x = r*cosine(pos.y)*sine(pos.x);
	//res.y = r*sine(pos.y);

	return res;
}




struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct HeatConstData {
	float4 Data;
};


struct VS_INPUT {	
	uint2 lon				: TEXCOORD0	;
	uint2 lat				: TEXCOORD1	;
	float4	Tex0			: TEXCOORD2	;	// Texture Coordinates
	float4	Tex1			: TEXCOORD3	;
	float4	Color			: COLOR		;
};

struct VS_OUTPUT {
    float4 Position	: SV_POSITION	;
	float4 Color	: COLOR			;
	float4 Tex		: TEXCOORD0		;
	float3 Normal	: TEXCOORD1		;
	float3 WPos		: TEXCOORD2		;
};


cbuffer CBStage		: register(b0) 	{ ConstData Stage : packoffset( c0 ); }
cbuffer PolyStage	: register(b1) 	{ HeatConstData HeatStage; }

Texture2D		DiffuseMap		: register(t0);
Texture2D		FloatMap		: register(t1);
Texture2D		FrameMap		: register(t2);
SamplerState	Sampler			: register(s0);
SamplerState	PointSampler	: register(s1);


#if 0
$ubershader PIXEL_SHADER VERTEX_SHADER DRAW_HEAT
$ubershader PIXEL_SHADER VERTEX_SHADER DRAW_COLORED
$ubershader PIXEL_SHADER VERTEX_SHADER XRAY
$ubershader PIXEL_SHADER VERTEX_SHADER DRAW_TEXTURED NO_DEPTH CULL_NONE USE_PALETTE_COLOR
$ubershader COMPUTE_SHADER BLUR_HORIZONTAL
$ubershader COMPUTE_SHADER BLUR_VERTICAL
#endif



#ifdef VERTEX_SHADER
VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	float angle = 0;

	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + v.Tex1.w);

	angle = float(lon);
	
	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));

	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;

	float4 wPos = float4(posX, posY, posZ, 1.0f);
	
	output.Position	=	mul(wPos, Stage.ViewProj);
	output.Normal	=	normal;
	output.Color	=	v.Color;
	output.Tex		=	v.Tex0;
	
	#ifdef XRAY
		output.WPos = wPos.xyz;
	#endif

	return output;
}
#endif


#ifdef PIXEL_SHADER
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	#ifdef XRAY
		float3 norm = normalize(input.Tex.xyz);
		float3 ndir	= normalize(-input.WPos);
		
		float  ndot = abs(dot( ndir, norm ));
		float  frsn	= pow(saturate(1.1f-ndot), 0.5);
		
		return frsn*float4(input.Color.xyz, input.Color.a);
	#endif
	#ifdef DRAW_HEAT
		//return float4(1.0f, 0.0f, 0.0f, 1.0f);

		float val = 0.0f;

		float valX = FloatMap.Sample(PointSampler, float2(input.Tex.x, 1.0f - input.Tex.y)).x;
		float valY = FrameMap.Sample(PointSampler, float2(input.Tex.x, 1.0f - input.Tex.y)).x;
		val	= lerp(valY, valX, HeatStage.Data.w);
		
		//float step = 0.5f/512.0f;
		//float val0 = FloatMap.Sample(PointSampler, float2(input.Tex.x - step, 1.0f - input.Tex.y - step)).x; // left top;
		//float val1 = FloatMap.Sample(PointSampler, float2(input.Tex.x + step, 1.0f - input.Tex.y - step)).x; // left top;
		//float val2 = FloatMap.Sample(PointSampler, float2(input.Tex.x - step, 1.0f - input.Tex.y + step)).x; // left top;
		//float val3 = FloatMap.Sample(PointSampler, float2(input.Tex.x + step, 1.0f - input.Tex.y + step)).x; // left top;
		//val += val0; // left top
		//val += val1; // right top
		//val += val2; // left bottom
		//val += val3; // right bottom
        //
		//val = val / 5.0f;

		val = (val - HeatStage.Data.y) / (HeatStage.Data.x - HeatStage.Data.y);
		val = clamp(val, 0.0f, 1.0f);
		
		float4	color	= DiffuseMap.Sample(Sampler, float2(val, 0.0f));
		
		//return float4(val, val, val, 1.0f);
		return float4(color.rgb, color.a * HeatStage.Data.z);
	#endif
	#ifdef DRAW_TEXTURED
		
		#ifdef USE_PALETTE_COLOR
			float3 color = DiffuseMap.Sample(Sampler, float2(HeatStage.Data.x, 0.5f)).xyz;
			return float4(color, HeatStage.Data.y);
		#else
			float4 color	= DiffuseMap.Sample(Sampler, input.Tex.xy);
			float3 ret		= color.rgb;
			
			#ifdef SHOW_FRAMES
				float4	frame	= FrameMap.Sample(Sampler, input.Tex.xy);
						ret		= color.rgb * (1.0 - frame.a) + frame.rgb*frame.a;
			#endif
			
			return float4(ret, color.a);
		#endif
	#endif
	#ifdef DRAW_COLORED
		return input.Color;
	#endif
}
#endif


#ifdef COMPUTE_SHADER

#define WIDTH 1024
#define GS2 7

groupshared float Line[WIDTH];	// TLS
Texture2D txInput 					: register(t0);	// Input texture to read from
RWTexture2D<float> OutputTexture	: register(u0);	// Tmp output


[numthreads(WIDTH,1,1)]
void CSMain(uint3 groupID: SV_GroupID, uint3 groupThreadID: SV_GroupThreadID)
{

	float G[] = { 0.00332,	0.009267,	0.022087,	0.044948,	0.078109,	0.115911,	0.146884,	0.158949,	0.146884,	0.115911,	0.078109,	0.044948,	0.022087,	0.009267,	0.00332 };
	
	#ifdef BLUR_HORIZONTAL
	// Fetch color from input texture
	float vColor=txInput[int2(groupThreadID.x, groupID.y)].x;
	#endif
	
	#ifdef BLUR_VERTICAL
	float vColor=txInput[int2(groupID.y, groupThreadID.x)].x;
	#endif
	// Store it into TLS
	Line[groupThreadID.x] = vColor;
	// Synchronize threads
	GroupMemoryBarrierWithGroupSync();
	
	// Compute horizontal Gaussian blur for each pixel
	vColor = 0;
	
	[unroll]
	for (int i=-GS2; i<=GS2; i++) {
		// Determine offset of pixel to fetch
		int nOffset = groupThreadID.x + i;
		// Clamp offset
		nOffset = clamp(nOffset, 0, WIDTH-1);
		// Add color for pixels within horizontal filter
		vColor += G[GS2+i] * Line[nOffset];
	}
	
	// Store result
	#ifdef BLUR_HORIZONTAL
	OutputTexture[int2(groupThreadID.x,groupID.y)]=vColor;
	#endif
	#ifdef BLUR_VERTICAL
	OutputTexture[int2(groupID.y, groupThreadID.x)]=vColor;
	#endif
}


#endif