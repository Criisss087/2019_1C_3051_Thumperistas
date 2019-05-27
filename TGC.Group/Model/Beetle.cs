using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.BoundingVolumes;
using System.Drawing;
using System.Collections.Generic;
using TGC.Core.Collision;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    class Beetle
    {

        public const float VELOCIDAD_ANGULAR = 15f;
        public const float VELOCIDAD = 600f;
        public const float VELOCIDADX = 500f;

        private TgcSceneLoader loader;
        public TgcScene beetle { get; set; }
        public float speed { get; set; }
        public TgcBoundingOrientedBox collider;

        public TGCMatrix translation { get; set; }
        public TGCMatrix scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TGCVector3 position { get; set; }
        public float traslacionZ { get; set; }
        public bool poderActivado = false;

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

            // Escalo el collider para no salirme de la pista en X, y para tener mas precision en Z con los recolectables
            var newScaling = scaling;
            var newTrasnlsation = TGCMatrix.Translation(new TGCVector3(-40,10,60));
            newScaling.Scale(2,1,0.5f);
            beetle.BoundingBox.transform(newScaling * newTrasnlsation);
            collider = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);
            
            this.speed = 900f;

        }

        public void Update()
        {   

        }

        public TGCVector3 Avanza(float ElapsedTime, float posX, float posY, float distAng)
        {
            position += new TGCVector3(posX, posY, speed * ElapsedTime);

            translation = TGCMatrix.Translation(position);
            rotation = TGCMatrix.RotationY(distAng);

            collider.move(new TGCVector3(posX, posY, speed * ElapsedTime));
            
            return position;
        }

        

        public bool ColisionandoConRecolectable(List<Recolectable> recolectables,ref Recolectable objetoColisionado)
        {
            foreach(Recolectable ObjRecoleactable in recolectables){
                if (TgcCollisionUtils.testSphereOBB(ObjRecoleactable.Collider,collider))
                {
                    objetoColisionado = ObjRecoleactable;
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

        public void ActivarPoder()
        {
            speed += 50f;
            //poderActivado = true;
        }

        public void DesvanecerVelocidad(float ElapsedTime)
        {
            /*if (speed > 400f)
                speed -= ElapsedTime*100;
            else
            {
                speed = 400f;
                System.Console.WriteLine("Desactive mi poder");
                poderActivado = false;
            }*/
            speed -= 50f;
        }

        public void Dispose()
        {
            this.beetle.DisposeAll();
        }

    }
}
