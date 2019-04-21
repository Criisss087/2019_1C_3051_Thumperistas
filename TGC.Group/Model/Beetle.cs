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

        public Beetle(string _mediaDir)
        {
            this.Loader = new TgcSceneLoader();

            Mesh = Loader.loadSceneFromFile(_mediaDir + "/Thumper/beetle-TgcScene.xml").Meshes[7];

            //Modifico como quiero que empiece el mesh
            Mesh.Position = TGCVector3.Empty;

            Mesh.Scale = TGCVector3.One * .5f; //Escalo a la mitad del beetle            
            Mesh.RotateY(FastMath.PI_HALF); //Lo roto
            
            //tambien tengo que rota el boundingbox porque eso no se actuliza
            Mesh.BoundingBox.transform(TGCMatrix.RotationY(FastMath.PI_HALF));

            Mesh.Position += new TGCVector3(0f, 10f, 0f);

            this.Speed = 300f;

        }
        
        public TGCVector3 Position()
        {
            return this.Mesh.Position;
        }

        public void Dispose()
        {
            this.Mesh.Dispose();
        }

    }
}
