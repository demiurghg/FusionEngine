//
// Copyright 2014 ADVANCED MICRO DEVICES, INC.  All Rights Reserved.
//
// AMD is granting you permission to use this software and documentation (if
// any) (collectively, the "Materials") pursuant to the terms and conditions
// of the Software License Agreement included with the Materials.  If you do
// not have a copy of the Software License Agreement, contact your AMD
// representative for a copy.
// You agree that you will not reverse engineer or decompile the Materials,
// in whole or in part, except as allowed by applicable law.
//
// WARRANTY DISCLAIMER: THE SOFTWARE IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND.  AMD DISCLAIMS ALL WARRANTIES, EXPRESS, IMPLIED, OR STATUTORY,
// INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, TITLE, NON-INFRINGEMENT, THAT THE SOFTWARE
// WILL RUN UNINTERRUPTED OR ERROR-FREE OR WARRANTIES ARISING FROM CUSTOM OF
// TRADE OR COURSE OF USAGE.  THE ENTIRE RISK ASSOCIATED WITH THE USE OF THE
// SOFTWARE IS ASSUMED BY YOU.
// Some jurisdictions do not allow the exclusion of implied warranties, so
// the above exclusion may not apply to You. 
// 
// LIMITATION OF LIABILITY AND INDEMNIFICATION:  AMD AND ITS LICENSORS WILL
// NOT, UNDER ANY CIRCUMSTANCES BE LIABLE TO YOU FOR ANY PUNITIVE, DIRECT,
// INCIDENTAL, INDIRECT, SPECIAL OR CONSEQUENTIAL DAMAGES ARISING FROM USE OF
// THE SOFTWARE OR THIS AGREEMENT EVEN IF AMD AND ITS LICENSORS HAVE BEEN
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.  
// In no event shall AMD's total liability to You for all damages, losses,
// and causes of action (whether in contract, tort (including negligence) or
// otherwise) exceed the amount of $100 USD.  You agree to defend, indemnify
// and hold harmless AMD and its licensors, and any of their directors,
// officers, employees, affiliates or agents from and against any and all
// loss, damage, liability and other expenses (including reasonable attorneys'
// fees), resulting from Your use of the Software or violation of the terms and
// conditions of this Agreement.  
//
// U.S. GOVERNMENT RESTRICTED RIGHTS: The Materials are provided with "RESTRICTED
// RIGHTS." Use, duplication, or disclosure by the Government is subject to the
// restrictions as set forth in FAR 52.227-14 and DFAR252.227-7013, et seq., or
// its successor.  Use of the Materials by the Government constitutes
// acknowledgement of AMD's proprietary rights in them.
// 
// EXPORT RESTRICTIONS: The Materials may be subject to export restrictions as
// stated in the Software License Agreement.
//

#if( SORT_SIZE>2048 )
	#error
#endif

#define NUM_THREADS		(SORT_SIZE/2)
#define INVERSION		(16*2 + 8*3)

//--------------------------------------------------------------------------------------
// Constant Buffers
//--------------------------------------------------------------------------------------
cbuffer NumElementsCB : register( b0 )
{
	int4 g_NumElements;
};

//--------------------------------------------------------------------------------------
// Structured Buffers
//--------------------------------------------------------------------------------------
RWStructuredBuffer<float2> Data : register( u0 );


//--------------------------------------------------------------------------------------
// Bitonic Sort Compute Shader
//--------------------------------------------------------------------------------------
groupshared float2	g_LDS[SORT_SIZE];


[numthreads(NUM_THREADS, 1, 1)]
void BitonicInnerSort(	uint3 Gid	: SV_GroupID, 
						uint3 DTid	: SV_DispatchThreadID, 
						uint3 GTid	: SV_GroupThreadID, 
						uint	GI	: SV_GroupIndex )
{
	int4 tgp;

	tgp.x = Gid.x * 256;
	tgp.y = 0;
	tgp.z = g_NumElements.x;
	tgp.w = min( 512, max( 0, g_NumElements.x - Gid.x * 512 ) );

	int GlobalBaseIndex = tgp.y + tgp.x*2 + GTid.x;
	int LocalBaseIndex  = GI;
	int i;

    // Load shared data
	[unroll]for( i = 0; i<2; ++i )
	{
		if( GI+i*NUM_THREADS<tgp.w )
			g_LDS[ LocalBaseIndex + i*NUM_THREADS ] = Data[ GlobalBaseIndex + i*NUM_THREADS ];
	}
    GroupMemoryBarrierWithGroupSync();

	// sort threadgroup shared memory
	for( int nMergeSubSize=SORT_SIZE>>1; nMergeSubSize>0; nMergeSubSize=nMergeSubSize>>1 ) 
	{			
		int tmp_index = GI;
		int index_low = tmp_index & (nMergeSubSize-1);
		int index_high = 2*(tmp_index-index_low);
		int index = index_high + index_low;

		unsigned int nSwapElem = index_high + nMergeSubSize + index_low;

		if( nSwapElem<tgp.w )
		{
			float2 a = g_LDS[index];
			float2 b = g_LDS[nSwapElem];

			if (a.x > b.x)
			{ 
				g_LDS[index] = b;
				g_LDS[nSwapElem] = a;
			}
		}
		GroupMemoryBarrierWithGroupSync();
	}
    
    // Store shared data
	[unroll]for( i = 0; i<2; ++i )
	{
		if( GI+i*NUM_THREADS<tgp.w )
			Data[ GlobalBaseIndex + i*NUM_THREADS ] = g_LDS[ LocalBaseIndex + i*NUM_THREADS ];
	}
}
