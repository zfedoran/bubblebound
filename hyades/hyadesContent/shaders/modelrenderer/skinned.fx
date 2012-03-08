//-----------------------------------------------------------------------------
// SkinnedModel.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Maximum number of bone matrices we can render using shader 2.0 in a single pass.
// If you change this, update SkinnedModelProcessor.cs to match.
#define MaxBones 59


// Input parameters.
uniform const float4x4 View;
uniform const float4x4 Projection;
uniform const float4x4 Bones[MaxBones];

uniform const float FarClip;
	  
uniform const float FogStart = 10;
uniform const float FogEnd = 400;
uniform const float FogEnabled = 0;
uniform const float DesaturateEnabled = 0;


uniform const float3 Light1Direction = normalize(float3(1, 1, -2));
uniform const float3 Light1Color = float3(0.9, 0.8, 0.7);

uniform const float3 Light2Direction = normalize(float3(-1, -1, 1));
uniform const float3 Light2Color = float3(0.6, 0.6, 0.8);

uniform const float3 AmbientColor = 0.2;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};


// Vertex shader input structure.
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};


// Vertex shader output structure.
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 Lighting : COLOR0;
    float3 TexCoord : TEXCOORD0;
};


// Pixel shader input structure.
struct PixelShaderInput
{
    float3 Lighting : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
	float4 Depth : COLOR1;
};

float ComputeFogFactor(float d)
{
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) ;
}

// Vertex shader program.
VertexShaderOutput SkinnedVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
    
    // Skin the vertex position.
    float4 position = mul(input.Position, skinTransform);
    float4 position_vs = mul(position, View);
    output.Position = mul(position_vs, Projection);

    // Skin the vertex normal, then compute lighting.
    float3 normal = normalize(mul(input.Normal, skinTransform));
    
    float3 light1 = max(dot(normal, Light1Direction), 0) * Light1Color;
    float3 light2 = max(dot(normal, Light2Direction), 0) * Light2Color;

    output.Lighting = light1 + light2 + AmbientColor;

    output.TexCoord.xy = input.TexCoord.xy;
	output.TexCoord.z = position_vs.z;
    
    return output;
}



// Pixel shader program.
PixelShaderOutput SkinnedPixelShader(PixelShaderInput input)
{
	PixelShaderOutput output;

    output.Color = tex2D(Sampler, input.TexCoord.xy);
    output.Color.rgb *= input.Lighting;

	const float3 coef = {0.3, 0.59, 0.11};
	float amount = ComputeFogFactor(-input.TexCoord.z);

	if(DesaturateEnabled > 0)
	{
		output.Color.rgb = lerp(output.Color.rgb, dot(coef.rgb, output.Color.rgb), amount); //desaturate
	}

	if(FogEnabled > 0)
	{
		output.Color.a = lerp(output.Color.a, 0, amount); //fog
		output.Color.rgb *= output.Color.a;
	}

    output.Depth = float4(-input.TexCoord.z / FarClip, 1.0f, 1.0f, 1.0f);

    return output;
}


technique SkinnedModelTechnique
{
    pass SkinnedModelPass
    {
        VertexShader = compile vs_2_0 SkinnedVertexShader();
        PixelShader = compile ps_2_0 SkinnedPixelShader();
    }
}
