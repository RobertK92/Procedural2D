#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

uniform float val;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 p = -1.0 + 2.0 * input.TextureCoordinates;	
	float len = length(p);
	float2 uv = input.TextureCoordinates + (p/len)*cos(len*32.0-val*8.0)*0.1;
	return tex2D(SpriteTextureSampler, uv) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};