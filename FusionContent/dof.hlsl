
#if 0
$ubershader	COC_TO_ALPHA|DEPTH_OF_FIELD
#endif

cbuffer GlobalConstants : register(b0) {
	float    LinDepthScale;
	float    LinDepthBias;
	float    CocScale;
	float    CocBias;
};


float GetLinearDepth(float z)
{
	return 1.0f / (z * LinDepthScale + LinDepthBias);
}


#ifdef COC_TO_ALPHA

Texture2D Depth : register(t0);


float4 VSMain(uint VertexID : SV_VertexID) : SV_POSITION
{
	return float4((VertexID == 0) ? 3.0f : -1.0f, (VertexID == 2) ? 3.0f : -1.0f, 1.0f, 1.0f);
}


float4 PSMain(float4 position : SV_POSITION) : SV_Target
{
	float depth = GetLinearDepth(Depth.Load(int3(position.xy, 0)).x);

	return float4(0.0f, 0.0f, 0.0f, CocScale / depth + CocBias);
}

#endif



#ifdef DEPTH_OF_FIELD

SamplerState	SamplerLinearClamp : register(s0);
Texture2D Source : register(t0);

struct PS_IN {
    float4 position : SV_POSITION;
  	float4 uv : TEXCOORD0;
};


PS_IN VSMain(uint VertexID : SV_VertexID)
{
	PS_IN output;
	output.position.x = (VertexID == 0) ? 3.0f : -1.0f;
	output.position.y = (VertexID == 2) ? 3.0f : -1.0f;
	output.position.zw = 1.0f;

	float texWidth, texHeight;
	Source.GetDimensions(texWidth, texHeight);

	output.uv.xy = output.position.xy * float2(0.5f, -0.5f) + 0.5f;
	output.uv.z = 1.0f / texWidth;
	output.uv.w = 1.0f / texHeight;

	return output;
}


float4 PSMain(PS_IN input) : SV_Target
{
	const int tapCount = 141;
	float2 poisson[141] = {
		1.797,    -0.352,  
		1.871,    -0.130,  
		1.875,     0.104,  
		1.810,     0.328,  
		1.693,     0.532,  
		1.575,     0.736,  
		1.458,     0.939,  
		1.340,     1.143,  
		1.223,     1.346,  
		1.078,     1.530,  
		0.883,     1.659,  
		1.556,     0.324,  
	   -1.611,     0.052,  
		1.378,    -0.555,  
		1.338,     0.507,  
		1.338,     0.170,  
		1.338,    -0.167,  
		1.613,     0.003,  
	   -0.963,     1.616,  
	   -1.142,     1.465,  
	   -1.268,     1.268,  
	   -1.386,     1.064,  
	   -1.503,     0.861,  
	   -1.621,     0.657,  
	   -1.738,     0.454,  
	   -1.844,     0.245,  
	   -1.881,     0.013,  
	   -1.852,    -0.219,  
	   -1.751,    -0.430,  
	   -1.634,    -0.634,  
	   -1.517,    -0.837,  
	   -1.399,    -1.041,  
	   -1.282,    -1.244,  
	   -1.159,    -1.445,  
	   -0.985,    -1.601,  
	   -1.137,    -0.671,  
	   -1.217,     0.003,  
	   -1.137,     0.339,  
	   -1.137,     0.676,  
	   -1.137,     1.013,  
	   -1.137,    -0.334,  
	   -1.129,    -0.999,  
	   -1.422,     0.279,  
		1.058,    -1.547,  
		1.209,    -1.369,  
		1.327,    -1.166,  
		1.444,    -0.962,  
		1.562,    -0.759,  
		1.679,    -0.555,  
		1.797,    -0.352,  
		0.840,     1.306,  
		0.794,     0.854,  
		0.789,     0.506,  
		0.789,     0.170,  
		0.789,    -0.167,  
		0.789,    -0.504,  
		0.789,    -0.840,  
		0.789,    -1.177,  
		0.789,    -1.514,  
		1.064,    -0.334,  
		1.064,     1.013,  
		1.121,     0.727,  
		1.064,     0.339,  
		1.064,     0.002,  
		1.098,    -1.028,  
		1.109,    -0.698,  
		0.238,     0.170,  
		0.238,     1.180,  
		0.238,     0.843,  
		0.238,     0.506,  
		0.238,    -1.177,  
		0.238,    -0.167,  
		0.238,    -0.504,  
		0.238,    -0.840,  
		0.238,     1.517,  
		0.238,    -1.514,  
		0.513,    -0.334,  
		0.541,     1.045,  
		0.513,     0.676,  
		0.513,     0.339,  
		0.513,     0.002,  
		0.513,    -1.008,  
		0.513,    -0.671,  
		0.513,    -1.344,  
		0.548,     1.371,  
	   -0.312,     0.170,  
	   -0.312,     1.180,  
	   -0.312,     0.843,  
	   -0.312,     0.506,  
	   -0.312,    -1.177,  
	   -0.312,    -0.167,  
	   -0.312,    -0.504,  
	   -0.312,    -0.840,  
	   -0.312,     1.517,  
	   -0.312,    -1.514,  
	   -0.037,    -0.334,  
	   -0.037,     1.013,  
	   -0.037,     0.676,  
	   -0.037,     0.339,  
	   -0.037,     0.002,  
	   -0.037,    -1.008,  
	   -0.037,    -0.671,  
	   -0.037,    -1.344,  
	   -0.037,     1.349,  
	   -0.863,     0.170,  
	   -0.896,     1.229,  
	   -0.863,     0.843,  
	   -0.863,     0.506,  
	   -0.863,    -1.177,  
	   -0.863,    -0.167,  
	   -0.863,    -0.504,  
	   -0.863,    -0.840,  
	   -0.827,    -1.425,  
	   -0.588,    -0.334,  
	   -0.588,     1.013,  
	   -0.588,     0.676,  
	   -0.588,     0.339,  
	   -0.588,     0.002,  
	   -0.588,    -1.008,  
	   -0.588,    -0.671,  
	   -0.588,    -1.344,  
	   -0.593,     1.411,  
	   -1.412,    -0.504,  
	   -1.388,     0.541,  
	   -1.480,    -0.230,  
		1.549,    -0.296,  
		0.633,    -1.731,  
		0.398,    -1.732,  
		0.163,    -1.732,  
	   -0.072,    -1.732,  
	   -0.307,    -1.732,  
	   -0.542,    -1.732,  
	   -0.774,    -1.702,  
	   -0.748,     1.710,  
	   -0.515,     1.732,  
	   -0.280,     1.732,  
	   -0.045,     1.732,  
		0.190,     1.732,  
		0.425,     1.732,  
		0.659,     1.728,  
		0.859,    -1.670,  
	};

	float dofMinThreshold = 0.05f;
	float centerCoc = clamp(Source.Load(int3(input.position.xy, 0)).a, -1, 1);
	float2 discRadius = 30.0f * min(0,centerCoc) * input.uv.zw;
	// float2 discRadiusLow = discRadius * 4.0f * 0.8f;

	float4 outColor = 0.0f;
	
	/*if (centerCoc<0) {
		return abs(centerCoc) * float4(0,0,1,1);
	} else {
		return centerCoc * float4(1,0,0,1);
	}//*/

	[fastopt]
	for(int t = 0; t < tapCount; ++t) { 
		float4 tapHigh = Source.SampleLevel(SamplerLinearClamp, input.uv.xy + poisson[t] * discRadius, 0);

		float4 tap = tapHigh; 

		tap.a = saturate(-tap.a) + 0.0001;  

		outColor.rgb += tap.a * tap.rgb;
		outColor.a += tap.a;
	}
	return outColor / outColor.a;
}

#endif