
// Camera settings.
uniform const float4x4	World;
uniform const float4x4	View;
uniform const float4x4	Projection;
uniform const float		FarClip;

uniform const float		FogStart = 10;
uniform const float		FogEnd = 400;
uniform const float		FogEnabled = 0;
uniform const float		DesaturateEnabled = 0;

// This sample uses a simple Lambert lighting model.
uniform const float3	LightDirection = normalize(float3(-1, -1, -1));
uniform const float3	DiffuseLight = 1.25;
uniform const float3	AmbientLight = 0.25;
uniform const float3	Color = float3(1,1,1);

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinate : TEXCOORD0;
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

// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    // Apply the world and camera matrices to compute the output position.
    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // Compute lighting, using a simple Lambert model.
    float3 worldNormal = mul(input.Normal, instanceTransform);
    
    float diffuseAmount = max(-dot(worldNormal, LightDirection), 0);
    
    float3 lightingResult = saturate(diffuseAmount * DiffuseLight + AmbientLight);
    
    output.Color = float4(lightingResult, 1);

    // Copy across the input texture coordinate.
    output.TextureCoordinate.xy = input.TextureCoordinate.xy;
	output.TextureCoordinate.z = viewPosition.z;

    return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
                                                  float4x4 instanceTransform : BLENDWEIGHT)
{
    return VertexShaderCommon(input, mul(World, transpose(instanceTransform)));
}


// When instancing is disabled we take the world transform from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
    return VertexShaderCommon(input, World);
}


// Both techniques share this same pixel shader.
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input) 
{
	PixelShaderOutput output;

	output.Color = tex2D(Sampler, input.TextureCoordinate.xy) * input.Color;

	const float3 coef = {0.3, 0.59, 0.11};
	float amount = ComputeFogFactor(-input.TextureCoordinate.z);

	if(DesaturateEnabled > 0)
	{
		output.Color.rgb = lerp(output.Color.rgb * Color, dot(coef.rgb, output.Color.rgb), amount); //desaturate
	}

	if(FogEnabled > 0)
	{
		output.Color.a = lerp(output.Color.a, 0, amount); //fog
		output.Color.rgb *= output.Color.a;
	}

	output.Depth = float4(-input.TextureCoordinate.z / FarClip, 1.0f, 1.0f, 1.0f);

    return output;
}


// Hardware instancing technique.
technique HardwareInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}


// For rendering without instancing.
technique NoInstancing
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 NoInstancingVertexShader();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

			