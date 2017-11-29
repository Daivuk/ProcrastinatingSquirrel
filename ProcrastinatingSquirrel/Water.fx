texture		SpriteTexture; sampler sDiffuse = sampler_state {texture = <SpriteTexture>;};
texture		SnowSplatter; sampler sSnowSplatter = sampler_state 
{
	texture = <SnowSplatter>;
	
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
texture		TexSens; sampler sSens = sampler_state 
{
	texture = <TexSens>;
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

float		InvSplatterSize;
float2		CurSplatterUV;
float		WaterAnim;


float4 PixelShaderFunction(	float4 color: COLOR0,
							float2 texCoord: TEXCOORD0) : COLOR0
{
    float4 sens = tex2D(sSens, float2(texCoord.x + WaterAnim, texCoord.y + WaterAnim * 2));
    float4 ground = tex2D(sDiffuse, float2(texCoord.x, texCoord.y + sens.r * .035));
    
    float2 localSplatterUV = (texCoord - color.ba) * 4 * InvSplatterSize;
    float2 globalSplatterUV = color.rg * .25 + CurSplatterUV;
    float4 splatter = tex2D(sSnowSplatter, globalSplatterUV + localSplatterUV);

    ground.a = (clamp(.5, 1, splatter.g) - .5) * 2;

    return ground;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
