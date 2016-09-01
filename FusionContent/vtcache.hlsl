
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

cbuffer CBParams : register(b0) { 
	int totalPageCount : packoffset( c0.x ); ;
	int targetMipLevel : packoffset( c0.y ); ;
};

//	pages must be ordered descending by mip-level!
StructuredBuffer<PAGE> pageData : register(t0);
RWTexture2D<float4> pageTable  : register(u0); 

groupshared uint visiblePageCount = 0; 
groupshared uint visiblePages[343];

#define BLOCK_SIZE_X 16 
#define BLOCK_SIZE_Y 16 
[numthreads(BLOCK_SIZE_X,BLOCK_SIZE_Y,1)] 
void CSMain( 
	uint3 groupId : SV_GroupID, 
	uint3 groupThreadId : SV_GroupThreadID, 
	uint  groupIndex: SV_GroupIndex, 
	uint3 dispatchThreadId : SV_DispatchThreadID) 
{
	int2 location		=	int2( dispatchThreadId.x, dispatchThreadId.y );
	uint threadCount 	= 	BLOCK_SIZE_X * BLOCK_SIZE_Y; 
	uint passCount 		=	(totalPageCount+threadCount-1) / threadCount;

	GroupMemoryBarrierWithGroupSync();
#if 1 
	//--------------------------
	// Tiled approach:
	//--------------------------
	for (uint passIt=0; passIt < passCount; passIt++) {
		uint 	pageIndex	=	passIt * threadCount + groupIndex;
		
		float2 tileMin = float2( groupId.x*BLOCK_SIZE_X,    		  groupId.y*BLOCK_SIZE_Y			  );
		float2 tileMax = float2( groupId.x*BLOCK_SIZE_X+BLOCK_SIZE_X, groupId.y*BLOCK_SIZE_Y+BLOCK_SIZE_Y );
		
		if ( pageIndex < totalPageCount ) 
		{
			PAGE 	page		=	pageData[ pageIndex ];
			
			if ( page.mip >= targetMipLevel ) 
			{
				float	size	=	exp2(page.mip - targetMipLevel);
				float2 	pageMin	=	float2( page.vx * size, 		page.vy * size 		  );
				float2 	pageMax	=	float2( page.vx * size + size, 	page.vy * size + size );
			
				if ( pageMin.x < tileMax.x && tileMin.x < pageMax.x 
				  && pageMin.y < tileMax.y && tileMin.y < pageMax.y ) 
				{
					uint offset; 
					InterlockedAdd(visiblePageCount, 1, offset); 
					visiblePages[offset] = pageIndex;
				}
			}
		}
	}
	
	GroupMemoryBarrierWithGroupSync();
	
	float4	physicalAddress	=	float4(0,0,999,0);
	
	for (uint i = 0; i < visiblePageCount; i++) {
	
		uint 	pageIndex 	= 	visiblePages[ i ];
		PAGE 	page 		= 	pageData[ pageIndex ];
		float	size		=	exp2(page.mip - targetMipLevel);

		if ( page.vx*size <= location.x && page.vx*size + size > location.x 
		  && page.vy*size <= location.y && page.vy*size + size > location.y ) 
		{
			if (physicalAddress.z>page.mip) {
				physicalAddress 	=	float4( page.offsetX, page.offsetY, page.mip, 1 );
			}
		}
	}
	
	GroupMemoryBarrierWithGroupSync();

#else 
	//--------------------------
	// Brute-force approach:
	//--------------------------
	float4 physicalAddress	=	float4(0,0,0,0);
	
	for (uint i = 0; i < totalPageCount; i++) {
	
		PAGE 	page 		= 	pageData[ i ];
		int		mip			=	(int)page.mip;
		float	size		=	exp2(mip);

		if ( page.vx*size <= location.x && (page.vx*size + size) > location.x 
		  && page.vy*size <= location.y && (page.vy*size + size) > location.y ) 
		{
			physicalAddress 	=	float4( page.offsetX, page.offsetY, page.mip, 1 );
		}
	}
#endif
	
	pageTable[dispatchThreadId.xy] = physicalAddress;
}
