// Shader input / output
struct VS_INPUT
{
    float3 Position     : POSITION;
	float2 Texcoord		: TEXCOORD;
};

struct VS_OUTPUT
{
	float4 Position		: SV_POSITION;
	float2 Texcoord		: TEXCOORD;
};

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	
	output.Texcoord = input.Texcoord;
	
	output.Position = float4( input.Position.xyz, 1.0f );

	return output;
}