texture2D SourceTexture0;
sampler2D BloomSampler = sampler_state
{
    Texture = <SourceTexture0>;
	/**/
	MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
	
};

texture2D SourceTexture1;
sampler2D BaseSampler = sampler_state
{
    Texture = <SourceTexture1>;
	
	MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
	/**/
};

float2	g_vSourceDimensions;
float2	g_vDestinationDimensions;

float BloomIntensity;
float BaseIntensity;

float BloomSaturation;
float BaseSaturation;

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

// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}


float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	
    // Look up the bloom and original base image colors.
    float4 bloom = tex2D(BloomSampler, texCoord);
    float4 base = tex2D(BaseSampler, texCoord);

    
    // Adjust color saturation and intensity.
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    base *= (1 - saturate(bloom));
    
    // Combine the two images.
    return base + bloom;
}


technique BloomCombine
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
