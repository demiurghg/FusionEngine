
/*-----------------------------------------------------------------------------
	General stuff :
-----------------------------------------------------------------------------*/

float sqr(float a) {
	return a * a;
}


float	LinearFalloff( float dist, float max_range )
{
	float fade = 0;
	fade = saturate(1 - (dist / max_range));
	fade *= fade;
	return saturate(fade);
}


float3	Lambert( float3 normal, float3 light_dir, float3 intensity, float3 diff_color, float bias = 0 )
{
	light_dir	=	normalize(light_dir);
	return intensity * diff_color * max( 0, dot(light_dir, normal) + bias ) / (1+bias);
}


/*-----------------------------------------------------------------------------
	Fresnel :
-----------------------------------------------------------------------------*/

float3	Fresnel( float c, float3 Fn )
{
	//	values below 2% are not physically possible.
	//	such values are used as shadow or holes.
	return Fn + ( saturate(50.0 * Fn) - Fn ) * pow(2, (-5.55473 * c - 6.98316)*c );
}

/*-----------------------------------------------------------------------------
	Geometry attenuation term :
-----------------------------------------------------------------------------*/

float GTerm ( float roughness, float3 N, float3 V, float3 H, float3 L ) 
{
	float a = sqr(roughness);
	
	float	g1	=	2 * dot(N,H) * dot(N,V) / (dot(V,H) + a );
	float	g2	=	2 * dot(N,H) * dot(N,L) / (dot(V,H) + a );
	float 	G	=	min(1, min(g1, g2));

	// additional attenuation due to roughness:
	return G * (1-roughness*0.5); 
}

float GTerm ( float roughness, float3 N, float3 V ) 
{
	float a = sqr(roughness)*0;
	float	g1	=	2 * dot(N,V) / ( dot(V,N) + a );
	float 	G	=	saturate(min(1, g1));

	// additional attenuation due to roughness:
	return G * (1-roughness*0.5);
}

/*-----------------------------------------------------------------------------
	Specular lighting :

	https://www.marmoset.co/toolbag/learn/pbr-theory	
	"This means that in theory conductors will not show any evidence of diffuse light. 
	In practice however there are often oxides or other residues on the surface of a 
	metal that will scatter some small amounts of light."

	http://blog.selfshadow.com/publications/s2013-shading-course/hoffman/s2013_pbs_physics_math_slides.pdf
-----------------------------------------------------------------------------*/

float3	CookTorrance( float3 N, float3 V, float3 L, float3 I, float3 F, float roughness )
{
			L	=	normalize(L);
			V	=	normalize(V);
	float3	H	=	normalize(V+L);
	
	//	Increase slightly low roughness value to 
	//	avoid point light flickering on shiny surfaces.
	//roughness = roughness;
	
	float m = roughness * roughness;

	//	NB: To remove harsh edge on grazing angles :
	float	edgeDecay	=	saturate(dot(L,N)*10);

	float	G		=	GTerm( roughness, N,V,H,L );
	
	float	cos_a	=	dot(N,H);
	float	sin_a	=	sqrt(abs(1 - cos_a * cos_a)); // 'abs' to avoid negative values
	
	float	D	=	exp( -(sin_a*sin_a) / (cos_a*cos_a) / (m*m) ) / (PI * m*m * cos_a * cos_a * cos_a * cos_a );

	F	=	Fresnel( dot(V,H), F );
						  
	return max(0, I * F * D * G / (4*abs(dot(N,L))*dot(V,N))) * edgeDecay;
}

