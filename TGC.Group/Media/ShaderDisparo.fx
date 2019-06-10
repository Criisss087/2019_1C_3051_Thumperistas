// //Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};


struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float2 RealPos : TEXCOORD1;
    float4 Color : COLOR0;
};

VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

    Output.RealPos = Input.Position;

	//Proyectar posicion
    Output.Position=mul(Input.Position,matWorldViewProj);
	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

    //Input.Color.rgb = Input.Position.xyz;
	//Propago el color x vertice
    Output.Color = Input.Color;
    
    return Output;
}

float4 ps_main(VS_OUTPUT Input) : COLOR0
{
    return Input.Texcoord.y > Input.Texcoord.x - 1.1f && Input.Texcoord.y < Input.Texcoord.x - 0.85f ? float4(0, 0, 1, 1) : float4(0, 1, 0, 1);
}



technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}