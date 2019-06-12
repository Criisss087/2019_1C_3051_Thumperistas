using System;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;

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
        public float time;
        public string technique;

        public Recolectable(String mediaDir, TGCVector3 PosicionInicial)
        {
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");

            time = 0;

            technique = "RenderScene";

            //cambios iniciales
            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Effect = TGCShaders.Instance.LoadEffect(mediaDir + "ShaderRecolectable.fx");
                mesh.Technique = technique;
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


        public void Render(TGCVector3 PosicionCamara, TGCVector3 FuenteDeLuz, float ElapsedTime)
        {
            time += ElapsedTime;

            foreach (var mesh in scene.Meshes)
            {
                mesh.Transform = Scaling * Rotation * Translation;

                mesh.Technique = technique;

                mesh.Effect.SetValue("PosicionCamara", new Microsoft.DirectX.Vector4(PosicionCamara.X, PosicionCamara.Y, PosicionCamara.Z, 0));
                mesh.Effect.SetValue("FuenteDeLuz", new Microsoft.DirectX.Vector4(FuenteDeLuz.X, FuenteDeLuz.Y, FuenteDeLuz.Z, 0));
                mesh.Effect.SetValue("time", time);

                mesh.Render();
            }
            //Collider.Render();
        }

    }
}
