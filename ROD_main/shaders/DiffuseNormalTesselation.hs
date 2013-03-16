struct VS_OUTPUT
{
	float4 WorldPos		: WORLDPOS;
	float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
	float3 Binormal		: BINORMAL;
	float3 Tangent		: TANGENT;
	float TessFactor	: VERTEXDISTANCEFACTOR;
};

struct HS_CONSTANT_DATA_OUTPUT
{
    float Edges[3]	: SV_TessFactor;
    float Inside	: SV_InsideTessFactor;
};

struct HS_CONTROL_POINT_OUTPUT
{
    float4 WorldPos		: WORLDPOS;
    float3 Normal		: NORMAL;
	float2 Texcoord		: TEXCOORD;
	float3 Binormal		: BINORMAL;
	float3 Tangent		: TANGENT;
};

HS_CONSTANT_DATA_OUTPUT ConstantsHS( InputPatch<VS_OUTPUT, 3> p, uint PatchID : SV_PrimitiveID )
{
    HS_CONSTANT_DATA_OUTPUT output = (HS_CONSTANT_DATA_OUTPUT)0;
    float4 edgeTessellationFactors;
    
    edgeTessellationFactors = float2(2,2).xxxy;
	
	float3 scaleFactor = float3(
		0.5f * (p[1].TessFactor + p[2].TessFactor),
		0.5f * (p[2].TessFactor + p[0].TessFactor),
		0.5f * (p[0].TessFactor + p[1].TessFactor)
	);
    // Scale edge factors 
    edgeTessellationFactors *= scaleFactor.xyzx;
	
    // Assign tessellation levels
    output.Edges[0] = abs(edgeTessellationFactors.x);
    output.Edges[1] = abs(edgeTessellationFactors.y);
    output.Edges[2] = abs(edgeTessellationFactors.z);
    output.Inside   = abs(edgeTessellationFactors.w);

    return output;
}

[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("ConstantsHS")]
[maxtessfactor(4.0)]
HS_CONTROL_POINT_OUTPUT HS( InputPatch<VS_OUTPUT, 3> inputPatch, 
                            uint uCPID : SV_OutputControlPointID )
{
    HS_CONTROL_POINT_OUTPUT    output = (HS_CONTROL_POINT_OUTPUT)0;
    
    output.WorldPos = inputPatch[uCPID].WorldPos;
    output.Normal =   inputPatch[uCPID].Normal;
	output.Tangent =   inputPatch[uCPID].Tangent;
	output.Binormal =   inputPatch[uCPID].Binormal;
	output.Texcoord =  inputPatch[uCPID].Texcoord;
    return output;
}