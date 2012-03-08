//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------

uniform const float	  TextureEnabled;
uniform const texture Texture;
uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
	MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

//-----------------------------------------------------------------------------
// Matrices
//-----------------------------------------------------------------------------

uniform const float4x4	World		: register(vs, c0);	
uniform const float4x4	View		: register(vs, c4);	
uniform const float4x4	Projection	: register(vs, c8);


//-----------------------------------------------------------------------------
// Vertex
//-----------------------------------------------------------------------------

struct VertexShaderInput
{
	float3	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float4	Color		: COLOR0;
};

struct VertexShaderOutput
{
	float4	PositionPS	: POSITION;	
	float4	Diffuse		: COLOR0;
	float2	TexCoord	: TEXCOORD0;
};

//-----------------------------------------------------------------------------
// Pixel
//-----------------------------------------------------------------------------

struct PixelShaderInput
{
	float4	Diffuse		: COLOR0;
	float2	TexCoord	: TEXCOORD0;
};

//-----------------------------------------------------------------------------
// Vertex shaders
//-----------------------------------------------------------------------------

VertexShaderOutput vs_main(VertexShaderInput vin)
{
	VertexShaderOutput vout;
	
	float4 position = float4(vin.Position,1);
	float4 pos_ws = mul(position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);

	vout.PositionPS	= pos_ps;
	vout.Diffuse	= vin.Color;
	//vout.Diffuse.xyz = vout.Diffuse.xyz * vout.Diffuse.w;
	vout.TexCoord	= vin.TexCoord.xy;

	return vout;
}

//-----------------------------------------------------------------------------
// Pixel shaders
//-----------------------------------------------------------------------------

float4 ps_main(PixelShaderInput pin) : COLOR
{
	float4 color = float4(1,1,1,1);
	if(TextureEnabled > 0)
	{
		color = tex2D(TextureSampler, pin.TexCoord.xy);
		clip(color.a - 0.001); // clip if alpha is 0

		//	clip(color.a - 1); // if the alpha is less than 1 then do not draw this pixel

	}

	color.rgb = ((1 - pin.Diffuse.a) * color.rgb) + (color.rgb * pin.Diffuse.rgb * pin.Diffuse.a);
	color *= pin.Diffuse.a;

	return color;
}



//-----------------------------------------------------------------------------
// Shader and technique definitions
//-----------------------------------------------------------------------------

Technique BasicEffect
{
	Pass
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader	 = compile ps_2_0 ps_main();
	}
}
