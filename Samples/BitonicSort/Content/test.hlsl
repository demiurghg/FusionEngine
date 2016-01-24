
#if 0
$ubershader
#endif

#define BLOCK_SIZE		64
#define BLOCK_LOGSIZE	6

struct PARAMS {
	int Size;
	int Log2Size;
};

cbuffer CBParams : register(b0) { PARAMS Params : packoffset( c0 ); }	

RWStructuredBuffer<float>	buffer	: register(u0);

groupshared float	sharedData[BLOCK_SIZE*2];


void Kernel ( int index, int p, int q )
{
	int d = 1 << (p-q);
	
	int i = index;

	bool up = ((i >> p) & 2) == 0;

	if ((i & d) == 0 && (sharedData[i] > sharedData[i|d]) == up) {
		float t = sharedData[i]; 
		sharedData[i] = sharedData[i|d]; 
		sharedData[i|d] = t;
	}
}




void Kernel2 ( int even, int odd, int p, int q )
{
	float a = sharedData[even];
	float b = sharedData[odd];
	
	if (a > b) {
		sharedData[odd]  = a;
		sharedData[even] = b;
	}
}


[numthreads( BLOCK_SIZE/2, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{	
	int sharedIndexEven	=	groupThreadID.x * 2 + 0;
	int sharedIndexOdd	=	groupThreadID.x * 2 + 1;
	int sharedIndex		=	groupThreadID.x;
	int threadIndexEven	=	BLOCK_SIZE * groupID.x + sharedIndexEven;
	int threadIndexOdd	=	BLOCK_SIZE * groupID.x + sharedIndexOdd;
	
	//------------------------------------------
	//	store input data in groupshared memory :
	sharedData[ sharedIndexEven ] = buffer[ threadIndexEven	];
	sharedData[ sharedIndexOdd  ] = buffer[ threadIndexOdd	];
	GroupMemoryBarrierWithGroupSync();
	

	//	bitonic sort of each 'BLOCK_SIZE'-blocks.
	for (int p=0; p<BLOCK_LOGSIZE; p++) {
		for (int q=0; q<=p; q++) {
			
			Kernel( sharedIndexEven, p, q );
			Kernel( sharedIndexOdd, p, q );
			
			GroupMemoryBarrierWithGroupSync();
		}
	}

	
	//-------------------
	//	write data back :
	GroupMemoryBarrierWithGroupSync();
	buffer[ threadIndexEven	] = sharedData[ sharedIndexEven ];
	buffer[ threadIndexOdd	] = sharedData[ sharedIndexOdd  ];

	
	//-------------------
	//	write data back :
}



