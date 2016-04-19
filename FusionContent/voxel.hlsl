
#if 0
$ubershader DEBUG_DRAW_VOXEL
$ubershader COPY_BUFFER_TO_VOXEL
#endif


#ifdef DEBUG_DRAW_VOXEL
 
Texture3D VolumeTexture : register(t0);
SamplerState NearestSampler : register(s0);

cbuffer WorldViewProjectionCB : register(b0) {
	float4x4 WorldViewProjection : packoffset(c0);
};

struct VertexOutput {
	float4 Position : SV_Position;
	float4 Color    : COLOR0;
};


VertexOutput VSMain ( uint VertexID : SV_VertexID )
{
	VertexOutput output;
	float3 worldPos;
	int w,h,d;
	
	VolumeTexture.GetDimensions( w,h,d );
	
	worldPos.x = (VertexID % w);
	worldPos.y = (VertexID / w) % h;
	worldPos.z = (VertexID / w) / h;
	
	float3 uvw = worldPos / float3(w,h,d);
	
	output.Position = float4( worldPos.xyz*0.25, 1 );
	output.Color	= VolumeTexture.SampleLevel( NearestSampler, uvw, 0 );
	
	return output;
}


[maxvertexcount(24)]
void GSMain( point VertexOutput inputPoint[1], inout TriangleStream<VertexOutput> outputStream )
{
	float sz = 0.125;
	
	if (inputPoint[0].Color.a<0.75f) {
		return;
	}
	
	VertexOutput	v0;
	v0.Position	=	mul( inputPoint[0].Position + float4(-sz,-sz,-sz,0), WorldViewProjection );
	v0.Color	=	inputPoint[0].Color;
	
	VertexOutput	v1;
	v1.Position	=	mul( inputPoint[0].Position + float4(-sz,-sz,+sz,0), WorldViewProjection );
	v1.Color	=	inputPoint[0].Color;
	
	VertexOutput	v2;
	v2.Position	=	mul( inputPoint[0].Position + float4(+sz,-sz,+sz,0), WorldViewProjection );
	v2.Color	=	inputPoint[0].Color;
	
	VertexOutput	v3;
	v3.Position	=	mul( inputPoint[0].Position + float4(+sz,-sz,-sz,0), WorldViewProjection );
	v3.Color	=	inputPoint[0].Color;
	
	
	VertexOutput	v4;
	v4.Position	=	mul( inputPoint[0].Position + float4(-sz,+sz,-sz,0), WorldViewProjection );
	v4.Color	=	inputPoint[0].Color;
	
	VertexOutput	v5;
	v5.Position	=	mul( inputPoint[0].Position + float4(-sz,+sz,+sz,0), WorldViewProjection );
	v5.Color	=	inputPoint[0].Color;
	
	VertexOutput	v6;
	v6.Position	=	mul( inputPoint[0].Position + float4(+sz,+sz,+sz,0), WorldViewProjection );
	v6.Color	=	inputPoint[0].Color;
	
	VertexOutput	v7;
	v7.Position	=	mul( inputPoint[0].Position + float4(+sz,+sz,-sz,0), WorldViewProjection );
	v7.Color	=	inputPoint[0].Color;

	//	bottom :
	outputStream.Append( v1 );
	outputStream.Append( v0 );
	outputStream.Append( v2 );
	outputStream.Append( v3 );
	outputStream.RestartStrip();

	//	top :
	outputStream.Append( v6 );
	outputStream.Append( v7 );
	outputStream.Append( v5 );
	outputStream.Append( v4 );
	outputStream.RestartStrip();

	//	back :
	outputStream.Append( v3 );
	outputStream.Append( v0 );
	outputStream.Append( v7 );
	outputStream.Append( v4 );
	outputStream.RestartStrip();

	//	front :
	outputStream.Append( v6 );
	outputStream.Append( v5 );
	outputStream.Append( v2 );
	outputStream.Append( v1 );
	outputStream.RestartStrip();

	//	right :
	outputStream.Append( v2 );
	outputStream.Append( v3 );
	outputStream.Append( v6 );
	outputStream.Append( v7 );
	outputStream.RestartStrip();

	//	left :
	outputStream.Append( v0 );
	outputStream.Append( v1 );
	outputStream.Append( v4 );
	outputStream.Append( v5 );
	outputStream.RestartStrip();
}


float4 PSMain ( VertexOutput input ) : SV_TARGET0
{
	return input.Color * 30;
}

#endif




#ifdef COPY_BUFFER_TO_VOXEL

StructuredBuffer<float4> source;
RWTexture3D<float4> destination;

[numthreads( 8, 8, 8 )]
void CSMain (
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int w,h,d;
	destination.GetDimensions(w,h,d);
	
	int addr	=	dispatchThreadID.x + dispatchThreadID.y * w + dispatchThreadID.z * w * h;
	
	destination[dispatchThreadID] = source[ addr ];
}

#endif


