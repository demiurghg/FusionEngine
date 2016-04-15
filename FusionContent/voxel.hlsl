
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
	float4 Position : SV_POSITION;
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
	
	output.Position = float4( worldPos.xyz, 1 );
	output.Color	= VolumeTexture.SampleLevel( NearestSampler, uvw, 0 );
	
	return output;
}


[maxvertexcount(24)]
void GSMain( point VertexOutput inputPoint[1], inout TriangleStream<VertexOutput> outputStream )
{
	outputStream.Append( inputPoint[0] );
	outputStream.Append( inputPoint[0] );
	outputStream.Append( inputPoint[0] );
	outputStream.RestartStrip();
}


float4 PSMain ( VertexOutput input ) : SV_TARGET0
{
	return input.Color;
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

