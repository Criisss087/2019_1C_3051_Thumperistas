using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.BoundingVolumes;
using System.Drawing;
using System.Collections.Generic;
using TGC.Core.Collision;

namespace TGC.Group.Model
{
    class Beetle
    {
        private TgcSceneLoader loader;
        public TgcScene beetle { get; set; }
        public float speed { get; set; }
        TgcBoundingOrientedBox collider;

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
            position = new TGCVector3(0, 10, 0);
            translation = TGCMatrix.Translation(position);
            scaling = TGCMatrix.Scaling(TGCVector3.One * .5f);  
            rotation = TGCMatrix.RotationY(FastMath.PI_HALF);

            foreach (var mesh in beetle.Meshes)
            {
                mesh.AutoTransformEnable = false;
                mesh.Transform = scaling * rotation * translation;
                mesh.BoundingBox.transform(scaling * rotation * translation);
            }

            //Seteo collider
            beetle.BoundingBox.transform(translation * scaling * rotation);
            collider = TgcBoundingOrientedBox.computeFromAABB(beetle.BoundingBox);

            this.speed = 400f;

        }

        public void Update()
        {   
            //Creo que va a ser mejor que el beetle no se mueva, solo se mueva la pista
            //position += new TGCVector3(0, 0, 1) * ElapsedTime * Speed;
            //position += new TGCVector3(0, 0, 1) * Speed;
        }

        public void Avanza(float ElapsedTime)
        {
          position += new TGCVector3(0, 0, speed*ElapsedTime);
          translation = TGCMatrix.Translation(position);
          collider.move(new TGCVector3(0, 0, speed * ElapsedTime));
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
            speed += 200f;
            poderActivado = true;
        }

        public void DesvanecerVelocidad(float ElapsedTime)
        {
            if (speed > 400f)
                speed -= ElapsedTime*100;
            else
            {
                speed = 400f;
                System.Console.WriteLine("Desactive mi poder");
                poderActivado = false;
            }
        }

        public void Dispose()
        {
            this.beetle.DisposeAll();
        }

    }
}
