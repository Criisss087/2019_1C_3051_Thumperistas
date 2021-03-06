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

float4 viewPos;
float4 lightPos;

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
    float3 WorldNormal : TEXCOORD2;
    float3 LightVec : TEXCOORD1;
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

    Output.WorldNormal = mul(Input.Normal, matInverseTransposeWorld).xyz;

    Output.LightVec = lightPos.xyz -  mul(Input.Position, matWorld).xyz;
    
    return (Output);
}

//Pixel Shader
float4 ps_main(VS_OUTPUT Input) : COLOR0
{  
    float4 colorBase = tex2D(diffuseMap,Input.Texcoord);

    float4 colorIluminacion = float4(1,0,1,1);

    float4 ambientLight = 0.5 * colorIluminacion; 
    
    // //R = 2 * N * (N dot L) - L

    float3 realViewPos =mul(viewPos.xyz,matWorld);
    float3 realLightVec =mul(Input.LightVec.xyz,matWorld);

    float3 N =normalize( Input.WorldNormal);
    float3 L =normalize(realLightVec);
    float3 R = normalize(2*N*dot(N,L)-L);
    float3 V =normalize( realViewPos - Input.RealPos.xyz);

    float4 specularLight = 0.3 * float4(1,1,1,1) * max(pow(dot(R,V), 100),0)*length(Input.Color);


    float4 diffuseLight = 0.8 * colorIluminacion * dot(N,L);

    float4 light = diffuseLight+specularLight+ambientLight;

    return colorBase+specularLight+diffuseLight;
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
