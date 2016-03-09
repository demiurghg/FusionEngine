//-------------------------------------------------------------------------------
//	HBAO shader:
//	http://www.derschmale.com/source/hbao/HBAOFragmentShader.hlsl
//	http://www.derschmale.com/source/hbao/HBAOVertexShader.hlsl
//-------------------------------------------------------------------------------

#if 0
$ubershader HBAO
#endif

#ifdef HBAO

Texture2D	Source : register(t0);

float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}

float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	float depth	=	Source.Load(int3(position.xy, 0)).r;
	return 1;//float4( ddx(depth)*100, ddy(depth)*100, 1, 1);
}

#endif

