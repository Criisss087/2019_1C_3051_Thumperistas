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
        private TgcSceneLoader Loader;
        public TgcMesh Mesh { get; set; }
        public float Speed { get; set; }
        TgcBoundingOrientedBox Collider;


        public TGCMatrix translation { get; set; }
        public TGCMatrix scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TGCVector3 position { get; set; }
        public float traslacionZ { get; set; }
        public bool poderActivado = false;
        



        public Beetle(string _mediaDir)
        {
            this.Loader = new TgcSceneLoader();

            Mesh = Loader.loadSceneFromFile(_mediaDir + "/Thumper/beetle-TgcScene.xml").Meshes[7];
            //Mesh.AutoTransform = false;

            //Modifico como quiero que empiece el mesh
            position = new TGCVector3(0, 10, 0);
            translation = TGCMatrix.Translation(position);
            scaling = TGCMatrix.Scaling(TGCVector3.One * .5f);  
            rotation = TGCMatrix.RotationY(FastMath.PI_HALF);

            //Seteo collider
            Mesh.BoundingBox.transform(translation * scaling * rotation);
            Collider = TgcBoundingOrientedBox.computeFromAABB(Mesh.BoundingBox);

            this.Speed = 400f;

        }
        
        public TGCVector3 Position()
        {
            return this.Mesh.Position;
        }

        public void Update()
        {   
            //Creo que va a ser mejor que el beetle no se mueva, solo se mueva la pista
            //position += new TGCVector3(0, 0, 1) * ElapsedTime * Speed;
            //position += new TGCVector3(0, 0, 1) * Speed;
        }

        public void Avanza(float ElapsedTime)
        {
          position += new TGCVector3(0, 0, Speed*ElapsedTime);
          translation = TGCMatrix.Translation(position);
          Collider.move(new TGCVector3(0, 0, Speed * ElapsedTime));
        }

        

        public bool ColisionandoConRecolectable(List<Recolectable> recolectables,ref Recolectable objetoColisionado)
        {
            foreach(Recolectable ObjRecoleactable in recolectables){
                if (TgcCollisionUtils.testSphereOBB(ObjRecoleactable.Collider,Collider))
                {
                    objetoColisionado = ObjRecoleactable;
                    return true;
                }
            }
            return false;
        }

       

        public void Render()
        {
            Mesh.Transform = scaling * rotation * translation;
            Mesh.Render();
            Collider.setRenderColor(Color.Blue);
            Collider.Render();
        }

        public void ActivarPoder()
        {
            Speed += 200f;
            poderActivado = true;
        }

        public void DesvanecerVelocidad(float ElapsedTime)
        {
            if (Speed > 400f) Speed -= ElapsedTime*100;
            else
            {
                Speed = 400f;
                System.Console.WriteLine("Desactive mi poder");
                poderActivado = false;
            }
        }

        public void Dispose()
        {
            this.Mesh.Dispose();
        }

    }
}
