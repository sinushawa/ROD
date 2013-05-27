cbuffer vsc
{
	matrix World;
	matrix ViewProjection;
	float3 eyePos;
	float padding1;
	float3 LightPos;
	float padding2;
	uniform float2x4 BoneDQ[100];
}

// Shader input / output
struct VS_INPUT
{
    float3 Position       : POSITION;
	float3 Normal		  : NORMAL;
	float2 Texcoord		  : TEXCOORD;
	float3 Binormal		  : BINORMAL;
	float3 Tangent		  : TANGENT;
	uint BoneIndices[4]   : BLENDINDICES;
	float Boneweights[4]  : BLENDWEIGHT;

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
	
	float2x4 blendDQ = input.Boneweights[0]*boneDQ[input.BoneIndices[0]];
	blendDQ += input.Boneweights[1]*boneDQ[input.BoneIndices[1]];
	blendDQ += input.Boneweights[2]*boneDQ[input.BoneIndices[2]];
	blendDQ += input.Boneweights[3]*boneDQ[input.BoneIndices[3]];

	float len = length(blendDQ[0]);
	blendDQ /= len;

	float3 position = input.Position.xyz + 2.0*cross(blendDQ[0].yzw, cross(blendDQ[0].yzw, input.Position.xyz) + blendDQ[0].x*input.Position.xyz);
	float3 trans = 2.0*(blendDQ[0].x*blendDQ[1].yzw - blendDQ[1].x*blendDQ[0].yzw + cross(blendDQ[0].yzw, blendDQ[1].yzw));
	position += trans;

	output.WorldPos = float4( position, 1.0f );
	
	float3 normal = input.Normal + 2.0*cross(blendDQ[0].yzw, cross(blendDQ[0].yzw, input.Normal) + blendDQ[0].x*input.Normal);

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