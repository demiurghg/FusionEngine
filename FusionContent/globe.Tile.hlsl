#include "globe.Include.hlsl"



Texture2D		DiffuseMap		: register(t0);
Texture2D		FrameMap		: register(t1);
SamplerState	Sampler			: register(s0);

struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	; // Factor, Radius
};


struct VS_INPUT {	
	uint2 lon				: TEXCOORD0	;
	uint2 lat				: TEXCOORD1	;
	float4	Tex				: TEXCOORD2	;	// Texture Coordinates
	float4	Tex1			: TEXCOORD3	;
	float4	Color			: COLOR		;

};


struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
	float3 XAxis		: TEXCOORD2		;
};



cbuffer CBStage		: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}


double dDiv(double a, double b) // 
{
	double r = double(1.0f/float(b));

	r = r * (2.0 - b*r);
	r = r * (2.0 - b*r);

	return a*r;
}


double sine_limited(double x) 
{
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
double sine_positive(double x) 
{
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

double sine(double x) 
{
	return x < 0.0 ? -sine_positive(-x) : sine_positive(x);
}

double cosine(double x) 
{
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


#if 0
$ubershader +SHOW_FRAMES
#endif

VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137);

	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;

	output.Position	=	mul(float4(posX, posY, posZ, 1), Stage.ViewProj);
	output.Normal	=	normal;

	output.Color	=	v.Color;
	output.Tex		=	v.Tex;
	
	return output;
}



////////////////////////// Draw map tiles and polygons
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
		float4 color	= DiffuseMap.Sample(Sampler, input.Tex.xy);
		float3 ret		= color.rgb;
		
		#ifdef SHOW_FRAMES
			float4	frame	= FrameMap.Sample(Sampler, input.Tex.xy);
					ret		= color.rgb * (1.0 - frame.a) + frame.rgb*frame.a;
		#endif
		
		return float4(ret, color.a);
}
