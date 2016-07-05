
#if 0
$ubershader DYNAMIC INJECTION|SIMULATION|MOVE EULER|RUNGE_KUTTA|COLOR
$ubershader STATIC INJECTION|MOVE|REDUCTION|(SIMULATION EULER|RUNGE_KUTTA)|LOCAL
$ubershader DRAW POINT|LINE
#endif

#define BLOCK_SIZE 256
#define WARP_SIZE 16
#define HALF_BLOCK BLOCK_SIZE/2

struct PARTICLE3D {
	float3	Position; // 3 coordinates
	float3	Velocity;
	float4	Color0;
	float	Size0;
	float	TotalLifeTime;
	float	LifeTime;
	int		LinksPtr;
	int		LinksCount;
	float3	Acceleration;
	float	Mass;
	float	Charge;
	int		Id;
	float	ColorType;
	int		Count;
	int		Group;
	int		Information;
	float	Energy;
	float3	Force;
	int		Cluster;
};

//struct 
struct LinkId {
	int id;
};


struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	int			MaxParticles;
	float		DeltaTime;
	float		LinkSize;
	float		CalculationRadius;
	float		Mass;
	int			StartIndex;
	int			EndIndex;
};


struct Link {
	int		par1;
	int		par2;
	float	length;
	float	force2;
	float3	orientation;
	float	weight;	
	int		linkType;
	float4	Color;
	float	Width;
	float	Time;
	float	TotalLifeTime;
	float	LifeTime;
};



SamplerState						Sampler				: 	register(s0);
Texture2D							Texture 			: 	register(t0);
Texture2D							Stroke 				: 	register(t4);
Texture2D							Border 				: 	register(t5);

RWStructuredBuffer<PARTICLE3D>		particleBufferSrc	: 	register(u0);
StructuredBuffer<PARTICLE3D>		GSResourceBuffer	:	register(t1);

StructuredBuffer<LinkId>			linksPtrBuffer		:	register(t2);
StructuredBuffer<Link>				linksBuffer			:	register(t3);

StructuredBuffer<PARTICLE3D>		particleReadBuffer	:	register(t4);
StructuredBuffer<PARTICLE3D>		particleReadBuffer2	:	register(t5);

RWStructuredBuffer<float4>			energyRWBuffer		:	register(u1);

cbuffer CB1 : register(c0) { 
	PARAMS Params; 
};

/*-----------------------------------------------------------------------------
	Simulation :
-----------------------------------------------------------------------------*/




#ifdef STATIC

groupshared float4 shPositions[BLOCK_SIZE];
groupshared float4 sh_energy[BLOCK_SIZE];


inline float4 pairBodyForce( float4 thisPos, float4 otherPos ) // 4th component is charge
{
	float3 R			= (otherPos - thisPos).xyz;		
	float Rsquared		= R.x * R.x + R.y * R.y + R.z * R.z + 0.1f;
	float Rsixth		= Rsquared * Rsquared * Rsquared;
	float invRCubed		= - otherPos.w / sqrt( Rsixth );	// we will multiply by constants later
	float energy		=   otherPos.w / sqrt( Rsquared );	// we will multiply by constants later
	return float4( mul( invRCubed, R ), energy ); // we write energy into the 4th component
}



float4 springForce( float4 pos, float4 otherPos ) // 4th component in otherPos is link length
												  // 4th component in pos is link strength
{
	float3 R			= (otherPos - pos).xyz;
	float Rabs			= length( R ) + 0.1f;
	float deltaR		= Rabs - otherPos.w;
	float absForce		= pos.w * ( deltaR ) / ( Rabs );
	float energy		= 0.5f * pos.w * deltaR * deltaR;
	return float4( mul( absForce, R ), energy ) * 0.0005f;  // we write energy into the 4th component
}


float4 tileForce( float4 position, uint threadId )
{
	float4 force = float4(0, 0, 0, 0);
	float4 otherPosition = float4(0, 0, 0, 0);
	for ( uint i = 0; i < BLOCK_SIZE; ++i )
	{
		otherPosition = shPositions[i];
		force += pairBodyForce( position, otherPosition );
	}
	return force;
}

float4 calcRepulsionForce( float4 position, uint3 groupThreadID )
{
	float4 force = float4(0, 0, 0, 0);
	uint tile = 0;
	for ( uint i = 0; i < Params.MaxParticles; i+= BLOCK_SIZE, tile += 1 )
	{
		uint srcId = tile*BLOCK_SIZE + groupThreadID.x;
		PARTICLE3D p = particleBufferSrc[srcId];
		float4 pos = float4( p.Position, p.Charge );
		shPositions[groupThreadID.x] = pos;
		
		GroupMemoryBarrierWithGroupSync();
		
		force += tileForce( position, groupThreadID.x );

		GroupMemoryBarrierWithGroupSync();
	}
	return force;
}


float4 calcLinksForce( float4 pos, uint id, uint linkListStart, uint linkCount )
{
	float4 force = float4( 0, 0, 0, 0 );
	PARTICLE3D otherP;
	[allow_uav_condition] for ( uint i = 0; i < linkCount; ++i )
	{
		Link link = linksBuffer[linksPtrBuffer[linkListStart + i].id];
		uint otherId = link.par1;
		if ( id == otherId )
		{
			otherId = link.par2;
		}
		otherP = particleBufferSrc[otherId];
		float4 otherPos = float4( otherP.Position, link.length );
		pos.w = link.weight; //1.0f;//
		force += springForce( pos, otherPos );
	}
	return force;
}


#ifdef SIMULATION
[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	uint id = groupID.x*BLOCK_SIZE + groupThreadID.x;
	
	PARTICLE3D p = particleBufferSrc[id];
	float4 pos = float4 ( p.Position, p.Charge );
	float4 force = float4( 0, 0, 0, 0 );

#ifdef EULER
	force = mul( calcRepulsionForce( pos, groupThreadID ), 100000.0f * pos.w ); // we multiply by all the constants here once

	force += calcLinksForce ( pos, id, p.LinksPtr, p.LinksCount );

#endif // EULER



#ifdef RUNGE_KUTTA
	force = float4( 0, 0, 0, 0 ); // just a placeholder
#endif // RUNGE_KUTTA

	// add potential well:
//	force.xyz += mul( 0.00005f*length(pos.xyz), -pos.xyz );
//	float3 accel = force.xyz;

	float invMass = 1 / Params.Mass;
	float3 acc = float3(0,0,0);
	acc += mul( force, invMass );
	p.Acceleration = acc;
	p.Force		= force.xyz;
	p.Energy	= force.w;
	particleBufferSrc[id] = p;
}

#endif // SIMULATION


#ifdef LOCAL
[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int numberOfVertices = Params.EndIndex - Params.StartIndex;
	if( dispatchThreadID.x < numberOfVertices )
	{
		uint id = dispatchThreadID.x;
		PARTICLE3D prt = particleBufferSrc[id];

		float4 force = float4(0,0,0,0);

		float Radius = 400;

		float3 R = prt.Position - float3(0,0,0);
		float Rabs = length(R) + 0.01f;

		float diff = Radius - Rabs;

		float factor = 0.003f;

		force.xyz += mul(R, factor*diff/Rabs);
		float invMass = 1 / Params.Mass;
		float3 acc = float3(0,0,0);
		acc += mul( force, invMass );
		prt.Acceleration = acc;
		prt.Force += force.xyz;
		prt.Energy += force.w;
		particleBufferSrc[id] = prt;
	}
}

#endif // LOCAL


#ifdef MOVE
[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	uint id = dispatchThreadID.x;

	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[ id ]; //PARTICLE3D p = particleReadBuffer[ id ];
		//p.Position.xyz += mul( p.Force, Params.DeltaTime );
		p.Position.xyz += mul( p.Velocity, Params.DeltaTime );
		p.Velocity += mul( p.Acceleration, Params.DeltaTime );
		particleBufferSrc[ id ] = p;
	}

}
#endif // MOVE



#ifdef REDUCTION

[numthreads( HALF_BLOCK, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
//	int id = dispatchThreadID.x;
	int id = groupID.x*BLOCK_SIZE + groupIndex;

	// load data into shared memory:
	PARTICLE3D p1curr = particleReadBuffer[ id ];
	PARTICLE3D p2curr = particleReadBuffer[ id + HALF_BLOCK ];

	PARTICLE3D p1next = particleReadBuffer2[ id ];
	PARTICLE3D p2next = particleReadBuffer2[ id + HALF_BLOCK ];

	float energy1 = p1next.Energy;
	float energy2 = p2next.Energy;

	float mass	= p1next.Mass + p2next.Mass;

	float dotPr1 = - (p1curr.Force.x*p1next.Force.x + p1curr.Force.y*p1next.Force.y + p1curr.Force.z*p1next.Force.z);
	float dotPr2 = - (p2curr.Force.x*p2next.Force.x + p2curr.Force.y*p2next.Force.y + p2curr.Force.z*p2next.Force.z);

	sh_energy[groupIndex] = float4( energy1 + energy2, dotPr1 + dotPr2, mass, 0 );
	GroupMemoryBarrierWithGroupSync();

	// full unroll:
	if ( HALF_BLOCK >= 512 )
	{
		if ( groupIndex < 256 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 256];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 256 )
	{
		if ( groupIndex < 128 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 128];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 128 )
	{
		if ( groupIndex < 64 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 64];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 64 )
	{
		if ( groupIndex < 32 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 32];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 32 )
	{
		if ( groupIndex < 16 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 16];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 16 )
	{
		if ( groupIndex < 8 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 8];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 8 )
	{
		if ( groupIndex < 4 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 4];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 4 )
	{
		if ( groupIndex < 2 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 2];}
		GroupMemoryBarrierWithGroupSync();
	}

	if ( HALF_BLOCK >= 2 )
	{
		if ( groupIndex < 1 ) {sh_energy[groupIndex] += sh_energy[groupIndex + 1];}
		GroupMemoryBarrierWithGroupSync();
	}

	if( groupIndex == 0 )
	{
		energyRWBuffer[groupID.x] = sh_energy[0];
	}
}

#endif // REDUCTION


#endif // STATIC


#ifdef DYNAMIC

struct BodyState
{
	float3 Position;
	float3 Velocity;
	float3 Acceleration;
	uint id;
};


struct Derivative
{
	float3 dxdt;
	float3 dvdt;
};



float3 SpringForce( in float3 bodyState, in float3 otherBodyState, float linkLength, float charge1, float charge2, float linkActivity )
{
	float3 R			= otherBodyState - bodyState;	
	float Rabs			= length( R ) + 0.5f;  			  	
	float absForce		= 0.1f * ( Rabs - linkLength ) / ( Rabs );		  		
	return mul( Rabs, R * 0.01f )  * 0.001f  * pow(linkActivity, 2);//mul( Rabs, tangForce * 0.000001f )  * 0.0000005f   ;//* (charge2 + 1)
}


float3 RepulsionForce( in float3 bodyState, in float3 otherBodyState, float charge1, float charge2)
{
	float3 R			= (otherBodyState  - bodyState ) ;			
	float Rsquared		= R.x * R.x + R.y * R.y + R.z * R.z + 0.001f;	
	float Rsixth		= Rsquared * Rsquared * Rsquared;
	float invRCubed		=  -1000.0f / sqrt( Rsixth );
	return mul( invRCubed, R ) * 1000 * (charge1 + 1) * (charge2 + 1) ;// / pow(group2 + 2, 3.0f) ; +  normalize( forceCentr) * bodySize / 2
}



float3 Acceleration( in PARTICLE3D prt, in int totalNum, in int particleId  )
{
	float3 acc = {0,0,0};
	float3 deltaForce = {0, 0, 0};
	float invMass = 1 / Params.Mass;

	PARTICLE3D other;
	[allow_uav_condition] for ( int lNum = 0; lNum < prt.Count; ++ lNum ) {
		int otherId = linksBuffer[linksPtrBuffer[prt.LinksPtr + lNum].id].par1;

		if ( otherId == particleId ) {
			otherId = linksBuffer[linksPtrBuffer[prt.LinksPtr + lNum].id].par2;
		}

		other = particleBufferSrc[otherId];
		Link link = linksBuffer[linksPtrBuffer[prt.LinksPtr + lNum].id];
		float activity = link.LifeTime / link.TotalLifeTime;
		if (activity < 0.02f){
			activity = 0.02f;
		}
		deltaForce += SpringForce( prt.Position, other.Position, link.length, prt.LinksCount, other.LinksCount, activity);
	}
	
	[allow_uav_condition] for ( int i = 0; i < totalNum; ++i ) {
		other = particleBufferSrc[ i ];
		float dist = length (prt.Position - other.Position);
		if(dist <= (prt.Size0 + other.Size0) * 10){
			
			//if (prt.Group == other.Group){
				deltaForce += RepulsionForce( prt.Position, other.Position, prt.LinksCount, other.LinksCount ) * 10;
			//}
		}
	}
	
	float4 force = float4(0,0,0,0);
	float Radius = prt.ColorType;
	if (prt.Charge == 0) Radius = Radius * 1.1f;

	float3 R = prt.Position - float3(0,0,0);
	float Rabs = length(R) + 0.01f;

	float diff = Radius - Rabs;

	float factor = 200.0f;

	force.xyz += mul(R, factor * diff/Rabs);
	//p.Force += force.xyz;
	float n = 55;
	float alpha = 2 * 3.1415926 / n;

	float3 clusterPoint = float3(0,0,0);
	

	float3 corVector =  clusterPoint - float3(0, 0, Radius);

	alpha = alpha * (prt.TotalLifeTime - 1);
	corVector = mul(corVector, float3x3(1, 0, 0,
												0, cos(alpha), -sin(alpha),
												0, sin(alpha), cos(alpha)));
	float cVabs			= length( corVector ) + 0.5f;
	float4 corForce = float4(0,0,0,0);
	//corForce.xyz += mul( cVabs, corVector ) * 0.0000001f;
	

	acc += mul( deltaForce, invMass );
	acc += mul( force, invMass );
	acc += mul( corForce, invMass );
	acc -= mul ( prt.Velocity, 50.0f );

	return acc;
}




void IntegrateEUL_SHARED( inout BodyState state, in uint numParticles )
{	
	state.Acceleration	= Acceleration( particleBufferSrc[state.id], numParticles, state.id );
}



[numthreads( BLOCK_SIZE, 1, 1 )]
void CSMain( 
	uint3 groupID			: SV_GroupID,
	uint3 groupThreadID 	: SV_GroupThreadID, 
	uint3 dispatchThreadID 	: SV_DispatchThreadID,
	uint  groupIndex 		: SV_GroupIndex
)
{
	int id = dispatchThreadID.x;

#ifdef INJECTION
	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[id];
		particleBufferSrc[id] = p;
	}
#endif

#ifdef SIMULATION
	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[ id ];		

			uint numParticles	=	0;
			uint stride			=	0;
			particleBufferSrc.GetDimensions( numParticles, stride );

			BodyState state;
			state.Position		=	p.Position;
			state.Velocity		=	p.Velocity;
			state.Acceleration	=	p.Acceleration;
			state.id			=	id;

#ifdef EULER

			IntegrateEUL_SHARED( state, Params.MaxParticles );

#endif

#ifdef RUNGE_KUTTA
	
			IntegrateEUL_SHARED( state, Params.MaxParticles );

#endif
			
			p.Acceleration = state.Acceleration;		
			particleBufferSrc[id] = p;

	}
#endif

#ifdef MOVE
	if (id < Params.MaxParticles) {
		PARTICLE3D p = particleBufferSrc[ id ];		

		p.Position.xyz += mul( p.Velocity, Params.DeltaTime );
		p.Velocity += mul( p.Acceleration, Params.DeltaTime );
		
		particleBufferSrc[ id ] = p;

	}
#endif

}
#endif



#ifdef DRAW

struct VSOutput {
int vertexID : TEXCOORD0;
};

struct GSOutput {
	float4	Position : SV_Position;
	float2	TexCoord : TEXCOORD0;
	float4	Color    : COLOR0;
	float2	SecondTex: TEXCOORD1;
	float2	IconCoord: TEXCOORD2;
};


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


#ifdef POINT
[maxvertexcount(26)]
void GSMain( point VSOutput inputPoint[1], inout TriangleStream<GSOutput> outputStream )
{
	GSOutput p0, p1, p2, p3;
	PARTICLE3D prt = GSResourceBuffer[ inputPoint[0].vertexID ];

		//float factor = saturate(prt.LifeTime / prt.TotalLifeTime);

	float sz				=  prt.Size0 / 2;		
	float time				= prt.LifeTime;
	float4 pos				=	float4( prt.Position.xyz, 1 );
	float4 posV				=	mul( pos, Params.View );
		
	float texRight	= prt.ColorType;
	float texLeft	= texRight - 0.1f;

	p0.Position = mul( posV + float4( sz, sz, 0, 0 ) , Params.Projection );
	p0.TexCoord = float2(0, 0);
	p0.Color = prt.Color0;
	p0.SecondTex = float2(0,0);
	p0.IconCoord = float2(texLeft,0);

	p1.Position = mul( posV + float4( -sz, sz, 0, 0 ) , Params.Projection );
	p1.TexCoord = float2(1, 0);
	p1.Color = prt.Color0;
	p1.SecondTex = float2(0,0);
	p1.IconCoord = float2(texRight,0);

	p2.Position = mul( posV + float4( -sz, -sz, 0, 0 ) , Params.Projection );
	p2.TexCoord = float2(1, 1);
	p2.Color = prt.Color0;
	p2.SecondTex = float2(0,0);
	p2.IconCoord = float2(texRight,1);

	p3.Position = mul( posV + float4( sz, -sz, 0, 0 ) , Params.Projection );
	p3.TexCoord = float2(0, 1);
	p3.Color = prt.Color0;
	p3.SecondTex = float2(0,0);
	p3.IconCoord = float2(texLeft, 1);
	
	outputStream.Append(p0);
	outputStream.Append(p1);
	outputStream.Append(p2);
	outputStream.RestartStrip( );
	
	outputStream.Append(p0);
	outputStream.Append(p2);
	outputStream.Append(p3);
	outputStream.RestartStrip();
}
#endif

#ifdef LINE
[maxvertexcount(24)]
void GSMain( point VSOutput inputLine[1], inout TriangleStream<GSOutput> outputStream )
{
	Link lk = linksBuffer[ inputLine[0].vertexID ];

	PARTICLE3D end1 = GSResourceBuffer[ lk.par1 ];
	PARTICLE3D end2 = GSResourceBuffer[ lk.par2 ];

	float4 pos1 = float4( end1.Position.xyz, 1 );
	float4 pos2 = float4( end2.Position.xyz, 1 );

	GSOutput p1, p2, p3, p4;

	pos1 = mul(pos1 , Params.View);
	pos2 = mul(pos2 , Params.View);
	float3 dir = normalize(pos2 - pos1);
	if (length(dir) == 0 ) return;

	float3 side = normalize(cross(dir, float3(0,0,-1)));					
	
	p1.TexCoord		=	float2(0, 1);
	p2.TexCoord		=	float2(0, 0);
	p3.TexCoord		=	float2(0, 1);
	p4.TexCoord		=	float2(0, 0);
									
	p1.Color		=	lk.Color;
	p2.Color		=	lk.Color;
	p3.Color		=	lk.Color;
	p4.Color		=	lk.Color;
				
	p1.IconCoord		=	float2(0, 0);
	p2.IconCoord		=	float2(0, 0);
	p3.IconCoord		=	float2(0, 0);
	p4.IconCoord		=	float2(0, 0);
	
	p1.SecondTex		=	float2(0, 0);
	p2.SecondTex		=	float2(0, 0);
	p3.SecondTex		=	float2(0, 0);
	p4.SecondTex		=	float2(0, 0);
				
	p1.Position = mul( pos1 + float4(side * lk.Width, 0)  /*+ float4(dir * end1.Size0 , 0)*/, Params.Projection ) ;	
	p2.Position = mul( pos1 - float4(side * lk.Width, 0)  /*+ float4(dir * end1.Size0 , 0)*/, Params.Projection ) ;	
	p3.Position = mul( pos2 + float4(side * lk.Width, 0)  /*- float4(dir * end2.Size0 , 0)*/, Params.Projection ) ;	
	p4.Position = mul( pos2 - float4(side * lk.Width, 0)  /*- float4(dir * end2.Size0 , 0)*/, Params.Projection ) ;	
	
	outputStream.Append(p1);
	outputStream.Append(p2);
	outputStream.Append(p3);
	outputStream.Append(p4);
	outputStream.RestartStrip();			
}
#endif

#ifdef LINE
float4 PSMain( GSOutput input ) : SV_Target
{
	return float4(input.Color.xyz, 0.7f) ;//* Stroke.Sample( Sampler, input.TexCoord ) +  Border.Sample( Sampler, input.TexCoord );
}
#endif

#ifdef POINT
float4 PSMain( GSOutput input ) : SV_Target
{
	clip( Texture.Sample( Sampler, input.TexCoord ).a < 0.7f ? -1:1 );
	clip( input.Color.a < 0.1f ? -1:1 );
	float4 color = Texture.Sample( Sampler, input.TexCoord ) * input.Color ;
	if( input.SecondTex.x > 0)
	{
		float4 result =	Texture.Sample( Sampler, input.TexCoord );
		if(result.a > 0)
		{
			//return result * float4(0,255,0, 255) / 255;
			//color =  Stroke.Sample( Sampler, input.IconCoord ) ; //
		}

	}
	return color ;//+ float4 (Border.Sample( Sampler, input.TexCoord ).rgb, 0); //input.Color;

}
#endif


#endif