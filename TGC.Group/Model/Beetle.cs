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

namespace TGC.Group.Model
{
    class Beetle
    {
        private TgcSceneLoader Loader;
        public TgcMesh Mesh { get; set; }
        public float Speed { get; set; }

        public TGCMatrix traslation { get; set; }
        public TGCMatrix scaling { get; set; }
        public TGCMatrix rotation { get; set; }
        public TGCVector3 position { get; set; }
        public float traslacionZ { get; set; }

        public Beetle(string _mediaDir)
        {
            this.Loader = new TgcSceneLoader();

            Mesh = Loader.loadSceneFromFile(_mediaDir + "/Thumper/beetle-TgcScene.xml").Meshes[7];
            Mesh.AutoTransform = false;

            //Modifico como quiero que empiece el mesh
            position = new TGCVector3(0, 10, 0);
            traslation = TGCMatrix.Translation(position);
            scaling = TGCMatrix.Scaling(TGCVector3.One * .5f);  
            rotation = TGCMatrix.RotationY(FastMath.PI_HALF);

            // tambien tengo que rota el boundingbox porque eso no se actuliza
            Mesh.BoundingBox.transform(TGCMatrix.RotationY(FastMath.PI_HALF)); 

            this.Speed = 5f;

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

        public void Render()
        {
            Mesh.Transform = scaling * rotation * traslation;
            Mesh.Render();
        }

        public void Dispose()
        {
            this.Mesh.Dispose();
        }

    }
}
