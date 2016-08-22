#if 0
$ubershader COMPUTE INJECTION|MOVE|REDUCTION|(SIMULATION EULER|RUNGE_KUTTA +LINKS)|LOCAL
#endif


#define BLOCK_SIZE 256
#define WARP_SIZE 16
#define HALF_BLOCK BLOCK_SIZE/2

struct PARAMS {
	float4		LocalCenter;
	uint		MaxParticles;
	int			StartIndex;
	int			EndIndex;
	float		DeltaTime;
	float		LocalRadius;
};

cbuffer CB1 : register(b0) {
	PARAMS Params; 
};


struct PARTICLE3D {
	float3	Position;
	float3	Velocity;
	float3	Force;
	float	Energy;
	float	Mass;
	float	Charge;

	float4	Color0;
	float	Size0;
	int		LinksPtr;
	int		LinksCount;
	float	DesiredRadius;
	int		Information;
	int		Group;
	int		Cluster;
};


struct LinkId {
	int id;
};

struct Link {
	uint par1;
	uint par2;
	float length;
	float strength;
};


RWStructuredBuffer<PARTICLE3D>		particleRWBuffer	: 	register(u0);

StructuredBuffer<PARTICLE3D>		particleReadBuffer	:	register(t0);
StructuredBuffer<PARTICLE3D>		particleReadBuffer2	:	register(t1);

RWStructuredBuffer<float4>			energyRWBuffer		:	register(u1);

StructuredBuffer<LinkId>			linksPtrBuffer		:	register(t2);
StructuredBuffer<Link>				linksBuffer			:	register(t3);

StructuredBuffer<int>				selectBuffer		:	register(t4);





#ifdef COMPUTE

groupshared float4 shPositions[BLOCK_SIZE];
groupshared float4 sh_energy[BLOCK_SIZE];


inline float4 pairBodyForce( float4 thisPos, float4 otherPos ) // 4th component is charge
{
	float3 R			= (otherPos - thisPos).xyz;		
	float Rsquared		= R.x * R.x + R.y * R.y + R.z * R.z + 0.1f;
	float Rsixth		= Rsquared * Rsquared * Rsquared;
	float invRCubed		= - otherPos.w  /sqrt( Rsixth );	// we will multiply by constants later
	float energy		=   otherPos.w / sqrt( Rsquared );	// we will multiply by constants later
	return float4( mul( invRCubed, R ), energy ) ; // we write energy into the 4th component
}



float4 springForce( float4 pos, float4 otherPos ) // 4th component in otherPos is link length
												  // 4th component in pos is link strength
{
	float3 R			= (otherPos - pos).xyz;
	float Rabs			= length( R ) + 0.1f;
	float deltaR		= Rabs - otherPos.w;
	float absForce		= pos.w * ( deltaR ) / ( Rabs );
	float energy		= 0.0005f * pos.w * deltaR * deltaR;
	return float4( mul( absForce, R ), energy ) * 0.00005f;  // we write energy into the 4th component
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
		PARTICLE3D p = particleRWBuffer[srcId];
		float4 pos = float4( p.Position, p.Charge );
		shPositions[groupThreadID.x] = pos;
		
		GroupMemoryBarrierWithGroupSync();
		
		force += tileForce( position, groupThreadID.x ) ;

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
		otherP = particleRWBuffer[otherId];
		float4 otherPos = float4( otherP.Position, link.length );
		pos.w = link.strength ; //1.0f;//
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
	
	PARTICLE3D p = particleRWBuffer[id];
	float4 pos = float4 ( p.Position, p.Charge );
	float4 force = float4( 0, 0, 0, 0 );

#ifdef EULER
	force = mul( calcRepulsionForce( pos, groupThreadID ), 100.0f / pos.w ); // we multiply by all the constants here once
#ifdef LINKS
	force += calcLinksForce ( pos, id, p.LinksPtr, p.LinksCount ) / 100;
#endif // LINKS
#endif // EULER



#ifdef RUNGE_KUTTA
	force = float4( 0, 0, 0, 0 ); // just a placeholder
#endif // RUNGE_KUTTA

	// add potential well:
//	force.xyz += mul( 0.00005f*length(pos.xyz), -pos.xyz );

//	float3 accel = force.xyz;

	p.Force		= force.xyz;
	p.Energy	= force.w;

	float4 forceCenter = float4(0,0,0,0);

	float Radius = p.DesiredRadius;
	//if (p.Information == 0) Radius = Radius * 1.1f;

	float3 R = p.Position - float3(0,0,0);
	float Rabs = length(R) + 0.01f;

	float diff = Radius - Rabs;

	//float factor = 0.0f;

	float factor = 0.00005f;

	forceCenter.xyz += mul(R, factor*diff/Rabs);
	if(p.Group == 1){
		p.Force += forceCenter.xyz;
	}
	


	//TO CENTER
	forceCenter = float4(0,0,0,0);

	Radius = 0;//Params.LocalRadius;

	R = p.Position - float3(0,0,0);
	Rabs = length(R) + 0.01f;

	diff = Radius - Rabs;

	factor = 0.000005f;

	forceCenter.xyz += mul(R, factor*diff/Rabs);
	p.Force += forceCenter.xyz;

	particleRWBuffer[id] = p;
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
		uint id = selectBuffer[Params.StartIndex + dispatchThreadID.x];
		PARTICLE3D prt = particleRWBuffer[id];

		float4 force = float4(0,0,0,0);

		float Radius = Params.LocalRadius;

		float3 R = prt.Position - Params.LocalCenter;
		float Rabs = length(R) + 0.01f;

		float diff = Radius - Rabs;

		float factor = 0.3f;

		force.xyz += mul(R, factor*diff/Rabs);
		prt.Force += force.xyz;
		prt.Energy += force.w;
		particleRWBuffer[id] = prt;
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
		PARTICLE3D p = particleReadBuffer[ id ];
		
//		p.Position.xyz += mul( p.Velocity, Params.DeltaTime );
//		p.Velocity += mul( p.Force, Params.DeltaTime );

		p.Position.xyz += mul( p.Force, Params.DeltaTime * 1000 );
		particleRWBuffer[ id ] = p;
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

	//float forceSq1 = - (p1.Force.x*p1.Force.x + p1.Force.y*p1.Force.y + p1.Force.z*p1.Force.z);
	//float forceSq2 = - (p2.Force.x*p2.Force.x + p2.Force.y*p2.Force.y + p2.Force.z*p2.Force.z);

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

	// this does not work on every GPU:
//	if ( groupIndex < 8 )
//	{
////		if ( BLOCK_SIZE >= 32 ) { sh_energy[groupIndex] += sh_energy[groupIndex + 16]; }
//	/*	if ( BLOCK_SIZE >= 16 )*/ { sh_energy[groupIndex] += sh_energy[groupIndex +  8]; }
//	/*	if ( BLOCK_SIZE >=  8 )*/ { sh_energy[groupIndex] += sh_energy[groupIndex +  4]; }
//	/*	if ( BLOCK_SIZE >=  4 )*/ { sh_energy[groupIndex] += sh_energy[groupIndex +  2]; }
//	/*	if ( BLOCK_SIZE >=  2 )*/ { sh_energy[groupIndex] += sh_energy[groupIndex +  1]; }
//	}


	if( groupIndex == 0 )
	{
		energyRWBuffer[groupID.x] = sh_energy[0];
	}

}

#endif // REDUCTION


#endif // COMPUTE

