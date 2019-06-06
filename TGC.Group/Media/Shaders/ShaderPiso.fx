// ---------------------------------------------------------
// Ejemplo shader Minimo:
// ---------------------------------------------------------

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float screen_dx = 1024;
float screen_dy = 768;

//Textura para DiffuseMap
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

//float time = 0;

/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 RealPos : POSITION1;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float4 RealPos : POSITION1;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal:NORMAL0;
};

VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);

    Output.RealPos = mul(Input.Position,matWorld);
	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

    Output.Normal = Input.Normal;

    //Input.Color.rgb = Input.Position.xyz;
	//Propago el color x vertice
    Output.Color = Input.Color;
    
    return (Output);
}

float frecuencia = 10;

float4 viewPos;
float4 lightPos;
//Pixel Shader




float4 ps_main(VS_OUTPUT Input) : COLOR0
{  
    float4 colorBase = tex2D(diffuseMap,Input.Texcoord);
    
    float4 ambientLight = float4(0.2,1,0,1); 
    
    // //R = 2 * N * (N dot L) - L

    float3 realViewPos =mul(viewPos.xyz,matWorld);
    float3 realLightPos =mul(lightPos.xyz,matWorld);

    float3 N =normalize( Input.Normal);
    float3 L = normalize(realLightPos.xyz - Input.RealPos);
    float3 R = normalize(2*N*dot(N,L)-L);
    float3 V =normalize( realViewPos.xyz - Input.RealPos);

    float4 specularLight = 0.8 * float4(1,1,1,1) * pow(dot(R,V), 1);


    float4 diffuseLight = 0.5 * float4(0.5,0.5,0.5,1) * dot(N,L);

    float4 light = specularLight;
    return float4(,1);
}

// ------------------------------------------------------------------
technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}
