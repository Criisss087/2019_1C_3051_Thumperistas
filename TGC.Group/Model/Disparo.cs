using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.Direct3D;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Particle;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;

namespace TGC.Group.Model
{
    public class Disparo
    {
        private TgcScene scene;
        private ParticleEmitter emitter;
        public TGCMatrix Translation { get; set; }
        public TGCMatrix Scaling { get; set; }
        public TGCMatrix Rotation { get; set; }
        public TGCVector3 Position { get; set; }
        public TgcBoundingSphere Collider;
        public static float speed = 1800f;

        private String particleTexturePath;
        private static String particleFileName = "Thumper\\Particles\\pisada.png";

        public Disparo(String mediaDir, TGCVector3 PosicionInicial)
        {
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");

            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Effect = TGCShaders.Instance.LoadEffect(mediaDir + "\\TgcMeshSpotLightShader.fx");
                mesh.Technique = TGCShaders.Instance.GetTGCMeshTechnique(mesh.RenderType);
            }

            Position = PosicionInicial;
            Rotation = TGCMatrix.Identity;
            Translation = TGCMatrix.Translation(new TGCVector3(0, 10, 0) + PosicionInicial); 
            Scaling = TGCMatrix.Scaling(TGCVector3.One * 0.2f);

            //seteo colisionador esfera
            Collider = new TgcBoundingSphere(new TGCVector3(0, 10, 0) + PosicionInicial, 5f);

            particleTexturePath = mediaDir + particleFileName;
            emitter = new ParticleEmitter(particleTexturePath, 10);
            emitter.Position = PosicionInicial;

            emitter.MinSizeParticle = 0.5f;
            emitter.MaxSizeParticle = 2f;
            emitter.ParticleTimeToLive = 0.5f;
            emitter.CreationFrecuency = 0.25f;
            emitter.Dispersion = 400;

        }

        public void Update()
        {

        }

        public TGCVector3 Avanza(float ElapsedTime, float posX, float posY)
        {
            Position += new TGCVector3(posX, posY, speed * ElapsedTime);
            
            Translation = TGCMatrix.Translation(Position);
            //Rotation = TGCMatrix.RotationY(distAng);
			
            Collider.moveCenter(new TGCVector3(posX, posY, speed * ElapsedTime));
            emitter.Position += new TGCVector3(posX, posY, speed * ElapsedTime);
            
            return Position;
        }

        public void Render(float ElapsedTime, TGCVector3 PosicionCamara)
        {
            foreach (var mesh in scene.Meshes)
            {
                mesh.Transform = Scaling * Rotation * Translation;

                mesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(new TGCVector3(mesh.Position.X + 10, mesh.Position.Y, mesh.Position.Z - 100)));
                mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(PosicionCamara));
                mesh.Effect.SetValue("spotLightDir", TGCVector3.Vector3ToFloat4Array(new TGCVector3(0, 0, 1)));
                mesh.Effect.SetValue("lightIntensity", 1000);
                mesh.Effect.SetValue("lightAttenuation", 15);
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Green));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.Green));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.Green));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.Green));
                mesh.Effect.SetValue("materialSpecularExp", 300f);

                mesh.Render();
            }

            emitter.render(ElapsedTime);

            //Collider.Render();
        }

    }
}
