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
            int longPista = 20; //cuantas piezas va a tener
            List<TgcMesh> Pista= new List<TgcMesh>();
            TGCVector3 acumOffsetPieza = new TGCVector3 (0,0,0);
            Random rnd = new Random();
            for (int i = 0; i < longPista; i++)
            {                
                int eleccionPista = elijoEntreTresProbabilidades(60, 15, 15);
                System.Console.WriteLine(eleccionPista);
                if (eleccionPista == 1)
                { // randomizo mis tres posibilidades la primera es piso, las otras dos son tuneles

                    for (int j = 0; j < 25; j++)
                    {
                        Pista.Add(generarSegmentoPiso(acumOffsetPieza));
                        acumOffsetPieza += new TGCVector3(0, 0, 50);
                    }


                }
                else if (eleccionPista == 2)
                {
                    float offsetPiezas = 30;
                    int longitudTunel = 10;
                    Pista.AddRange(generarTunel(acumOffsetPieza, longitudTunel, "testMeshCreatorCircle-TgcScene.xml",offsetPiezas));
                    acumOffsetPieza += new TGCVector3(0, 0, offsetPiezas) * longitudTunel;
                    
                }   // MUUUUUUUUUUUUCHA REPETICION DE CODIGO #ToDo
                else
                {
                    float offsetPiezas = 30;
                    int longitudTunel = 10;
                    Pista.AddRange(generarTunel(acumOffsetPieza, longitudTunel, "triangular_tunnel-TgcScene.xml",offsetPiezas));
                    acumOffsetPieza += new TGCVector3(0, 0, offsetPiezas) * longitudTunel;
                }

            }

            return Pista;
        }

        List<TgcMesh> generarTunel(TGCVector3 Posicion, int Longitud, String nombreArchivo,float offset) //si tuviese tuenes con mas meshes deberia pasar tmb la cant meshes
        {
            List<TgcMesh> Tunel = new List<TgcMesh>();
            Random rnd = new Random();
            Color ColorRandom = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            TGCVector3 acumOffset = TGCVector3.Empty;
            for (int j = 0; j < 10; j++)
            {
                TgcMesh MeshAux = cargarMesh(nombreArchivo, 0);
                MeshAux.Move(Posicion+acumOffset + TGCVector3.Up * 5);
                MeshAux.Scale = TGCVector3.One * 3;
                MeshAux.setColor(ColorRandom);
                Tunel.Add(MeshAux);//cargo tunel circular (no esta rotada la BoundingBox)             
                acumOffset += new TGCVector3(0, 0, offset);
            }
            return Tunel;
        }

        TgcMesh generarSegmentoPiso(TGCVector3 Posicion)
        {
            TGCBox piso = new TGCBox();
            TGCVector3 Tamanio = new TGCVector3(30, 10, 30);
            TgcTexture texturaPiso = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Textures/Gray.jpg");
            piso.setTexture(texturaPiso);
            piso.Size = Tamanio;
            piso.Transform = TGCMatrix.Translation(Posicion); 
            piso.updateValues();
            return piso.ToMesh("piso");
        }


        int elijoEntreTresProbabilidades(int probA,int probB,int probC)
        {
            int probTotal = probA + probB + probC;
            Random rnd = new Random();
            int randomNumber =rnd.Next(probTotal);
            if (randomNumber < probA) return 1;
            else if (randomNumber < probA+probB) return 2;
            else return 3;
        }

        public void Render()
        {
            foreach (TgcMesh AuxMesh in GetSegmentosPista())
            {
                AuxMesh.Render();
            }
        }

        public void BoundingBoxRender()
        {
            foreach (TgcMesh AuxMesh in GetSegmentosPista())
            {
                AuxMesh.BoundingBox.Render();
            }
        }

        public void Dispose()
        {
            foreach (TgcMesh AuxMesh in GetSegmentosPista())
            {
                AuxMesh.Dispose();
            }

        }

    }
}
