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
	uniform float4 BoneDQ[106];
}


// Shader input / output
struct VS_INPUT
{
    float3 Position       : POSITION;
	float3 Normal		  : NORMAL;
	float2 Texcoord		  : TEXCOORD;
	float3 Binormal		  : BINORMAL;
	float3 Tangent		  : TANGENT;
	uint4 BoneIndices     : BLENDINDICES;
	float4 Boneweights    : BLENDWEIGHT;

};

struct VS_OUTPUT
{
	float4 Position		: SV_POSITION;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
	float3 Binormal		: BINORMAL;
	float3 Tangent		: TANGENT;
};
float3 transformPositionDQ( float3 position, float4 realDQ, float4 dualDQ )
{
    return position + 2 * cross( realDQ.xyz, cross(realDQ.xyz, position) + realDQ.w*position ) +  2 * (realDQ.w * dualDQ.xyz - dualDQ.w * realDQ.xyz + cross( realDQ.xyz, dualDQ.xyz));
}
float3 transformNormalDQ( float3 normal, float4 realDQ, float4 dualDQ )
{
    return normal + 2.0 * cross( realDQ.xyz, cross( realDQ.xyz, normal ) + realDQ.w * normal );
}

VS_OUTPUT VS(VS_INPUT input)
{
	VS_OUTPUT output;
	
	float2x4 BoneDQC[53] = (float2x4[53])BoneDQ;

	float2x4 boneDQ = (float2x4)0;
    float2x4 m = BoneDQC[input.BoneIndices[0]];
    float4 dq0 = (float1x4)m ;     
    
    boneDQ = input.Boneweights[0] * m ;

    m = BoneDQC[input.BoneIndices[1]];
    float4 dq = (float1x4)m ;   
    if (dot( dq0, dq ) < 0)        
      boneDQ -= input.Boneweights[1] * m;
    else 
    boneDQ += input.Boneweights[1] * m;        

        
    m = BoneDQC[input.BoneIndices[2]];
    dq = (float1x4)m ;          
    if (dot( dq0, dq ) < 0)        
      boneDQ -= input.Boneweights[2] * m;
    else 
    boneDQ += input.Boneweights[2] * m ;
            
            
    m = BoneDQC[input.BoneIndices[3]];
    dq = (float1x4)m ;              
    if (dot( dq0, dq ) < 0)        
      boneDQ -= input.Boneweights[3] * m;
    else                
    boneDQ += input.Boneweights[3] * m;

	float length = sqrt(boneDQ[0].w * boneDQ[0].w + boneDQ[0].x * boneDQ[0].x + boneDQ[0].y * boneDQ[0].y + boneDQ[0].z * boneDQ[0].z) ;
	boneDQ = boneDQ / length ;
	float3 position = transformPositionDQ( input.Position.xyz,  boneDQ[0], boneDQ[1] );
	float3 normal = transformNormalDQ( input.Normal, boneDQ[0], boneDQ[1] );

	output.Tangent = input.Tangent;

	output.Binormal = input.Binormal;
	
	output.Texcoord = input.Texcoord;
	
	output.Normal = mul(normal, (float3x3)World);
	output.Normal = normalize(output.Normal);

	output.Tangent = mul(input.Tangent, (float3x3)World);
	output.Tangent = normalize(output.Tangent);

	output.Binormal = mul(input.Binormal, (float3x3)World);
	output.Binormal = normalize(output.Binormal);
	
	output.Texcoord = input.Texcoord;
	
	output.Position = mul( float4( position.xyz, 1.0f ), World );
	output.Position = mul( output.Position, ViewProjection );
	return output;
}