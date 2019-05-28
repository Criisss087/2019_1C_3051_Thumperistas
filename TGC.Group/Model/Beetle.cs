using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.BoundingVolumes;
using System.Drawing;
using System.Collections.Generic;
using TGC.Core.Collision;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Input;

namespace TGC.Group.Model
{
    class Beetle
    {

        public const float VELOCIDAD_ANGULAR = 15f;
        public const float VELOCIDAD = 900f;
        public const float VELOCIDADX = 200f;

        private TgcSceneLoader loader;
        public TgcScene beetle { get; set; }
        public float speed { get; set; }
        public bool escudo { get; set; } = true;
        public bool slide { get; set; } = false;
        public bool derecha { get; set; } = false;
        public bool izquierda { get; set; } = false;
        public bool godMode { get; set; } = false;
        public TgcBoundingOrientedBox collider;
        public TgcBoundingOrientedBox colliderRecolectables;


        public TGCMatrix translation { get; set; }
        public TGCMatrix scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TGCVector3 position { get; set; }
        public float traslacionZ { get; set; }
        public bool poderActivado = false;
        public float distAng = FastMath.PI_HALF;

        public Beetle(string _mediaDir)
        {
            this.loader = new TgcSceneLoader();

            beetle = loader.loadSceneFromFile(_mediaDir + "/Thumper/beetle-TgcScene.xml");

            //Modifico como quiero que empiece el mesh
            position = new TGCVector3(0, 8f, 0);
            translation = TGCMatrix.Translation(position);
            scaling = TGCMatrix.Scaling(TGCVector3.One * .5f);
            //rotation = TGCMatrix.RotationY(Geometry.DegreeToRadian(45));         //FastMath.PI_HALF);

            foreach (var mesh in beetle.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Transform = scaling * rotation * translation;
                mesh.BoundingBox.transform(scaling * rotation * translation);
            }

            //Seteo collider
            beetle.BoundingBox.transform(translation * scaling * rotation);
            colliderRecolectables = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

            // Escalo el bb para un nuevo collider en la pista, para no salirme de la pista en X
            var newScaling = scaling;
            var newTrasnlsation = TGCMatrix.Translation(new TGCVector3(-40, 10, 60));
            newScaling.Scale(2, 1, 0.5f);
            beetle.BoundingBox.transform(newScaling * newTrasnlsation);
            collider = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

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

            if (Input.keyPressed(Key.G) && godMode)
            {
                godMode = false;
            }
            if (Input.keyPressed(Key.G) && !godMode)
            {
                godMode = true;
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


            // para debug
            if (Input.keyPressed(Key.E))
                escudo = true;

            // Para armar un menu de pausa... falta desarrollar
            if (Input.keyPressed(Key.Escape))
            {
                if (speed == 0f)
                    speed = Beetle.VELOCIDAD;
                else if (speed == Beetle.VELOCIDAD)
                    speed = 0f;
            }

        }

        public bool Sliding()
        {
            return slide || izquierda || derecha;
        }

        public TGCVector3 Avanza(float ElapsedTime, float posX, float posY)
        {
            position += new TGCVector3(posX, posY, speed * ElapsedTime);

            translation = TGCMatrix.Translation(position);
            rotation = TGCMatrix.RotationY(distAng);

            collider.move(new TGCVector3(posX, posY, speed * ElapsedTime));

            return position;
        }



        public bool ColisionandoConRecolectable(List<Recolectable> recolectables, ref Recolectable objetoColisionado)
        {
            foreach (Recolectable ObjRecoleactable in recolectables)
            {
                if (TgcCollisionUtils.testSphereOBB(ObjRecoleactable.Collider, colliderRecolectables))
                {
                    objetoColisionado = ObjRecoleactable;
                    return true;
                }
            }
            return false;
        }

        public bool ColisionandoConObstaculo(List<Obstaculo> obstaculos, ref Obstaculo objetoColisionado)
        {
            foreach (Obstaculo obs in obstaculos)
            {
                if (TgcCollisionUtils.testObbObb(obs.Collider, colliderRecolectables))
                {
                    objetoColisionado = obs;
                    return true;
                }
            }
            return false;
        }

        public void Render()
        {
            foreach (var mesh in beetle.Meshes)
            {
                mesh.Transform = scaling * rotation * translation;
                mesh.BoundingBox.transform(scaling * rotation * translation);
            }
            beetle.RenderAll();
            collider.setRenderColor(Color.Blue);
            collider.Render();
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
            escudo = true;
        }

        public void PerderEscudo()
        {
            escudo = false;
        }

        public void Dispose()
        {
            this.beetle.DisposeAll();
        }

    }
}
