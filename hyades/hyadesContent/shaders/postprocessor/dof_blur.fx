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













float g_fSigma = 0.5f;

float CalcGaussianWeight(int iSamplePoint)
{
	float g = 1.0f / sqrt(2.0f * 3.14159 * g_fSigma * g_fSigma);  
	return (g * exp(-(iSamplePoint * iSamplePoint) / (2 * g_fSigma * g_fSigma)));
}

float4 GaussianBlurH_PS (	in float2 in_vTexCoord			: TEXCOORD0,
						uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;

    for (int i = -iRadius; i < iRadius; i++)
    {   
		float fWeight = CalcGaussianWeight(i);
		vTexCoord.x = in_vTexCoord.x + (i / g_vSourceDimensions.x);
		float4 vSample = tex2D(PointSampler0, vTexCoord);
		vColor += vSample * fWeight;
    }
	
	return vColor;
}

float4 GaussianBlurV_PS (	in float2 in_vTexCoord			: TEXCOORD0,
						uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;

    for (int i = -iRadius; i < iRadius; i++)
    {   
		float fWeight = CalcGaussianWeight(i);
		vTexCoord.y = in_vTexCoord.y + (i / g_vSourceDimensions.y);
		float4 vSample = tex2D(PointSampler0, vTexCoord);
		vColor += vSample * fWeight;
    }

    return vColor;
}

float4 GaussianDepthBlurH_PS (	in float2 in_vTexCoord			: TEXCOORD0,
							uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;
	float4 vCenterColor = tex2D(PointSampler0, in_vTexCoord);
	float fCenterDepth = tex2D(PointSampler1, in_vTexCoord).x; 

	int i;

    for (i = -iRadius; i < 0; i++)
    {   
		vTexCoord.x = in_vTexCoord.x + (i / g_vSourceDimensions.x);
		float fDepth = tex2D(PointSampler1, vTexCoord).x;
		float fWeight = CalcGaussianWeight(i);
    
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
    
    for (i = 1; i < iRadius; i++)
    {   
		vTexCoord.x = in_vTexCoord.x + (i / g_vSourceDimensions.x);
		float fDepth = tex2D(PointSampler1, vTexCoord).x;
		float fWeight = CalcGaussianWeight(i);
    
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
    
    vColor += vCenterColor * CalcGaussianWeight(0);
	
	return vColor;
}

float4 GaussianDepthBlurV_PS(	in float2 in_vTexCoord			: TEXCOORD0,
							uniform int iRadius		)	: COLOR0
{
    float4 vColor = 0;
	float2 vTexCoord = in_vTexCoord;
	float4 vCenterColor = tex2D(PointSampler0, in_vTexCoord);
	float fCenterDepth = tex2D(PointSampler1, in_vTexCoord).x; 

	int i;

    for (i = -iRadius; i < 0; i++)
    {   
		vTexCoord.y = in_vTexCoord.y + (i / g_vSourceDimensions.y);
		float fDepth = tex2D(PointSampler1, vTexCoord).x;
		float fWeight = CalcGaussianWeight(i);
		
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
    
    for (i = 1; i < iRadius; i++)
    {   
		vTexCoord.y = in_vTexCoord.y + (i / g_vSourceDimensions.y);
		float fDepth = tex2D(PointSampler1, vTexCoord).x;
		float fWeight = CalcGaussianWeight(i);
    
		if (fDepth >= fCenterDepth)
		{
			float4 vSample = tex2D(PointSampler0, vTexCoord);
			vColor += vSample * fWeight;
		}
		else
			vColor +=  vCenterColor * fWeight;
    }
	
	vColor += vCenterColor * CalcGaussianWeight(0);
	
	return vColor;
}

technique GaussianBlurH
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 GaussianBlurH_PS(6);
    }
}

technique GaussianBlurV
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 GaussianBlurV_PS(6);
    }
}

technique GaussianDepthBlurH
{
    pass p0
    {
        VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 GaussianDepthBlurH_PS(6);
    }
}

technique GaussianDepthBlurV
{
    pass p0
    {
        VertexShader = compile vs_3_0 PostProcessVS();
        PixelShader = compile ps_3_0 GaussianDepthBlurV_PS(6);
    }
}


