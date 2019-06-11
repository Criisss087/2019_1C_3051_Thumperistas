//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

// float4 viewPos;
float4 center;
float time;

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



struct VS_INPUT
{
    float4 Position : POSITION0;
    
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;       
    float4 Color : COLOR0;
    float4 realPos : POSITION1;
};


VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

   	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);
    Output.realPos = mul (Input.Position,matWorld);
	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

	//Propago el color x vertice
    Output.Color = Input.Color; 

    return (Output);
}


VS_OUTPUT vs_estiramiento(VS_INPUT Input)
{
    VS_OUTPUT Output;

   	//Proyectar posicion
    float factorSin = abs(sin(time*2)*2) + 1.5;
    float factorCos = abs(sin(time*2)*2) + 1.5;
    Input.Position = float4(Input.Position.x*factorSin, Input.Position.y*factorCos,Input.Position.zw);
    Output.Position = mul(Input.Position, matWorldViewProj);
    Output.realPos = mul (Input.Position,matWorld);
   
	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

	//Propago el color x vertice
    Output.Color = Input.Color; 

    return (Output);
}


float4 ps_main(VS_OUTPUT Input) : COLOR0
{
    return Input.Color;
}

float4 ps_cambio_colores(VS_OUTPUT Input):COLOR0{
    

    return float4(cos(time),sin(time),sin(time * 100000),1);
}


technique Deformacion1{
    pass Pass_0{
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_cambio_colores();
    }
}

technique Deformacion2{
    pass Pass_0{
        VertexShader = compile vs_3_0 vs_estiramiento();
        PixelShader = compile ps_3_0 ps_main();
    }
}

technique Deformacion3{
    pass Pass_0{
        VertexShader= compile vs_3_0 vs_estiramiento();
        PixelShader = compile ps_3_0 ps_cambio_colores();
    }
}