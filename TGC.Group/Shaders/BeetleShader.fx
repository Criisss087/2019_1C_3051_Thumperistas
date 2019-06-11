/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

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

//Parametros de la Luz
float3 ambientColor; //Color RGB para Ambient de la luz
float3 diffuseColor; //Color RGB para Ambient de la luz
float3 specularColor; //Color RGB para Ambient de la luz
float specularExp; //Exponente de specular
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float time = 0;

/**************************************************************************************/
/* DIFFUSE_MAP */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 LightVec : TEXCOORD2;
    float3 HalfAngleVec : TEXCOORD3;
};

//Vertex Shader
VS_OUTPUT_DIFFUSE_MAP vs_DiffuseMap(VS_INPUT_DIFFUSE_MAP input)
{
    VS_OUTPUT_DIFFUSE_MAP output;

	//Proyectar posicion
    output.Position = mul(input.Position, matWorldViewProj);

	//Enviar Texcoord directamente
    output.Texcoord = input.Texcoord;

	/* Pasar normal a World-Space
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
    float3 worldPosition = mul(input.Position, matWorld);
    output.LightVec = lightPosition.xyz - worldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara.
    float3 viewVector = eyePosition.xyz - worldPosition;

	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
    output.HalfAngleVec = viewVector + output.LightVec;

    return output;
}

//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
    float2 Texcoord : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float3 LightVec : TEXCOORD2;
    float3 HalfAngleVec : TEXCOORD3;
};

//Pixel Shader
float4 ps_DiffuseMap(PS_DIFFUSE_MAP input) : COLOR0
{
	//Normalizar vectores
    float3 Nn = normalize(input.WorldNormal);
    float3 Ln = normalize(input.LightVec);
    float3 Hn = normalize(input.HalfAngleVec);

	//Obtener texel de la textura
    float4 texelColor = tex2D(diffuseMap, input.Texcoord);
    
	//Componente Diffuse: N dot L
    float3 n_dot_l = dot(Nn, Ln);
    float3 diffuseLight = diffuseColor * max(0.0, n_dot_l); //Controlamos que no de negativo


	//Componente Specular: (N dot H)^exp
    float3 n_dot_h = dot(Nn, Hn);
    float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: specularColor * pow(max(0.0, n_dot_h), specularExp);

	//Color final: modular (Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
    float4 finalColor = float4(saturate(ambientColor + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);

    return finalColor;
}

float4 ps_SinEscudo(PS_DIFFUSE_MAP input) : COLOR0
{
	//Obtener texel de la textura
    float4 texelColor = tex2D(diffuseMap, input.Texcoord);   

    return texelColor;
}

/*
* Technique DIFFUSE_MAP
*/
technique ConEscudoMetalico
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_DiffuseMap();
        PixelShader = compile ps_3_0 ps_DiffuseMap();
    }
}

technique SinEscudo
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_DiffuseMap();
        PixelShader = compile ps_3_0 ps_SinEscudo();
    }
}

struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
    float4 RealPos : TEXCOORD1;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float4 RealPos : TEXCOORD3;
    float3 Normal : NORMAL0;
    float3 WorldNormal : TEXCOORD1;
    float3 LightVec : TEXCOORD2;
    float4 Color : COLOR0;
};

VS_OUTPUT vs_explosivo(VS_INPUT Input)
{
    VS_OUTPUT Output;

    Output.RealPos = float4(5 * sin(time) * Input.Position.x, 5 * sin(time) * Input.Position.y, 5 * sin(time) * Input.Position.z, 1);

    Output.Position = mul(Output.RealPos, matWorldViewProj);

    Output.Texcoord = Input.Texcoord;

    Output.Normal = Input.Normal;

    Output.Color = Input.Color;

    Output.WorldNormal = mul(Input.Normal, matInverseTransposeWorld).xyz;

    Output.LightVec = lightPosition.xyz - mul(Input.Position, matWorld).xyz;

    return Output;
}

float4 ps_explosivo(VS_OUTPUT Input) : COLOR0
{
    float4 ColorBase = float4(1, 0, 0, 1);

    float4 ColorIluminacion = float4(1, 1, 1, 1);

    float4 LuzAmbiente = 0.1 * ColorIluminacion;

    float3 PosicionRealCamara = mul(eyePosition.xyz, matWorld);

    float3 VectorRealLuz = mul(Input.LightVec.xyz, matWorld);

    float3 N = normalize(Input.WorldNormal);
    float3 L = normalize(VectorRealLuz);
    float3 R = normalize(2 * N * dot(N, L) - L);
    float3 V = normalize(PosicionRealCamara - Input.RealPos.xyz);

    float4 LuzEspecular = 0.3 * float4(1, 1, 1, 1) * max(pow(dot(R, V), 100), 0) * length(Input.Color);

    float4 LuzDifusa = 0.5 * ColorIluminacion * dot(N, L);

    float4 Luz = LuzDifusa + LuzEspecular + LuzAmbiente;

    return Luz + ColorBase;
}

technique Explosiva
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_explosivo();
        PixelShader = compile ps_3_0 ps_explosivo();
    }
}