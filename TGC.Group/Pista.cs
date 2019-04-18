using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Group
{
    
    public class Pista
    {

        List<TgcMesh> SegmentosPista;
        String MediaDir;
        public Pista(String _MediaDir)
        {
            this.MediaDir = _MediaDir;
            this.SegmentosPista = generarPista();            
        }

        public List<TgcMesh> GetSegmentosPista()
        {
            return SegmentosPista;
        }

        TgcMesh cargarMesh(string nombreArchivo, int nroMeshes)
        {
            return new TgcSceneLoader().loadSceneFromFile(MediaDir + "/Thumper/" + nombreArchivo).Meshes[nroMeshes];
        }

        List<TgcMesh> generarPista()
        {
            int longPista = 300; //cuantas piezas va a tener
            List<TgcMesh> Pista= new List<TgcMesh>();
            float offsetPieza = 50;
            float acumOffsetPieza = 0;
            Random rnd = new Random();
            for (int i = 0; i < longPista; i++)
            {
                TgcMesh MeshAux;
                if (rnd.Next(2) > 0) MeshAux = cargarMesh("triangular_tunnel-TgcScene.xml", 0);//cargo tunel triangular (ya esta rotado)
                else
                {
                    MeshAux = cargarMesh("testMeshCreatorCircle-TgcScene.xml", 0);//cargo el tunel circular (el nuevo ya esta rotado)

                }



                MeshAux.AutoTransform = false; //No termino de enteder que hace pero si lo dejo las transformaciones no funcionan como quiero
                MeshAux.Transform *= TGCMatrix.Scaling(TGCVector3.One * rnd.Next(3, 6)) * TGCMatrix.Translation(new TGCVector3(0, 0, acumOffsetPieza));

                // MeshAux.Position += forward * acumOffsetPieza; //cada pieza una adelante de la otra            

                // MeshAux.Scale = TGCVector3.One * rnd.Next(2, 5); //escalar random --- simplique estas dos lineas en un gran transform

                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                MeshAux.setColor(randomColor);


                MeshAux.BoundingBox.transform(MeshAux.Transform);

                Pista.Add(MeshAux);
                acumOffsetPieza += offsetPieza; //todos estos valores hardcodeados deberia pasarlos a las declaraciones xd

            }
            return Pista;
        }





    }
}
