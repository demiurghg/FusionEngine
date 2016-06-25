
struct VS_INPUT {	
	uint2 	X		: TEXCOORD0	;
	uint2 	Y		: TEXCOORD1	;
	uint2 	Z		: TEXCOORD2	;
	float4	Tex		: TEXCOORD3	;
	float4	Color	: COLOR		;
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
};


struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
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
	float4		SizeMult;
	float4 		Dummy;
};


cbuffer CBStage			: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}
cbuffer CBDotsStage 	: register(b1) 	{	DataForDots	DotsData	;	}



#if 0
$ubershader DOTS_SCREENSPACE +TEST
#endif


VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos = double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	double cPosX = asdouble(v.X.x, v.X.y);
	double cPosY = asdouble(v.Y.x, v.Y.y);
	double cPosZ = asdouble(v.Z.x, v.Z.y);
	
	double posX = cPosX - cameraPos.x;
	double posY = cPosY - cameraPos.y;
	double posZ = cPosZ - cameraPos.z;


	output.Position	= float4(posX, posY, posZ, 1);
	output.Color	= v.Color;
	output.Tex		= v.Tex;
	
	return output;
}


[maxvertexcount(4)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	input	=	inputArray[0];

	float radius = input.Tex.x * DotsData.SizeMult.x;

	float3 	pos 	= input.Position.xyz;
	float4	color	= input.Color;
	
#ifdef TEST
	color.a = saturate(input.Tex.w - DotsData.SizeMult.w);
#endif
	
#ifdef DOTS_SCREENSPACE
	// Plane
	float3 viewPos = mul(float4(pos, 1), DotsData.View).xyz;
	
	output.Color	=	color;
	
	output.Position	= mul(float4(viewPos.x + radius, viewPos.y + radius, viewPos.z, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	= mul(float4(viewPos.x - radius, viewPos.y + radius, viewPos.z, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	= mul(float4(viewPos.x + radius, viewPos.y - radius, viewPos.z, 1), DotsData.Proj);
	stream.Append( output );
	
	output.Position	= mul(float4(viewPos.x - radius, viewPos.y - radius, viewPos.z, 1), DotsData.Proj);
	stream.Append( output );
#endif

}


float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	//return float4(1.0f, 0.0f, 0.0f, 1.0f);
	return input.Color;
}
