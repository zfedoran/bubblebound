texture2D SourceTexture0;
sampler2D TextureSampler = sampler_state
{
    Texture = <SourceTexture0>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float2	g_vSourceDimensions;
float2	g_vDestinationDimensions;
float BloomThreshold;

void PostProcessVS (	in float3 in_vPositionOS				: POSITION,
						in float2 in_vTexCoordAndCornerIndex	: TEXCOORD0,					
						out float4 out_vPositionCS				: POSITION,
						out float2 out_vTexCoord				: TEXCOORD0)
{
	// Offset the position by half a pixel to correctly align texels to pixels
	out_vPositionCS.x = in_vPositionOS.x - (1.0f / g_vDestinationDimensions.x);
	out_vPositionCS.y = in_vPositionOS.y + (1.0f / g_vDestinationDimensions.y);
	out_vPositionCS.z = in_vPositionOS.z;
	out_vPositionCS.w = 1.0f;
	
	// Pass along the texture coordinate 
	out_vTexCoord = in_vTexCoordAndCornerIndex.xy;
}	


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original image color.
    float4 c = tex2D(TextureSampler, texCoord);

    // Adjust it to keep only values brighter than the specified threshold.
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}


technique BloomExtract
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
