using System;
using TGC.Core.BoundingVolumes;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;



namespace TGC.Group.Model
{
    public class Recolectable
    {
        private TgcScene scene;
        
        public TGCMatrix translation { get; set; }
        public TGCMatrix scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TgcBoundingSphere Collider;

        public Recolectable(String mediaDir, TGCVector3 PosicionInicial)
        {
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");

            //cambios iniciales
            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
            }

            rotation = TGCMatrix.Identity;
            translation = TGCMatrix.Translation(new TGCVector3(0, 10, 0) + PosicionInicial); // el +10 es para que este sobre la pista
            scaling = TGCMatrix.Scaling(TGCVector3.One*0.2f);

            //seteo colisionador esfera
            //Collider = new TgcBoundingSphere(translation, 5f);
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

        
        public void Render()
        {
            foreach (var mesh in scene.Meshes)
            {
                mesh.Transform = scaling * rotation * translation;
                mesh.Render();
            }
            Collider.Render();
        }

    }
}
