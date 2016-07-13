struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct DataForDots{
	float		LimitAlpha;
	float		FadingVelocity;
	float		DeltaTime;
	float		Parameter;
};

struct VS_INPUT {	
	uint2 	X		: TEXCOORD0	;
	uint2 	Y		: TEXCOORD1	;
	uint2 	Z		: TEXCOORD2	;	// Texture Coordinates
	float4	Tex		: TEXCOORD3	;
	float4	Color	: COLOR		;
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
};

cbuffer CBStage			: register(b0) 	{ ConstData Stage : packoffset( c0 ); }
cbuffer CBDotsStage 	: register(b1) 	{ DataForDots	FadingData	;	}

Texture2D		DiffuseMap		: register(t0);
SamplerState	Sampler			: register(s0);


#if 0
$ubershader DRAW_LINES
$ubershader DRAW_TEXTURED_POLY +POINT_FADING
#endif



VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX.x, Stage.CameraX.y), asdouble(Stage.CameraY.x, Stage.CameraY.y), asdouble(Stage.CameraZ.x, Stage.CameraZ.y));

	double cPosX = asdouble(v.X.x, v.X.y);
	double cPosY = asdouble(v.Y.x, v.Y.y);
	double cPosZ = asdouble(v.Z.x, v.Z.y);
	
	double posX = cPosX - cameraPos.x;
	double posY = cPosY - cameraPos.y;
	double posZ = cPosZ - cameraPos.z;

	output.Position	=	mul(float4(posX, posY, posZ, 1), Stage.ViewProj);

	output.Color	=	v.Color;
	output.Tex		=	v.Tex;
	
#ifdef POINT_FADING
	if ( v.Color.a > FadingData.LimitAlpha){
		output.Color.a = v.Color.a - (FadingData.FadingVelocity * FadingData.DeltaTime);
	}
#endif
	return output;
}

#ifdef DRAW_LINES
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float4 color = input.Color;
	return color;
}
#endif

#ifdef DRAW_TEXTURED_POLY
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float4	color = DiffuseMap.Sample(Sampler, input.Tex.xy);
	//color = float4(1.0f, 0.0f, 0.0f, 1.0f);
	
	color.rgb *= input.Color.rgb;

	color.a *= input.Color.a;

	return color;
}
#endif
