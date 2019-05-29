using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    public class Disparo
    {
        private TgcScene scene;
        public TGCMatrix Translation { get; set; }
        public TGCMatrix Scaling { get; set; }
        public TGCMatrix Rotation { get; set; }
        public TGCVector3 Position { get; set; }
        public TgcBoundingSphere Collider;
        public static float speed = 1800f;

        public Disparo(String mediaDir, TGCVector3 PosicionInicial)
        {
            scene = new TgcSceneLoader().loadSceneFromFile(mediaDir + "/Thumper/Sphere-TgcScene.xml");

            foreach (var mesh in scene.Meshes)
            {
                mesh.AutoTransformEnable = false;
            }

            Position = PosicionInicial;
            Rotation = TGCMatrix.Identity;
            Translation = TGCMatrix.Translation(new TGCVector3(0, 10, 0) + PosicionInicial); 
            Scaling = TGCMatrix.Scaling(TGCVector3.One * 0.2f);

            //seteo colisionador esfera
            Collider = new TgcBoundingSphere(new TGCVector3(0, 10, 0) + PosicionInicial, 5f);

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

            return Position;
        }

        public void Render()
        {
            foreach (var mesh in scene.Meshes)
            {
                mesh.Transform = Scaling * Rotation * Translation;
                mesh.Render();
            }
            //Collider.Render();
        }

    }
}
