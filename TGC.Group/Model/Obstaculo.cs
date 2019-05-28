using System;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;



namespace TGC.Group.Model
{
    public class Obstaculo
    {
        private TGCBox box;
        public TgcMesh boxMesh;

        public TGCMatrix Translation { get; set; }
        public TGCMatrix Scaling { get; set; }
        public TGCMatrix Rotation { get; set; }
        public TGCVector3 Position { get; set; }
        public TgcBoundingOrientedBox Collider;

        public Obstaculo(TGCVector3 PosicionInicial)
        {
            box = new TGCBox();
            TGCVector3 Tamanio = new TGCVector3(30, 30, 30);
            box.AutoTransformEnable = false;
            box.Size = Tamanio;

            Translation = TGCMatrix.Translation(PosicionInicial);
            Scaling = TGCMatrix.Scaling(new TGCVector3(40f, 0.1f, 0.1f));
            Rotation = TGCMatrix.RotationYawPitchRoll(1, 1, 1);

            box.Transform = Scaling * Rotation * Translation;
            box.updateValues();

            boxMesh = box.ToMesh("box");

            boxMesh.AutoUpdateBoundingBox = false;
            boxMesh.Position = PosicionInicial;
            boxMesh.Rotation = new TGCVector3(1, 1, 1);

            Collider = TgcBoundingOrientedBox.computeFromAABB(boxMesh.BoundingBox);

        }

        public void Update()
        {

        }

        public void Render()
        {
            boxMesh.Render();
            Collider.Render();
        }

    }
}
