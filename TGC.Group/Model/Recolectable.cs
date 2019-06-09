using System;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;



namespace TGC.Group.Model
{
    public class Recolectable
    {
        private TgcScene scene;

        public TGCMatrix Translation { get; set; }
        public TGCMatrix Scaling { get; set; }
        public TGCMatrix Rotation { get; set; }
        public TGCVector3 Position { get; set; }
        public TgcBoundingSphere Collider;
        public Effect effect;
        private Surface g_pDepthStencil;
        private Texture g_pRenderTarget;
        private Texture g_pRenderTarget4;
        private Texture g_pRenderTarget4Aux;
        private VertexBuffer g_pVBV3D;

        public Recolectable(String mediaDir, TGCVector3 PosicionInicial)
        {
            var d3dDevice = D3DDevice.Instance.Device;
            
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");
            
            string compilationErrors;
            effect = Effect.FromFile(d3dDevice, mediaDir + "GaussianBlur.fx", null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);

            effect.Technique = "DefaultTechnique";

            g_pDepthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, DepthFormat.D24S8, MultiSampleType.None, 0, true);

            g_pRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth, d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            g_pRenderTarget4 = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4, d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            g_pRenderTarget4Aux = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth / 4, d3dDevice.PresentationParameters.BackBufferHeight / 4, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            effect.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            effect.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);

            CustomVertex.PositionTextured[] vertices = {new CustomVertex.PositionTextured(-1, 1, 1, 0, 0), new CustomVertex.PositionTextured(1, 1, 1, 1, 0), new CustomVertex.PositionTextured(-1, -1, 1, 0, 1), new CustomVertex.PositionTextured(1, -1, 1, 1, 1)};
            
            g_pVBV3D = new VertexBuffer(typeof(CustomVertex.PositionTextured), 4, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionTextured.Format, Pool.Default);

            g_pVBV3D.SetData(vertices, 0, LockFlags.None);

            //cambios iniciales
            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
            }

            Rotation = TGCMatrix.Identity;
            Translation = TGCMatrix.Translation(new TGCVector3(0, 10, 0) + PosicionInicial); // el +10 es para que este sobre la pista
            Scaling = TGCMatrix.Scaling(TGCVector3.One * 0.2f);
            Position = PosicionInicial;

            //seteo colisionador esfera
            Collider = new TgcBoundingSphere(new TGCVector3(0, 10, 0) + PosicionInicial, 5f);

        }

        public void Update()
        {
            if (ColisionoConBeetle())
            {

            }
        }

        bool ColisionoConBeetle()
        {
            return false;
        }


        public void Render()
        {
            var device = D3DDevice.Instance.Device;
                
            effect.Technique = "DefaultTechnique";

            var pOldRT = device.GetRenderTarget(0);

            var pSurf = g_pRenderTarget.GetSurfaceLevel(0);

            device.SetRenderTarget(0, pSurf);

            var pOldDS = device.DepthStencilSurface;

            device.DepthStencilSurface = g_pDepthStencil;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            device.BeginScene();

            foreach (var Mesh in scene.Meshes)
            {
                Mesh.Transform = Scaling * Rotation * Translation;
                Mesh.Render();
            }

            device.EndScene();

            pSurf.Dispose();

            pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
            device.SetRenderTarget(0, pSurf);
            device.BeginScene();

            effect.Technique = "DownFilter4";
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.SetStreamSource(0, g_pVBV3D, 0);
            effect.SetValue("g_RenderTarget", g_pRenderTarget);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effect.Begin(FX.None);
            effect.BeginPass(0);
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.EndPass();
            effect.End();
            pSurf.Dispose();

            device.EndScene();

            device.DepthStencilSurface = pOldDS;
            for (var P = 0; P < 10; ++P)
            {
                pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
                device.SetRenderTarget(0, pSurf);
                
                device.BeginScene();

                effect.Technique = "GaussianBlurSeparable";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget4);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();
                pSurf.Dispose();

                device.EndScene();
                
                if (P < 9)
                {
                    pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                    device.SetRenderTarget(0, pSurf);
                    pSurf.Dispose();
                    device.BeginScene();
                }

                else
                    device.SetRenderTarget(0, pOldRT);
                
                effect.Technique = "GaussianBlurSeparable";
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.SetStreamSource(0, g_pVBV3D, 0);
                effect.SetValue("g_RenderTarget", g_pRenderTarget4Aux);

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                effect.Begin(FX.None);
                effect.BeginPass(1);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                effect.EndPass();
                effect.End();

                if (P < 9)
                    device.EndScene();

                else
                {
                    device.DepthStencilSurface = pOldDS;
                    device.SetRenderTarget(0, pOldRT);

                    device.BeginScene();

                    effect.Technique = "GaussianBlur";
                    device.VertexFormat = CustomVertex.PositionTextured.Format;
                    device.SetStreamSource(0, g_pVBV3D, 0);
                    effect.SetValue("g_RenderTarget", g_pRenderTarget);

                    device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                    effect.Begin(FX.None);
                    effect.BeginPass(0);
                    device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                    effect.EndPass();
                    effect.End();
                    device.EndScene();
                }
            }

            device.BeginScene();
            device.EndScene();
            device.Present();
            Collider.Render();
        }

    }
}
