using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using System;
using System.Collections.Generic;
using TGC.Examples.Camara;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    class Beetle
    {
        private TgcSceneLoader Loader;
        public TgcMesh Mesh { get; set; }
        public float Speed { get; set; }

        public TGCVector3 position { get; set; }
        public TGCVector3 scaling { get; set; }
        public TGCMatrix rotation { get; set; }

        public Beetle(string _mediaDir)
        {
            this.Loader = new TgcSceneLoader();

            Mesh = Loader.loadSceneFromFile(_mediaDir + "/Thumper/beetle-TgcScene.xml").Meshes[7];
            Mesh.AutoTransform = false;


            //Modifico como quiero que empiece el mesh
            position = new TGCVector3(0, 10, 0);
            scaling = TGCVector3.One * .5f;
            rotation = TGCMatrix.RotationY(FastMath.PI_HALF);


            //tambien tengo que rota el boundingbox porque eso no se actuliza
            // Mesh.BoundingBox.transform(TGCMatrix.RotationY(FastMath.PI_HALF)); ya no estoy tan seguro de esto


            this.Speed = 1500f;

        }
        
        public TGCVector3 Position()
        {
            return this.Mesh.Position;
        }

        public void Update()
        {
            position += new TGCVector3(0, 0, 1) * ElapsedTime * Speed;
        }

        public void Render()
        {
            Mesh.Transform = TGCMatrix.Scaling(scaling) * rotation * TGCMatrix.Translation(position);

            Mesh.Render();


        }

        public void Dispose()
        {
            this.Mesh.Dispose();
        }

    }
}
