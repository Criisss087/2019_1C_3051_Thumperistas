using System;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
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

        public Recolectable(String mediaDir, TGCVector3 PosicionInicial)
        {
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");

            //cambios iniciales
            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Effect = TGCShaders.Instance.LoadEffect(mediaDir + "\\TgcMeshSpotLightShader.fx");
                mesh.Technique = TGCShaders.Instance.GetTGCMeshTechnique(mesh.RenderType);
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


        public void Render(TGCVector3 PosicionCamara)
        {
            foreach (var mesh in scene.Meshes)
            {
                mesh.Transform = Scaling * Rotation * Translation;

                mesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(new TGCVector3(mesh.Position.X, mesh.Position.Y, mesh.Position.Z - 100)));
                mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(PosicionCamara));
                mesh.Effect.SetValue("spotLightDir", TGCVector3.Vector3ToFloat4Array(new TGCVector3(0, 0, 1)));
                mesh.Effect.SetValue("lightIntensity", 1000);
                mesh.Effect.SetValue("lightAttenuation", 15);
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.Yellow));
                mesh.Effect.SetValue("materialSpecularExp", 300f);

                mesh.Render();
            }
            //Collider.Render();
        }

    }
}
