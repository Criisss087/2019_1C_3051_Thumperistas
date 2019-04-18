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
            int longPista = 50; //cuantas piezas va a tener
            List<TgcMesh> Pista= new List<TgcMesh>();
            TGCVector3 acumOffsetPieza = new TGCVector3 (0,0,0);
            Random rnd = new Random();
            for (int i = 0; i < longPista; i++)
            {                
                System.Console.WriteLine(TGCVector3.PrintVector3(acumOffsetPieza));
                int eleccionPista = elijoEntreTresProbabilidades(60, 15, 15);
                if (eleccionPista == 1)
                { // randomizo mis tres posibilidades la primera es piso, las otras dos son tuneles

                    for (int j = 0; j < 50; j++)
                    {
                        Pista.Add(generarSegmentoPiso(acumOffsetPieza));
                        acumOffsetPieza += new TGCVector3(0, 0, 50);
                    }


                }
                else if (eleccionPista == 2)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        TgcMesh MeshAux = cargarMesh("triangular_tunnel-TgcScene.xml", 0);
                        MeshAux.Move(acumOffsetPieza);
                        Pista.Add(MeshAux);//cargo tunel triangular (ya esta rotado)
                        acumOffsetPieza += new TGCVector3(0, 0, 30);
                    }
                    
                }   // MUUUUUUUUUUUUCHA REPETICION DE CODIGO #ToDo
                else
                {
                    for (int j = 0; j < 10; j++)
                    {
                        TgcMesh MeshAux = cargarMesh("testMeshCreatorCircle-TgcScene.xml", 0);
                        MeshAux.Move(acumOffsetPieza);                       
                        Pista.Add(MeshAux); //cargo tunel circular (no esta rotada la BoundingBox)
                        acumOffsetPieza += new TGCVector3(0, 0, 30);
                    }
                }



               // MeshAux.AutoTransform = false; //No termino de enteder que hace pero si lo dejo las transformaciones no funcionan como quiero
               // MeshAux.Transform *= TGCMatrix.Scaling(TGCVector3.One * rnd.Next(3, 6)) * TGCMatrix.Translation(new TGCVector3(0, 0, acumOffsetPieza));

                // MeshAux.Position += forward * acumOffsetPieza; //cada pieza una adelante de la otra            

                // MeshAux.Scale = TGCVector3.One * rnd.Next(2, 5); //escalar random --- simplique estas dos lineas en un gran transform

                //Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                //MeshAux.setColor(randomColor);


                //MeshAux.BoundingBox.transform(MeshAux.Transform);

            }

            return Pista;
        }

        TgcMesh generarSegmentoPiso(TGCVector3 Posicion)
        {
            TGCBox piso = new TGCBox();
            TGCVector3 Tamanio = new TGCVector3(30, 10, 30);
            piso.Color = Color.DarkGray;
            piso.Size = Tamanio;
            piso.Transform = TGCMatrix.Translation(Posicion); //Posicion;
            //System.Console.WriteLine(piso.Position.ToString());
            piso.updateValues();
            return piso.ToMesh("piso");
        }


        int elijoEntreTresProbabilidades(int probA,int probB,int probC)
        {
            int probTotal = probA + probB + probC;
            Random rnd = new Random();
            int randomNumber =rnd.Next(probTotal);
            if (randomNumber < probA) return 1;
            else if (randomNumber >= probA && randomNumber < probB) return 2;
            else return 3;
        }

    }
}
