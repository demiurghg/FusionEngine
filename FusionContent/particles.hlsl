
#if 0
$ubershader INITIALIZE|INJECTION|SIMULATION|DRAW
#endif

// change ParticleSystem.cs too!
#define BLOCK_SIZE		256		
#define MAX_INJECTED 	4096
#define MAX_PARTICLES 	(256*256)
#define MAX_IMAGES		512

#define BEAM			0x0001
#define LIT				0x0002
#define SHADOW			0x0004

struct PARTICLE {
	float3	Position;
	float3	Velocity;
	float3	Acceleration;
	float3	TailPosition;
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
	uint 	Effects;
};

struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	float4		CameraForward;
	float4		CameraRight;
	float4		CameraUp;
	float4		CameraPosition;
	float4		Gravity;
	int			MaxParticles;
	float		DeltaTime;
	uint		DeadListSize;
};

cbuffer CB1 : register(b0) { 
	PARAMS Params; 
};

cbuffer CB2 : register(b1) { 
	float4 Images[MAX_IMAGES]; 
};


//-----------------------------------------------
//	States :
//-----------------------------------------------
SamplerState	Sampler	 : 	register(s0);

//-----------------------------------------------
//	SRVs :
//-----------------------------------------------
Texture2D						Texture 				: 	register(t0);
StructuredBuffer<PARTICLE>		injectionBuffer			:	register(t1);
StructuredBuffer<PARTICLE>		particleBufferGS		:	register(t2);
StructuredBuffer<float2>		sortParticleBufferGS	:	register(t3);

//-----------------------------------------------
//	UAVs :
//-----------------------------------------------
RWStructuredBuffer<PARTICLE>	particleBuffer		: 	register(u0);

#ifdef INJECTION
ConsumeStructuredBuffer<uint>	deadParticleIndices	: 	register(u1);
#endif
#if (defined SIMULATION) || (defined INITIALIZE)
AppendStructuredBuffer<uint>	deadParticleIndices	: 	register(u1);
#endif

RWStructuredBuffer<float2>		sortParticleBuffer	: 	register(u2);

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
	//	id must be less than max injected particles.
	//	dead list must contain at leas MAX_INJECTED indices to prevent underflow.
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

		//	meausure distance :
		float  time		=	p.Time;
		float3 accel	=	p.Acceleration + Params.Gravity.xyz * p.Gravity;
		float3 position	=	p.Position + p.Velocity * time + accel * time * time / 2;
		
		float4 ppPos	=	mul( mul( float4(position,1), Params.View ), Params.Projection );

		sortParticleBuffer[ id ] = float2( -abs(ppPos.z / ppPos.w), id );
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
	float4	Color     : COLOR0;
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
	
	uint prtId = (uint)( sortParticleBufferGS[ inputPoint[0].vertexID ].y );
	//uint prtId = inputPoint[0].vertexID;
	
	PARTICLE prt = particleBufferGS[ prtId ];
	
	if (prt.Time >= prt.LifeTime ) {
		return;
	}
	
	float factor	=	saturate(prt.Time / prt.LifeTime);
	
	float  sz 		=   lerp( prt.Size0, prt.Size1, factor )/2;
	float  time		=	prt.Time;
	float4 color	=	lerp( prt.Color0, prt.Color1, Ramp( prt.FadeIn, prt.FadeOut, factor ) );
	float3 accel	=	prt.Acceleration + Params.Gravity.xyz * prt.Gravity;
	float3 position	=	prt.Position     + prt.Velocity * time + accel * time * time / 2;
	float3 tailpos	=	prt.TailPosition + prt.Velocity * time + accel * time * time / 2;
	float  a		=	lerp( prt.Angle0, prt.Angle1, factor );	

	float2x2	m	=	float2x2( cos(a), sin(a), -sin(a), cos(a) );
	
	float3		rt	=	(Params.CameraRight.xyz * cos(a) + Params.CameraUp.xyz * sin(a)) * sz;
	float3		up	=	(Params.CameraUp.xyz * cos(a) - Params.CameraRight.xyz * sin(a)) * sz;
	
	float4		image	=	Images[prt.ImageIndex ];
	
	float4 pos0	=	mul( float4( position + rt + up, 1 ), Params.View );
	float4 pos1	=	mul( float4( position - rt + up, 1 ), Params.View );
	float4 pos2	=	mul( float4( position - rt - up, 1 ), Params.View );
	float4 pos3	=	mul( float4( position + rt - up, 1 ), Params.View );
	
	if (prt.Effects && BEAM == BEAM) {
		float3 dir	=	normalize(position - tailpos);
		float3 eye	=	normalize(Params.CameraPosition.xyz - tailpos);
		float3 side	=	normalize(cross( eye, dir ));
		pos0		=	mul( float4( tailpos  + side * sz, 1 ), Params.View );
        pos1		=	mul( float4( position + side * sz, 1 ), Params.View );
        pos2		=	mul( float4( position - side * sz, 1 ), Params.View );
	    pos3		=	mul( float4( tailpos  - side * sz, 1 ), Params.View );
	}
	
	p0.Position	= mul( pos0, Params.Projection );
	p0.TexCoord	= float2(image.z, image.y);
	p0.Color 	= color;
	
	p1.Position	= mul( pos1, Params.Projection );
	p1.TexCoord	= float2(image.x, image.y);
	p1.Color 	= color;
	
	p2.Position	= mul( pos2, Params.Projection );
	p2.TexCoord	= float2(image.x, image.w);
	p2.Color 	= color;
	
	p3.Position	= mul( pos3, Params.Projection );
	p3.TexCoord	= float2(image.z, image.w);
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
	float4 color	=	Texture.Sample( Sampler, input.TexCoord ) * input.Color;
	
	//	saves about 5%-10% of rasterizer time:
	clip( color.a < 0.001f ? -1:1 );
	return color;
}
#endif

