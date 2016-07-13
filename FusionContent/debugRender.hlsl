/*-----------------------------------------------------------------------------
	Debug render shader :
-----------------------------------------------------------------------------*/

#if 0
$ubershader SOLID|GHOST
#endif

struct BATCH {
	float4x4	View;
	float4x4	ViewProjection;
	float4		ViewPosition;
	float4		PixelSize;
};

cbuffer CBBatch : register(b0) { BATCH Batch : packoffset( c0 ); }

struct VS_IN {
	float4 pos : POSITION;
	float4 col : COLOR;
};

struct GS_IN {
	float4 pos : POSITION;
	float4 col : COLOR;
	float  wth : TEXCOORD0;
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};


GS_IN VSMain( VS_IN input )
{
	GS_IN output = (GS_IN)0;
	
	output.pos = float4(input.pos.xyz,1);
	output.col = input.col;
	output.wth = input.pos.w;
	
	return output;
}


[maxvertexcount(6)]
void GSMain( line GS_IN inputPoint[2], inout TriangleStream<PS_IN> outputStream )
{
	PS_IN p0, p1, p2, p3;

	float3 position	=	inputPoint[0].pos.xyz;
	float3 tailpos	=	inputPoint[1].pos.xyz;

	float4 vpos0  = mul( float4(inputPoint[0].pos.xyz,1), Batch.View );
	float4 vpos1  = mul( float4(inputPoint[1].pos.xyz,1), Batch.View );
	
	float z0	=	abs(vpos0.z / vpos0.w);
	float z1	=	abs(vpos0.z / vpos0.w);
	
	float sz0 = max(inputPoint[0].wth/2, 0);
	float sz1 = max(inputPoint[1].wth/2, 0);
	
	sz0 = max( sz0, max( Batch.PixelSize.x * z0, Batch.PixelSize.y * z0 ));
	sz1 = max( sz1, max( Batch.PixelSize.x * z1, Batch.PixelSize.y * z1 ));

	float3 dir	=	normalize( position - tailpos );
	float3 eye	=	normalize( Batch.ViewPosition.xyz - tailpos );
	float3 side	=	normalize( cross( eye, dir ) );
	float4 pos0	=	mul( float4( tailpos  - side * sz1, 1 ), Batch.ViewProjection );
	float4 pos1	=	mul( float4( position - side * sz0, 1 ), Batch.ViewProjection );
	float4 pos2	=	mul( float4( position + side * sz0, 1 ), Batch.ViewProjection );
	float4 pos3	=	mul( float4( tailpos  + side * sz1, 1 ), Batch.ViewProjection );
	
	
	p0.pos = pos0;
	p0.col = inputPoint[1].col;
	
	p1.pos = pos1;
	p1.col = inputPoint[0].col;
	
	p2.pos = pos2;
	p2.col = inputPoint[0].col;
	
	p3.pos = pos3;
	p3.col = inputPoint[1].col;

	
	outputStream.Append(p0);
	outputStream.Append(p1);
	outputStream.Append(p2);
	
	outputStream.RestartStrip();

	outputStream.Append(p0);
	outputStream.Append(p2);
	outputStream.Append(p3);

	outputStream.RestartStrip();
}



float4 PSMain( PS_IN input ) : SV_Target
{
	#ifdef SOLID
	return float4(input.col.rgb,1);
	#endif
	#ifdef GHOST
	return float4(input.col.rgb,0.33f);
	#endif
}




