cbuffer vsc
{
	matrix World;
	matrix ViewProjection;
	float3 eyePos;
}

// Shader input / output
struct VS_INPUT
{
    float3 Position     : POSITION;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
};

struct VS_OUTPUT
{
	float4 Position		: SV_POSITION;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	
	output.Normal = mul(input.Normal, (float3x3)World);
	output.Normal = normalize(output.Normal);
	
	output.Texcoord = input.Texcoord;
	
	output.Position = mul( float4( input.Position.xyz, 1.0f ), World );
	output.Position = mul( output.Position, ViewProjection );

	return output;
}