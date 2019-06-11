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
        public string technique;

        private String particleTexturePath;
        private static String particleFileName = "Thumper\\Particles\\pisada.png";
        private float time;

        public Disparo(String mediaDir, TGCVector3 PosicionInicial, string Tecnica)
        {
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");

            time = 0;

            technique = Tecnica;

            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Effect = TGCShaders.Instance.LoadEffect(mediaDir + "ShaderDisparo.fx");
                mesh.Technique = technique;
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

        public void Render(float ElapsedTime, TGCVector3 PosicionCamara, TGCVector3 FuenteDeLuz)
        {
            foreach (var mesh in scene.Meshes)
            {
                mesh.Transform = Scaling * Rotation * Translation;

                time += ElapsedTime;

                mesh.Technique = technique;

                mesh.Effect.SetValue("PosicionCamara", new Microsoft.DirectX.Vector4(PosicionCamara.X, PosicionCamara.Y, PosicionCamara.Z, 0));
                mesh.Effect.SetValue("FuenteDeLuz", new Microsoft.DirectX.Vector4(FuenteDeLuz.X, FuenteDeLuz.Y, FuenteDeLuz.Z, 0));
                mesh.Effect.SetValue("Posicion", new Microsoft.DirectX.Vector4(mesh.Position.X, mesh.Position.Y, mesh.Position.Z, 0));
                mesh.Effect.SetValue("time", time);

                mesh.Render();
            }

            emitter.render(ElapsedTime);

            //Collider.Render();
        }

    }
}
