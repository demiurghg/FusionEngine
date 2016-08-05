
#if 0
$ubershader
#endif

struct PAGE {
	float vx;
	float vy;
	float offsetX;
	float offsetY;
	float mip;
};

struct PARAMS {
	int pageCount;
};

cbuffer CBParams : register(b0) { 
	PARAMS Params : packoffset( c0 ); 
};

StructuredBuffer<PAGE> pageData : register(t0);
RWTexture2D<float4> pageTable  : register(u0); 

#define BLOCK_SIZE_X 16 
#define BLOCK_SIZE_Y 16 
[numthreads(BLOCK_SIZE_X,BLOCK_SIZE_Y,1)] 
void CSMain( 
	uint3 groupId : SV_GroupID, 
	uint3 groupThreadId : SV_GroupThreadID, 
	uint  groupIndex: SV_GroupIndex, 
	uint3 dispatchThreadId : SV_DispatchThreadID) 
{
	pageTable[dispatchThreadId.xy] = float4(1,1,0,1);
}
