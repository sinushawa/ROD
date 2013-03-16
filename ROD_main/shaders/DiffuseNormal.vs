cbuffer perObject
{
	matrix World;
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
	float4 Position		: SV_POSITION;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
	float3 Binormal		: BINORMAL;
	float3 Tangent		: TANGENT;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	
	output.Normal = mul(input.Normal, (float3x3)World);
	output.Normal = normalize(output.Normal);

	output.Tangent = mul(input.Tangent, (float3x3)World);
	output.Tangent = normalize(output.Tangent);

	output.Binormal = mul(input.Binormal, (float3x3)World);
	output.Binormal = normalize(output.Binormal);
	
	output.Texcoord = input.Texcoord;
	
	output.Position = mul( float4( input.Position.xyz, 1.0f ), World );
	
	return output;
}