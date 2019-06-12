float4x4 matWorld;
float4x4 matWorldView;
float4x4 matWorldViewProj;
float4x4 matInverseTransposeWorld;

float4 PosicionCamara;
float4 FuenteDeLuz;
float time = 0;

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

VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

    Output.RealPos = Input.Position;

    Output.Position = mul(Input.Position, matWorldViewProj);

    Output.Texcoord = Input.Texcoord;

    Output.Normal = Input.Normal;

    Output.Color = Input.Color;

    Output.WorldNormal = mul(Input.Normal, matInverseTransposeWorld).xyz;

    Output.LightVec = FuenteDeLuz.xyz - mul(Input.Position, matWorld).xyz;
    
    return Output;
}

float4 ps_main(VS_OUTPUT Input) : COLOR0
{
    float4 ColorBase = float4(1, 1, 0, 1);

    float4 ColorIluminacion = float4(1, 1, 1, 1);

    float4 LuzAmbiente = 0.1 * ColorIluminacion;

    float3 PosicionRealCamara = mul(PosicionCamara.xyz, matWorld);

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

technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}

VS_OUTPUT vs_recolectado(VS_INPUT Input)
{
    VS_OUTPUT Output;

    Output.RealPos = float4(5 * sin(time) * Input.Position.x, 5 * sin(time) * Input.Position.y, 5 * sin(time) * Input.Position.z, 1);

    Output.Position = mul(Output.RealPos, matWorldViewProj);

    Output.Texcoord = Input.Texcoord;

    Output.Normal = Input.Normal;

    Output.Color = Input.Color;

    Output.WorldNormal = mul(Input.Normal, matInverseTransposeWorld).xyz;

    Output.LightVec = FuenteDeLuz.xyz - mul(Input.Position, matWorld).xyz;
    
    return Output;
}

float4 ps_recolectado(float3 LightVec : TEXCOORD2, float3 WorldNormal : TEXCOORD1, float4 RealPos : TEXCOORD3, float4 Color : COLOR0) : COLOR0
{
    if (abs(RealPos.y) > 2)
        discard;

    float4 ColorBase = float4(1, 1, 0, 1);

    float4 ColorIluminacion = float4(1, 1, 1, 1);

    float4 LuzAmbiente = 0.1 * ColorIluminacion;

    float3 PosicionRealCamara = mul(PosicionCamara.xyz, matWorld);

    float3 VectorRealLuz = mul(LightVec, matWorld);

    float3 N = normalize(WorldNormal);
    float3 L = normalize(VectorRealLuz);
    float3 R = normalize(2 * N * dot(N, L) - L);
    float3 V = normalize(PosicionRealCamara - RealPos.xyz);

    float4 LuzEspecular = 0.3 * float4(1, 1, 1, 1) * max(pow(dot(R, V), 100), 0) * length(Color);

    float4 LuzDifusa = 0.5 * ColorIluminacion * dot(N, L);

    float4 Luz = LuzDifusa + LuzEspecular + LuzAmbiente;

    return Luz + ColorBase;
}

technique Recolectado
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_recolectado();
        PixelShader = compile ps_3_0 ps_recolectado();
    }
}