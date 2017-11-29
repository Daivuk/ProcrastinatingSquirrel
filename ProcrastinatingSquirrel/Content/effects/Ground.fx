texture		SpriteTexture; sampler sDiffuse = sampler_state {texture = <SpriteTexture>;};
texture		SnowSplatter; sampler sSnowSplatter = sampler_state 
{
	texture = <SnowSplatter>;
	
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
texture		SnowTexture; sampler sSnowTexture = sampler_state {texture = <SnowTexture>;};
float		InvSplatterSize;
float2		CurSplatterUV;
float		snowAmount;

/*
float2 Viewport;

void SpriteVertexShader(inout float4 color    : COLOR0,

                       inout float2 texCoord : TEXCOORD0,

                       inout float4 position : POSITION0)

{

   // Half pixel offset for correct texel centering.

   position.xy -= 0.5;

   // Viewport adjustment.

   position.xy = position.xy / Viewport;

   position.xy *= float2(2, -2);

   position.xy -= float2(1, -1);

}

*/


float4 PixelShaderFunction(	float4 color: COLOR0,
							float2 texCoord: TEXCOORD0) : COLOR0
{
    float4 ground = tex2D(sDiffuse, texCoord);
    float4 snow = tex2D(sSnowTexture, texCoord);
    
    float2 localSplatterUV = (texCoord - color.ba) * 4 * InvSplatterSize;
    float2 globalSplatterUV = color.rg * .25 + CurSplatterUV;
    float4 splatter = tex2D(sSnowSplatter, globalSplatterUV + localSplatterUV);
    
    float snowPercent = splatter.r * snowAmount;
    
    float4 result = ground;// = lerp(ground, snow, snowPercent);
    
    if (snowPercent > snow.a) result = 
		float4(lerp(ground.rgb, snow.rgb, clamp(0, .25, snowPercent - snow.a) * 4), 1);

    return result;
}


technique Technique1
{
    pass Pass1
    {
     //   VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
