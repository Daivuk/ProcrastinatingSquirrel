texture		SpriteTexture; sampler sDiffuse = sampler_state {texture = <SpriteTexture>;};
texture		TexSens; sampler sSens = sampler_state 
{
	texture = <TexSens>;
	MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};
float2 squirrelPosition;
float2 topLeftPosition;
float2 sensTopLeftPosition;
float2 bottomRightPosition;
float sensRadius;
float sensOffset;


float4 PixelShaderFunction(	float2 texCoord: TEXCOORD0) : COLOR0
{
    float4 sens = tex2D(sDiffuse, texCoord + float2(sensOffset, 0) + sensTopLeftPosition);
	float2 position = lerp(topLeftPosition, bottomRightPosition, texCoord);
	float percent = 1 - clamp(0, 1, distance(squirrelPosition, position) / sensRadius);
	float sensPercent = 1 - clamp(0, .5, percent) * 2;
	float visiblePercent = clamp(0, .25f, percent) * 4;
	float2 diffuseOffset = float2((sens.r - .5f) * .025f, 0) * sensPercent;
    float4 diffuse = tex2D(sSens, texCoord + diffuseOffset);
	
	float sensLinePercent = 0;
	if (percent < .125)
	{
		sensLinePercent = 1;
	}
	else if (percent >= .125 && percent < .2)
	{
		sensLinePercent = 1 - (percent - .125) / .075;
	}
		
	diffuse.a *= visiblePercent * (1 - sensPercent);
	diffuse = lerp(diffuse, float4(0, 0, 0, .4), sensLinePercent);

//	diffuse = diffuse + float4(sens.r * sensPercent, 0, 0, 1);

    return diffuse;
}


technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
