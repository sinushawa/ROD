cbuffer vsc
{
	matrix World;
	matrix ViewProjection;
	float3 eyePos;
	float padding1;
	float3 LightPos;
	float padding2;
}

// Shader input / output
struct VS_INPUT
{
    float3 Position     : POSITION;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
	float3 Binormal		: BINORMAL;
	float3 Tangent		: TANGENT;
};

struct VS_OUTPUT
{
	float4 WorldPos		: WORLDPOS;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
	float3 Binormal		: BINORMAL;
	float3 Tangent		: TANGENT;
	float TessFactor	: VERTEXDISTANCEFACTOR;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	
	output.WorldPos = float4( input.Position, 1.0f );
	
	output.Normal = input.Normal;

	output.Tangent = input.Tangent;

	output.Binormal = input.Binormal;
	
	output.Texcoord = input.Texcoord;
	
	float4 worldPos = mul( float4( input.Position, 1.0f ), World );
	float cameraDistance = distance(eyePos, worldPos);
	float lightDistance = distance(LightPos, worldPos);
	
	const float maxDistance = 320.0f;
	
	// Need to do some clipping? No need to tesselate vertices that are behind the camera
	output.TessFactor = clamp(1.0f - (cameraDistance+lightDistance / maxDistance), 0.1f, 1.0f);
	//output.TessFactor = clamp(smoothstep(0.0f, maxDistance, (cameraDistance / maxDistance)), 0.1f, 1.0f);
	
	return output;
}