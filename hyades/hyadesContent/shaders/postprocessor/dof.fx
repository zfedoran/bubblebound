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





float time;

float g_fFarClip;
float g_fFocalDistance;
float g_fFocalWidth;
float g_fAttenuation;

static const int NUM_DOF_TAPS = 12;
static const float MAX_COC = 10.0f;

float2 g_vFilterTaps[NUM_DOF_TAPS];

float GetBlurFactor(in float fDepthVS)
{
	return smoothstep(0, g_fFocalWidth, abs(g_fFocalDistance - (fDepthVS * g_fFarClip)));
}

float4 DOFDiscPS(in float2 in_vTexCoord		: TEXCOORD0) : COLOR
{
	// Start with center sample color
	float4 vColorSum = tex2D(PointSampler0, in_vTexCoord);
	float fTotalContribution = 1.0f;

	// Depth and blurriness values for center sample
	float fCenterDepth = tex2D(PointSampler1, in_vTexCoord).x;
	float fCenterBlur = GetBlurFactor(fCenterDepth);

	if (fCenterBlur > 0)
	{
		// Compute CoC size based on blurriness
		float fSizeCoC = fCenterBlur * MAX_COC + fCenterBlur*(sin(in_vTexCoord.x*64+time*4)+sin(in_vTexCoord.x*16-time))*10;

		// Run through all filter taps
		for (int i = 0; i < NUM_DOF_TAPS; i++)
		{
			// Compute sample coordinates
			float2 vTapCoord = in_vTexCoord + g_vFilterTaps[i] * fSizeCoC;

			// Fetch filter tap sample
			float4 vTapColor = tex2D(LinearSampler0, vTapCoord);
			float fTapDepth = tex2D(PointSampler1, vTapCoord).x;
			float fTapBlur = GetBlurFactor(fTapDepth);

			// Compute tap contribution based on depth and blurriness
			float fTapContribution = (fTapDepth > fCenterDepth) ? 1.0f : fTapBlur;

			// Accumulate color and sample contribution
			vColorSum += vTapColor * fTapContribution;
			fTotalContribution += fTapContribution;
		}
	}

	// Normalize color sum
	float4 vFinalColor = vColorSum / fTotalContribution;
	return vFinalColor;
}

float4 DOFBlurBufferPS (	in float2 in_vTexCoord			: TEXCOORD0	)	: COLOR0 
{
	float4 vOriginalColor = tex2D(PointSampler0, in_vTexCoord);
	float4 vBlurredColor = tex2D(LinearSampler1, in_vTexCoord);
	float fDepthVS = tex2D(PointSampler2, in_vTexCoord).x;

	float fBlurFactor = GetBlurFactor(fDepthVS);
	
    return lerp(vOriginalColor, vBlurredColor, saturate(fBlurFactor) * g_fAttenuation);
}

technique DOFDiscBlur
{
	pass p0
    {
        VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 DOFDiscPS();
    }
}

technique DOFBlurBuffer
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 DOFBlurBufferPS();
    }
}