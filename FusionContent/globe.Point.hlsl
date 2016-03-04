//////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////
/////////// Functions
double dDiv(double a, double b)
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
////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

struct VS_INPUT {	
	uint2 lon				: TEXCOORD0	;
	uint2 lat				: TEXCOORD1	;
	float4	Tex0			: TEXCOORD2	;	// Texture Coordinates
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


struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float2 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
};

/////////////////////////////// Constant Buffers
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	; // Factor, Radius
};

struct DataForDots {
	float4x4	View;
	float4x4	Proj;
	float4		AtlasSizeImgSize;
	float4		SizeMult;
};

struct DotsColorsStruct {
	float4 Colors[16];
};

Texture2D		DiffuseMap		: register(t0);
SamplerState	Sampler			: register(s0);


cbuffer CBStage			: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}
cbuffer CBDotsStage 	: register(b1) 	{	DataForDots	DotsData	;	}
cbuffer CBColorsStage 	: register(b2) 	{	DotsColorsStruct DotsColors	;	}



#if 0
$ubershader DOTS_WORLDSPACE +ROTATION_ANGLE
$ubershader DOTS_SCREENSPACE +ROTATION_ANGLE
#endif


VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	float angle = 0;

	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + double(v.Tex1.x));

	angle = float(lon);

	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;

	float3 xAxis = float3(1, 0, 0);
	
#ifdef ROTATION_ANGLE
	float4x4 rotMatZ = float4x4( cos(-v.Tex0.w),sin(-v.Tex0.w),0,0, 	-sin(-v.Tex0.w),cos(-v.Tex0.w),0,0, 0,0,1,0, 0,0,0,1 );
	xAxis = normalize(mul(float4(1, 0, 0, 0), rotMatZ).xyz);
#endif
	
	float4x4 rotMatY = float4x4( cos(angle), 0, -sin(angle), 0, 0,1,0,0, sin(angle),0,cos(angle),0, 0,0,0,1 );

	output.Position	= float4(posX, posY, posZ, 1);
	output.Normal	= normal.xyz;
	output.XAxis 	= normalize(mul(float4(xAxis, 0), rotMatY).xyz);
	output.Color	= v.Color;
	output.Tex		= v.Tex0;
	
	return output;
}


[maxvertexcount(4)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	input	=	inputArray[0];

	float halfWidth = (input.Tex.z * DotsData.SizeMult.x) / 2.0f;
	
	//float x = input.Position.x;
	//float y = input.Position.y;
	//float z = input.Position.z;

	float3 pos = input.Position.xyz;
	float3 xAxis = input.XAxis;
	float3 zAxis = normalize(cross(xAxis,input.Normal)); 
	xAxis = normalize(cross(zAxis,input.Normal));
	
	float texRight	= ((input.Tex.x+1) 	* DotsData.AtlasSizeImgSize.z)/DotsData.AtlasSizeImgSize.x;
	float texLeft	= (input.Tex.x 		* DotsData.AtlasSizeImgSize.z)/DotsData.AtlasSizeImgSize.x;

	float4	color	= input.Color;
	//int		colorInd	= int(input.Tex.y);
	//if(colorInd != 0) color = DotsColors.Colors[colorInd];

	//float ang = dot(input.Normal, Stage.ViewDir.xyz);
	//color.a = ang;
	
	////// Kostyl
	//if(input.Tex.x == 13) {
	//	halfWidth = halfWidth*2.5f;
	//	color = float4(0.9f,0.9f,0.1f,1.0f);
	//}
	//if(input.Tex.x == 14) {
	//	halfWidth = halfWidth*1.7f;
	//	color = float4(0.3f,0.3f,0.3f,1.0f);
	//}
	/////////////
	
	float4x4 mat;
#ifdef DOTS_SCREENSPACE
	// Plane
	float3 viewPos = mul(float4(pos, 1), DotsData.View).xyz;
	
	output.Color	=	color;
	output.Normal	=	input.Normal;
	
	output.Position	= mul(float4(viewPos.x + halfWidth, viewPos.y + halfWidth, viewPos.z, 1), DotsData.Proj);
	output.Tex		= float2(texRight, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(viewPos.x - halfWidth, viewPos.y + halfWidth, viewPos.z, 1), DotsData.Proj);
	output.Tex		= float2(texLeft, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(viewPos.x + halfWidth, viewPos.y - halfWidth, viewPos.z, 1), DotsData.Proj);
	output.Tex		= float2(texRight, 1.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(viewPos.x - halfWidth, viewPos.y - halfWidth, viewPos.z, 1), DotsData.Proj);
	output.Tex		= float2(texLeft, 1.0f);
	stream.Append( output );
#endif
#ifdef DOTS_WORLDSPACE
	// Plane
	output.Color	=	color;
	output.Normal	=	input.Normal;

	output.Position	= mul(float4(pos + halfWidth*xAxis - halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texRight, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(pos - halfWidth*xAxis - halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texLeft, 0.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(pos + halfWidth*xAxis + halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texRight, 1.0f);
	stream.Append( output );
	
	output.Position	= mul(float4(pos - halfWidth*xAxis + halfWidth*zAxis, 1), Stage.ViewProj);
	output.Tex		= float2(texLeft, 1.0f);
	stream.Append( output );
#endif	
}


float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	float4 color = DiffuseMap.Sample(Sampler, input.Tex);

	color.rgb *= input.Color.rgb;

	color.a *= input.Color.a;

	//color = float4(1.0f, 0.0f, 0.0f, 1.0f);
	return color;
}
