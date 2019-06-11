using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.BoundingVolumes;
using System.Drawing;
using System.Collections.Generic;
using TGC.Core.Collision;
using TGC.Core.Shaders;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Input;
using TGC.Core.Particle;
using System;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Group.Model
{
    public class Beetle
    {

        public const float VELOCIDAD_ANGULAR = 15f;
        public const float VELOCIDAD = 900f;
        public const float VELOCIDADX = 200f;

        private static float[] ambientColor = new float[] { 10, 10, 10 };
        private static float[] diffuseColor = new float[] { 60, 60, 60 };
        private static float[] specularColor = new float[] { 200, 200, 200 };

        private String MediaDir;
        private String ShadersDir;
        private String particleTexturePath;
        private static String particleFileName = "Thumper\\Particles\\humo.png";

        private TgcSceneLoader loader;
        public TgcScene beetle { get; set; }
        public float speed { get; set; }
        public bool escudo { get; set; } = true;
        public bool slide { get; set; } = false;
        public bool derecha { get; set; } = false;
        public bool izquierda { get; set; } = false;
        public bool godMode { get; set; } = false;
        public bool inmunidadTemp { get; set; } = false;
        public TgcBoundingOrientedBox colliderPista { get; }
        public TgcBoundingOrientedBox colliderRecolectablesOk;
        public TgcBoundingOrientedBox colliderRecolectablesWrong;
        private ParticleEmitter emitter;
        private Effect effect;

        public TGCMatrix translation { get; set; }
        public TGCMatrix scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TGCVector3 position { get; set; }
        public float traslacionZ { get; set; }
        public bool poderActivado = false;
        public float distAng = FastMath.PI_HALF;
        private float time;

        public enum TipoColision
        {
            Nada = 0,
            Colision = 1,
            Error = 2
        }

        public Beetle(string _mediaDir, string _shadersDir)
        {
            MediaDir = _mediaDir;
            ShadersDir = _shadersDir;

            loader = new TgcSceneLoader();

            beetle = loader.loadSceneFromFile(MediaDir + "/Thumper/beetle-TgcScene.xml");

            //Modifico como quiero que empiece el mesh
            position = new TGCVector3(0, 8f, 0);
            translation = TGCMatrix.Translation(position);
            scaling = TGCMatrix.Scaling(TGCVector3.One * .5f);

            // Shader Test
            effect = TGCShaders.Instance.LoadEffect(ShadersDir + "BeetleShader.fx");

            foreach (var mesh in beetle.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Transform = scaling * rotation * translation;
                mesh.BoundingBox.transform(scaling * rotation * translation);

                mesh.Effect = effect;
                mesh.Technique = "ConEscudoMetalico";
            }

            //Seteo collider Ok
            beetle.BoundingBox.transform(translation * scaling * rotation);
            //colliderRecolectablesOk = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

            // Escalo el bb para un nuevo collider en la pista, para no salirme de la pista en X
            var newScaling = scaling;
            var newTrasnlsation = TGCMatrix.Translation(new TGCVector3(-40, 10, 60));
            newScaling.Scale(2, 1, 0.5f);
            beetle.BoundingBox.transform(newScaling * newTrasnlsation);
            colliderPista = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

            //Collider para fallos
            newTrasnlsation = TGCMatrix.Translation(new TGCVector3(-40, 10, -30));
            beetle.BoundingBox.transform(newScaling * newTrasnlsation);
            colliderRecolectablesWrong = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

            newTrasnlsation = TGCMatrix.Translation(new TGCVector3(-40, 10, 16));
            newScaling.Scale(2, 1, 1f);
            beetle.BoundingBox.transform(newScaling * newTrasnlsation);
            colliderRecolectablesOk = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

            // Emisor de particulas:
            particleTexturePath = _mediaDir + particleFileName;
            emitter = new ParticleEmitter(particleTexturePath, 30);
            emitter.Position = position;

            emitter.MinSizeParticle = 4f;
            emitter.MaxSizeParticle = 6f;
            emitter.ParticleTimeToLive = 2f;
            emitter.CreationFrecuency = 0.1f;
            emitter.Dispersion = 50;


            this.speed = 900f;

        }

        public void Update(TgcD3dInput Input, float ElapsedTime)
        {
            if (Input.keyDown(Key.Space))
                slide = true;
            else
                slide = false;

            // Cambie esto para desacelerar y acelerar, para pruebas

            if (Input.keyPressed(Key.A))
            {
                AumentarVelocidad();
            }
            if (Input.keyPressed(Key.S))
            {
                DesvanecerVelocidad(ElapsedTime);
            }


            // Capturador de Giro
            if (Input.keyDown(Key.LeftArrow))
            {
                if (distAng > Geometry.DegreeToRadian(45))
                    distAng -= Beetle.VELOCIDAD_ANGULAR * ElapsedTime;

                izquierda = true;
                //Ver como activar arrastrado
            }
            else
            {
                if (distAng < FastMath.PI_HALF)
                    distAng += Beetle.VELOCIDAD_ANGULAR * ElapsedTime;

                izquierda = false;
            }

            if (Input.keyDown(Key.RightArrow))
            {
                if (distAng < Geometry.DegreeToRadian(120))
                    distAng += Beetle.VELOCIDAD_ANGULAR * ElapsedTime;

                derecha = true;
                //Ver como activar arrastrado
            }
            else
            {
                if (distAng > FastMath.PI_HALF)
                    distAng -= Beetle.VELOCIDAD_ANGULAR * ElapsedTime;

                derecha = false;
            }

            // Activar/Desactivar God Mode
            if (Input.keyPressed(Key.G))
                godMode = !godMode;


            // para debug
            if (Input.keyPressed(Key.E))
                GanarEscudo();


        }

        public bool Sliding()
        {
            return slide || izquierda || derecha;
        }

        public bool Inmunidad()
        {
            return godMode || inmunidadTemp;
        }

        public TGCVector3 Avanza(float ElapsedTime, float posX, float posY)
        {
            position += new TGCVector3(posX, posY, speed * ElapsedTime);

            translation = TGCMatrix.Translation(position);
            rotation = TGCMatrix.RotationY(distAng);

            colliderPista.move(new TGCVector3(posX, posY, speed * ElapsedTime));
            colliderRecolectablesOk.move(new TGCVector3(posX, posY, speed * ElapsedTime));
            colliderRecolectablesWrong.move(new TGCVector3(posX, posY, speed * ElapsedTime));

            emitter.Position += new TGCVector3(posX, posY, speed * ElapsedTime);

            return position;
        }

        public TipoColision ColisionandoConRecolectable(List<Recolectable> recolectables, ref Recolectable objetoColisionado)
        {
            foreach (Recolectable ObjRecoleactable in recolectables)
            {
                if (TgcCollisionUtils.testSphereOBB(ObjRecoleactable.Collider, colliderRecolectablesOk))
                {
                    objetoColisionado = ObjRecoleactable;
                    return TipoColision.Colision;
                }
                if (TgcCollisionUtils.testSphereOBB(ObjRecoleactable.Collider, colliderRecolectablesWrong))
                {
                    if (godMode)
                    {
                        objetoColisionado = ObjRecoleactable;
                        return TipoColision.Colision;
                    }
                    else
                    {
                        return TipoColision.Error;
                    }

                }
            }
            return TipoColision.Nada;
        }

        public TipoColision ColisionandoConObstaculo(List<Obstaculo> obstaculos, ref Obstaculo objetoColisionado)
        {
            foreach (Obstaculo obs in obstaculos)
            {
                if (TgcCollisionUtils.testObbObb(obs.Collider, colliderRecolectablesOk))
                {
                    objetoColisionado = obs;
                    return TipoColision.Colision;
                }
            }
            return TipoColision.Nada;
        }

        private TGCVector3 getLigthPos(float ElapsedTime)
        {
            TGCVector3 ligthPos = new TGCVector3(0, 0, 0);

            float x = FastMath.Abs(FastMath.Cos(time * 100) * 200);
            float y = 600f + FastMath.Cos(time * 100) * 50;

            ligthPos = position + new TGCVector3(x, y, 0);

            return ligthPos;
        }

        public void Render(float ElapsedTime, int AcumuladorPoder, TGCVector3 PosicionCamara)
        {
            time += ElapsedTime;

            foreach (var mesh in beetle.Meshes)
            {
                mesh.Transform = scaling * rotation * translation;
                mesh.BoundingBox.transform(scaling * rotation * translation);

                TGCVector3 ligthPos = getLigthPos(ElapsedTime);
                TGCVector3 eyePosition = PosicionCamara + new TGCVector3(0, 600, -100);

                mesh.Effect.SetValue("lightPosition", TGCVector3.Vector3ToFloat4Array(ligthPos));
                mesh.Effect.SetValue("eyePosition", TGCVector3.Vector3ToFloat4Array(eyePosition));
                mesh.Effect.SetValue("ambientColor", ambientColor);
                mesh.Effect.SetValue("diffuseColor", diffuseColor);
                mesh.Effect.SetValue("specularColor", specularColor);
                mesh.Effect.SetValue("specularExp", 300f);

            }
            beetle.RenderAll();

            //Render Colliders para debug
            colliderPista.setRenderColor(Color.Blue);
            //colliderPista.Render();

            colliderRecolectablesOk.setRenderColor(Color.Yellow);
            //colliderRecolectablesOk.Render();

            colliderRecolectablesWrong.setRenderColor(Color.Red);
            //colliderRecolectablesWrong.Render();

            if (AcumuladorPoder > 10)
                emitter.render(ElapsedTime);
        }

        public void AumentarVelocidad()
        {
            speed += 50f;
        }

        public void DesvanecerVelocidad(float ElapsedTime)
        {
            speed -= 50f;
        }

        public void GanarEscudo()
        {
            foreach (var mesh in beetle.Meshes)
            {
                mesh.Technique = "ConEscudoMetalico";
            }

            escudo = true;
        }

        public void PerderEscudo()
        {
            foreach (var mesh in beetle.Meshes)
            {
                mesh.Technique = "SinEscudo";
            }

            escudo = false;

        }

        public void Dispose()
        {
            this.beetle.DisposeAll();
        }

    }
}
