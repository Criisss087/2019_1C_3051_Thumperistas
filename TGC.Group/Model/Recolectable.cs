using System;
using TGC.Core.BoundingVolumes;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;



namespace TGC.Group.Model
{
    public class Recolectable
    {
        TgcMesh Mesh;
        
        public TGCVector3 translation { get; set; }
        public TGCVector3 scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TgcBoundingSphere Collider;

        public Recolectable(String mediaDir, TGCVector3 PosicionInicial)
        {
            
            Mesh = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml").Meshes[0];
            //Mesh.AutoTransform = false;

            

            //cambios iniciales
            rotation = TGCMatrix.Identity;
            translation = new TGCVector3(0, 10, 0) + PosicionInicial; // el +10 es para que este sobre la pista
            scaling = TGCVector3.One*0.2f;

            //seteo colisionador esfera
            Collider = new TgcBoundingSphere(translation, 5f);

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
            Mesh.Transform = TGCMatrix.Scaling(scaling)* rotation *TGCMatrix.Translation(translation);
            Mesh.Render();
            Collider.Render();
        }

    }
}
