cbuffer vsc
{
	matrix World;
	matrix ViewProjection;
	float3 eyePos;
	float padding1;
	float3 LightPos;
	float padding2;
}
cbuffer vsb
{
	uniform float2x4 BoneDQ[54];
}

// Shader input / output
struct VS_INPUT
{
    float3 Position       : POSITION;
	float3 Normal		  : NORMAL;
	float2 Texcoord		  : TEXCOORD;
	float3 Binormal		  : BINORMAL;
	float3 Tangent		  : TANGENT;
	uint4 BoneIndices    : BLENDINDICES;
	float4 Boneweights    : BLENDWEIGHT;

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
	
	float2x4 boneDQ = input.Boneweights[0]*BoneDQ[input.BoneIndices[0]];
	boneDQ += input.Boneweights[1]*BoneDQ[input.BoneIndices[1]];
	boneDQ += input.Boneweights[2]*BoneDQ[input.BoneIndices[2]];
	boneDQ += input.Boneweights[3]*BoneDQ[input.BoneIndices[3]];

	float len = length(boneDQ[0]);
	boneDQ /= len;

	float3 position = input.Position.xyz + 2.0*cross(boneDQ[0].yzw, cross(boneDQ[0].yzw, input.Position.xyz) + boneDQ[0].x*input.Position.xyz);
	float3 trans = 2.0*(boneDQ[0].x*boneDQ[1].yzw - boneDQ[1].x*boneDQ[0].yzw + cross(boneDQ[0].yzw, boneDQ[1].yzw));
	position += trans;

	output.WorldPos = float4( position, 1.0f );
	
	float3 normal = input.Normal + 2.0*cross(boneDQ[0].yzw, cross(boneDQ[0].yzw, input.Normal) + boneDQ[0].x*input.Normal);

	output.Normal = normal;

	output.Tangent = input.Tangent;

	output.Binormal = input.Binormal;
	
	output.Texcoord = input.Texcoord;
	
	float4 worldPos = mul( float4( position, 1.0f ), World );
	float cameraDistance = distance(eyePos, worldPos);
	float lightDistance = distance(LightPos, worldPos);
	
	const float maxDistance = 320.0f;
	
	// Need to do some clipping? No need to tesselate vertices that are behind the camera
	output.TessFactor = clamp(1.0f - (cameraDistance+lightDistance / maxDistance), 0.1f, 1.0f);
	//output.TessFactor = clamp(smoothstep(0.0f, maxDistance, (cameraDistance / maxDistance)), 0.1f, 1.0f);
	
	return output;
}