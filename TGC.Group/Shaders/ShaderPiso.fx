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
float4x4 matViewAnt; //Matriz View anterior
float4x4 matProj; //Matriz Projection actual

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


/****************************************************************************************/
//Bloom
/****************************************************************************************/

//default
struct VS_INPUT_DEFAULT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};
struct VS_OUTPUT_DEFAULT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Norm : TEXCOORD1; // Normales
    float3 Pos : TEXCOORD2; // Posicion real 3d
    float3 LightVec : TEXCOORD3;
};
//Vertex Shader
VS_OUTPUT_DEFAULT vs_mainDefault(VS_INPUT_DEFAULT Input)
{
    VS_OUTPUT_DEFAULT Output;

	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);

	//Las Texcoord quedan igual
    Output.Texcoord = Input.Texcoord;

	// Calculo la posicion real
    float4 pos_real = mul(Input.Position, matWorld);
    Output.Pos = float3(pos_real.x, pos_real.y, pos_real.z);

	// Transformo la normal y la normalizo
	//Output.Norm = normalize(mul(Input.Normal,matInverseTransposeWorld));
    Output.Norm = normalize(mul(Input.Normal, matWorld));
    Output.LightVec = lightPos.xyz - mul(Input.Position, matWorld).xyz;
    return (Output);
}



//Pixel Shader
float4 ps_mainDefault(float3 Texcoord : TEXCOORD0, float3 N : TEXCOORD1,
	float3 Pos : TEXCOORD2, float3 LightVec : TEXCOORD3) : COLOR0
{

    float4 colorBase = tex2D(diffuseMap, Texcoord);

    float4 colorIluminacion = float4(1, 1, 1, 1);

    float4 ambientLight = 0.1 * colorIluminacion;
    
    // //R = 2 * N * (N dot L) - L

    float3 realViewPos = mul(viewPos.xyz, matWorld);
    float3 realLightVec = mul(LightVec.xyz, matWorld);

    float3 Norm = normalize(N);
    float3 L = normalize(realLightVec);
    float3 R = normalize(2 * Norm * dot(Norm, L) - L);
    float3 V = normalize(realViewPos - Pos.xyz);

    float4 specularLight = 0.3 * float4(1, 1, 1, 1) * max(pow(dot(R, V), 100), 0);


    float4 diffuseLight = 0.4 * colorIluminacion * dot(Norm, L);

    float4 light = diffuseLight + specularLight + ambientLight;

    return 0.4 * diffuseLight + colorBase; //el specular se ve muy mal, y el ambient la verdad que no le agregaba nada. Asi q lo deje solo con el diffuse
}

technique DefaultTechnique
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_mainDefault();
        PixelShader = compile ps_3_0 ps_mainDefault();
    }
}

///////////////////////////////////
//// Gaussian Blur
///////////////////////////////////


texture g_RenderTarget;
sampler RenderTarget =
sampler_state
{
    Texture = <g_RenderTarget>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};


texture g_GlowMap;
sampler GlowMap =
sampler_state
{
    Texture = <g_GlowMap>;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};


static const int kernel_r = 12;
static const int kernel_size = 13;
static const float Kernel[kernel_size] =
{
    0.002216, 0.008764, 0.026995, 0.064759, 0.120985, 0.176033, 0.199471, 0.176033, 0.120985, 0.064759, 0.026995, 0.008764, 0.002216,
};

void VSCopy(float4 vPos : POSITION, float2 vTex : TEXCOORD0, out float4 oPos : POSITION, out float2 oScreenPos : TEXCOORD0)
{
    oPos = vPos;
    oScreenPos = vTex;
    oPos.w = 1;
}


void Blur(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_size; ++i)
        for (int j = 0; j < kernel_size; ++j)
            Color += tex2D(RenderTarget, screen_pos + float2((float) (i - kernel_r) / screen_dx, (float) (j - kernel_r) / screen_dy)) * Kernel[i] * Kernel[j];
    Color.a = 1;
}

technique GaussianBlur
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 Blur();
    }
}



float4 PSDownFilter4(in float2 Tex : TEXCOORD0) : COLOR0
{
    float4 Color = 0;
    for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
            Color += tex2D(RenderTarget, Tex + float2((float) i / screen_dx , (float) j / screen_dy));

    return Color / 16;
}

technique DownFilter4
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSDownFilter4();
    }
}




float4 PSGrayScale(in float2 Tex : TEXCOORD0, in float2 vpos : VPOS) : COLOR0
{
    float4 ColorBase = tex2D(RenderTarget, Tex);
    float4 ColorBrillante = tex2D(GlowMap, Tex + float2((float) 16 / screen_dx, (float) 16 / screen_dy));
	// Y = 0.2126 R + 0.7152 G + 0.0722 B
    // float Yb = 0.2126 * ColorBase.r + 0.7152 * ColorBase.g + 0.0722 * ColorBase.b;
    // float Yk = 0.2126 * ColorBrillante.r + 0.7152 * ColorBrillante.g + 0.0722 * ColorBrillante.b;
    // if (round(vpos.y / 2) % 2 == 0)
    // {
    //     Yb *= 0.85;
    //     Yk *= 0.85;
    // }

    return ColorBase + ColorBrillante; //float4(Yk * 0.75, Yk * 0.1, Yb * 0.6 + Yk * 0.75, 1);
}

technique GrayScale
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 PSGrayScale();
    }

}

void BlurH(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_size; ++i)
        Color += tex2D(RenderTarget, screen_pos + float2((float) (i - kernel_r) / (screen_dx), 0)) * Kernel[i];
    Color.a = 1;
}

void BlurV(float2 screen_pos : TEXCOORD0, out float4 Color : COLOR)
{
    Color = 0;
    for (int i = 0; i < kernel_size; ++i)
        Color += tex2D(RenderTarget, screen_pos + float2(0, (float) (i - kernel_r) / screen_dy)) * Kernel[i];
        
    Color.a = 1;
}

technique GaussianBlurSeparable
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 BlurH();
    }

    pass Pass_1
    {
        VertexShader = compile vs_3_0 VSCopy();
        PixelShader = compile ps_3_0 BlurV();
    }
}



///////////////////////////////////
//// Velocity Map
///////////////////////////////////
struct VS_INPUT_VELOCITY
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};

struct VS_OUTPUT_VELOCITY
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Norm : TEXCOORD1; // Normales
    float4 vPosActual : TEXCOORD2; // Posicion actual
    float4 vPosAnterior : TEXCOORD3; // Posicion anterior
    //float2 Vel : TEXCOORD3; // velocidad por pixel

};

//Vertex Shader
VS_OUTPUT_VELOCITY vs_velocity(VS_INPUT_VELOCITY Input)
{
    VS_OUTPUT_VELOCITY Output;

	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);
   
	//Las Texcoord quedan igual
    Output.Texcoord = Input.Texcoord;

	// Transformo la normal y la normalizo
    Output.Norm = normalize(mul(Input.Normal, matWorld));

	// posicion actual
    Output.vPosActual = Output.Position;
	// posicion anterior
    Output.vPosAnterior = mul(Input.Position, matWorld * matViewAnt * matProj);

    return (Output);
   
}

//Pixel Shader Velocity
float4 ps_velocity(float3 Texcoord : TEXCOORD0, float4 vPosActual : TEXCOORD2, float4 vPosAnterior : TEXCOORD3) : COLOR0
{
	//Obtener el texel de textura
    float4 fvBaseColor = tex2D(diffuseMap, Texcoord);
    if (fvBaseColor.a < 0.1)
        discard;
       
    vPosActual /= vPosActual.w;
    vPosAnterior /= vPosAnterior.w;
    float2 Vel = vPosActual - vPosAnterior;

    return float4(Vel.x, Vel.y, 0.0f, 1.0f);
}

technique VelocityMap
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_velocity();
        PixelShader = compile ps_3_0 ps_velocity();
    }

}



///////////////////////////////////
//// Motion Blur
///////////////////////////////////

texture texVelocityMap;
sampler2D velocityMap = sampler_state
{
    Texture = (texVelocityMap);
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture texVelocityMapAnt;
sampler2D velocityMapAnt = sampler_state
{
    Texture = (texVelocityMapAnt);
    MinFilter = POINT;
    MagFilter = POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
float PixelBlurConst = 0.05f;
static const float NumberOfPostProcessSamples = 12.0f;
void vs_copy(float4 vPos : POSITION, float2 vTex : TEXCOORD0, out float4 oPos : POSITION, out float2 oScreenPos : TEXCOORD0)
{
    oPos = vPos;
    oScreenPos = vTex;
    oPos.w = 1;
}

float4 ps_motion_blur(in float2 Tex : TEXCOORD0) : COLOR0
{
    float4 curFramePixelVelocity = tex2D(velocityMap, Tex);
    float4 lastFramePixelVelocity = tex2D(velocityMapAnt, Tex);

    float2 pixelVelocity;
    float curVelocitySqMag = curFramePixelVelocity.r * curFramePixelVelocity.r +
                             curFramePixelVelocity.g * curFramePixelVelocity.g;
    float lastVelocitySqMag = lastFramePixelVelocity.r * lastFramePixelVelocity.r +
                              lastFramePixelVelocity.g * lastFramePixelVelocity.g;
                                   
    if (lastVelocitySqMag > curVelocitySqMag)
    {
        pixelVelocity.x = lastFramePixelVelocity.r * PixelBlurConst;
        pixelVelocity.y = -lastFramePixelVelocity.g * PixelBlurConst;
    }
    else
    {
        pixelVelocity.x = curFramePixelVelocity.r * PixelBlurConst;
        pixelVelocity.y = -curFramePixelVelocity.g * PixelBlurConst;
    }
	
	
    float3 Blurred = 0;
    for (float i = 0; i < NumberOfPostProcessSamples; i++)
    {
        float2 lookup = pixelVelocity * i / NumberOfPostProcessSamples + Tex;
        float4 Current = tex2D(RenderTarget, lookup);
        Blurred += Current.rgb;
    }

    return float4(Blurred / NumberOfPostProcessSamples, 1.0f);
}


technique PostProcessMotionBlur
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_copy();
        PixelShader = compile ps_3_0 ps_motion_blur();
    }
}