float2	g_vSourceDimensions;
float2	g_vDestinationDimensions;

texture2D SourceTexture0;
sampler2D PointSampler0 = sampler_state
{
    Texture = <SourceTexture0>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LinearSampler0 = sampler_state
{
    Texture = <SourceTexture0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

texture2D SourceTexture1;
sampler2D PointSampler1 = sampler_state
{
    Texture = <SourceTexture1>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LinearSampler1 = sampler_state
{
    Texture = <SourceTexture1>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

texture2D SourceTexture2;
sampler2D PointSampler2 = sampler_state
{
    Texture = <SourceTexture2>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LinearSampler2 = sampler_state
{
    Texture = <SourceTexture2>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

// Reconstruct position from a linear depth buffer
float3 PositionFromDepth(sampler2D DepthSampler, float2 vTexCoord, float3 vFrustumRay)
{	
	float fPixelDepth = tex2D(DepthSampler, vTexCoord).x;
	return fPixelDepth * vFrustumRay;
}

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





// Downscales to 1/16 size, using 16 samples
float4 DownscalePS (	in float2 in_vTexCoord			: TEXCOORD0,
						uniform bool bDecodeLuminance	)	: COLOR0
{
	float4 vColor = 0;
	for (int x = 0; x < 4; x++)
	{
		for (int y = 0; y < 4; y++)
		{
			float4 vSample = tex2D(PointSampler0, in_vTexCoord);
			vColor += vSample;
		}
	}

	vColor /= 16.0f;
	
		
	if (bDecodeLuminance)
		vColor = float4(exp(vColor.r), 1.0f, 1.0f, 1.0f);
	
	return vColor;
}

// Upscales or downscales using hardware bilinear filtering
float4 HWScalePS (	in float2 in_vTexCoord			: TEXCOORD0	)	: COLOR0
{
	return tex2D(LinearSampler0, in_vTexCoord);
}


technique Downscale4
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 DownscalePS(false);
    }
}

technique Downscale4Luminance
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 DownscalePS(true);
    }
}

technique ScaleHW
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 HWScalePS();
    }
}