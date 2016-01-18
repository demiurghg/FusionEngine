
#if 0
$ubershader INITIALIZE|INJECTION|SIMULATION|DRAW
#endif

// change ParticleSystem.cs too!
#define BLOCK_SIZE		256		
#define MAX_INJECTED 	1024
#define MAX_PARTICLES 	(384*1024)

struct PARTICLE {
	float3	Position;
	float3	Velocity;
	float3	Acceleration;
	float4	Color0;
	float4	Color1;
	float	Gravity;
	float	Damping;
	float	Size0;
	float	Size1;
	float	Angle0;
	float	Angle1;
	float	LifeTime;
	float	Time;
	float	FadeIn;
	float	FadeOut;
	uint	ImageIndex;
};

struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	float4		CameraForward;
	float4		CameraRight;
	float4		CameraUp;
	float4		Gravity;
	int			MaxParticles;
	float		DeltaTime;
	uint		DeadListSize;
};

cbuffer CB1 : register(b0) { 
	PARAMS Params; 
};

SamplerState						Sampler				: 	register(s0);

Texture2D							Texture 			: 	register(t0);

StructuredBuffer<PARTICLE>			injectionBuffer		:	register(t1);
StructuredBuffer<PARTICLE>			particleBufferGS	:	register(t2);

RWStructuredBuffer<PARTICLE>		particleBuffer		: 	register(u0);

#ifdef INJECTION
ConsumeStructuredBuffer<uint>		deadParticleIndices	: 	register(u1);
#endif
#if (defined SIMULATION) || (defined INITIALIZE)
AppendStructuredBuffer<uint>		deadParticleIndices	: 	register(u1);
#endif

/*-----------------------------------------------------------------------------
	Simulation :
-----------------------------------------------------------------------------*/
#if (defined INJECTION) || (defined SIMULATION) || (defined INITIALIZE)
[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int id = dispatchThreadID.x;
	
#ifdef INITIALIZE
	deadParticleIndices.Append(id);
#endif	

#ifdef INJECTION
	if (id < Params.MaxParticles && Params.DeadListSize > MAX_INJECTED ) {
		PARTICLE p = injectionBuffer[ id ];
		
		uint newIndex = deadParticleIndices.Consume();
		
		particleBuffer[ newIndex ] = p;
	}
#endif

#ifdef SIMULATION
	if (id < Params.MaxParticles) {
		PARTICLE p = particleBuffer[ id ];
		
		if (p.LifeTime>0) {
			if (p.Time < p.LifeTime) {
				p.Time += Params.DeltaTime;
			} else {
				p.LifeTime = -1;
				deadParticleIndices.Append( id );
			}
		}
		
		particleBuffer[ id ] = p;
	}
#endif
}
#endif


/*-----------------------------------------------------------------------------
	Rendering :
-----------------------------------------------------------------------------*/

struct VSOutput {
	int vertexID : TEXCOORD0;
};

struct GSOutput {
	float4	Position : SV_Position;
	float2	TexCoord : TEXCOORD0;
	float2	TexCoord1 : TEXCOORD1;
	float 	TexCoord2 : TEXCOORD2;
	float3	TexCoord3 : TEXCOORD3;
	float4	Color    : COLOR0;
	int4	Color1    : COLOR1;
};


#if DRAW
VSOutput VSMain( uint vertexID : SV_VertexID )
{
	VSOutput output;
	output.vertexID = vertexID;
	return output;
}


float Ramp(float f_in, float f_out, float t) 
{
	float y = 1;
	t = saturate(t);
	
	float k_in	=	1 / f_in;
	float k_out	=	-1 / (1-f_out);
	float b_out =	-k_out;	
	
	if (t<f_in)  y = t * k_in;
	if (t>f_out) y = t * k_out + b_out;
	
	return y;
}



[maxvertexcount(6)]
void GSMain( point VSOutput inputPoint[1], inout TriangleStream<GSOutput> outputStream )
{
	GSOutput p0, p1, p2, p3;
	
	p0.TexCoord1 = 0; p0.TexCoord2 = 0; p0.TexCoord3 = 0; p0.Color1 = 0;
	p1.TexCoord1 = 0; p1.TexCoord2 = 0; p1.TexCoord3 = 0; p1.Color1 = 0;
	p2.TexCoord1 = 0; p2.TexCoord2 = 0; p2.TexCoord3 = 0; p2.Color1 = 0;
	p3.TexCoord1 = 0; p3.TexCoord2 = 0; p3.TexCoord3 = 0; p3.Color1 = 0;
	
	PARTICLE prt = particleBufferGS[ inputPoint[0].vertexID ];
	
	if (prt.Time >= prt.LifeTime ) {
		return;
	}
	
	float factor	=	saturate(prt.Time / prt.LifeTime);
	
	float  sz 		=   lerp( prt.Size0, prt.Size1, factor )/2;
	float  time		=	prt.Time;
	float4 color	=	lerp( prt.Color0, prt.Color1, Ramp( prt.FadeIn, prt.FadeOut, factor ) );
	float3 position	=	prt.Position + prt.Velocity * time + prt.Acceleration * time * time / 2;
	float  a		=	lerp( prt.Angle0, prt.Angle1, factor );	

	float2x2	m	=	float2x2( cos(a), sin(a), -sin(a), cos(a) );
	
	float3		rt	=	(Params.CameraRight.xyz * cos(a) + Params.CameraUp.xyz * sin(a)) * sz;
	float3		up	=	(Params.CameraUp.xyz * cos(a) - Params.CameraRight.xyz * sin(a)) * sz;
	
	p0.Position	= mul( float4( position + rt + up, 1 ), Params.View );
	p0.Position	= mul( p0.Position, Params.Projection );
	p0.TexCoord	= float2(1,1);
	p0.Color 	= color;
	
	p1.Position	= mul( float4( position - rt + up, 1 ), Params.View );
	p1.Position	= mul( p1.Position, Params.Projection );
	p1.TexCoord	= float2(0,1);
	p1.Color 	= color;
	
	p2.Position	= mul( float4( position - rt - up, 1 ), Params.View );
	p2.Position	= mul( p2.Position, Params.Projection );
	p2.TexCoord	= float2(0,0);
	p2.Color 	= color;
	
	p3.Position	= mul( float4( position + rt - up, 1 ), Params.View );
	p3.Position	= mul( p3.Position, Params.Projection );
	p3.TexCoord	= float2(1,0);
	p3.Color 	= color;

	outputStream.Append(p0);
	outputStream.Append(p1);
	outputStream.Append(p2);
	
	outputStream.RestartStrip();

	outputStream.Append(p0);
	outputStream.Append(p2);
	outputStream.Append(p3);

	outputStream.RestartStrip();
}



float4 PSMain( GSOutput input ) : SV_Target
{
	return /*Texture.Sample( Sampler, input.TexCoord ) **/ input.Color * 8;
}
#endif

