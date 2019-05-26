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
using TGC.Group.Model;

namespace TGC.Group
{
    
    public class Pista
    {

        public List<TgcMesh> SegmentosPista = new List<TgcMesh>();
        public List<TgcMesh> SegmentosTunel = new List<TgcMesh>();
        public List<Recolectable> Recolectables = new List<Recolectable>();
        public String MediaDir;
        public TGCVector3 posActual = new TGCVector3(0,0,0);
        public TGCVector3 posUltimaPieza = new TGCVector3(0, 0, 0);
        public TGCVector3 posUltimoTunel = new TGCVector3(0, 0, 0);
        private Random rnd = new Random();

        private bool tunelActivo;
        private int cantTunelActual = 0;
        private String rutaTunelActual;
        private Color colorTunelActual;

        public Pista(String _MediaDir)
        {
            this.MediaDir = _MediaDir;
            generarPista();
        }

        public List<TgcMesh> GetSegmentosPista()
        {
            return SegmentosPista;
        }

        TgcMesh cargarMesh(string nombreArchivo, int nroMeshes)
        {
            return new TgcSceneLoader().loadSceneFromFile(MediaDir + "/Thumper/" + nombreArchivo).Meshes[nroMeshes];
        }

        private void generarPista()
        {;
            // Primero agrego un camino de 100 para el inicio 
            for (int j = 0; j < 100; j++)
            {
                this.SegmentosPista.Add(generarSegmentoPiso(this.posUltimaPieza));
                this.posUltimaPieza += new TGCVector3(0, 0, 50);
            }
        }

        public void UpdatePista()
        {
            // La pista se mantiene infinita
            this.SegmentosPista.Add(generarSegmentoPiso(this.posUltimaPieza));
            
            this.SegmentosPista.RemoveAt(0);

            if(this.tunelActivo)
            {
                TgcMesh MeshAux = cargarMesh(this.rutaTunelActual, 0);
                MeshAux.Move(this.posUltimaPieza + TGCVector3.Up * 5);
                MeshAux.Scale = TGCVector3.One * 3;
                MeshAux.setColor(this.colorTunelActual);
                this.SegmentosTunel.Add(MeshAux);
                this.cantTunelActual--;

                if(this.cantTunelActual == 0)
                {
                    this.tunelActivo = false;
                }

                if (this.SegmentosTunel.ElementAt(0).Position.Z < (this.posActual.Z - 100 ))
                    this.SegmentosTunel.RemoveAt(0);
            }

            this.posUltimaPieza += new TGCVector3(0, 0, 50);
        }

        public void UpdateTunel()
        {
            if (!this.tunelActivo)
            {
                // vars para tuneles
                // Generacion aleatoria de túneles sobre la pista
                int eleccionPista = elijoEntreTresProbabilidades(700, 10, 10);

                //Cantidad de posiciones del tunel
                //float offsetPiezas = 50;
                this.cantTunelActual = this.rnd.Next(31);
                if (this.cantTunelActual < 15)
                    this.cantTunelActual += 15;

                this.colorTunelActual = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

                if (eleccionPista == 1)
                {
                    this.rutaTunelActual = "";
                    this.tunelActivo = false;
                }
                else if (eleccionPista == 2)
                {
                    //generarTunel("testMeshCreatorCircle-TgcScene.xml", offsetPiezas, longitudTunel);
                    this.rutaTunelActual = "testMeshCreatorCircle-TgcScene.xml";
                    this.tunelActivo = true;
                }
                else if (eleccionPista == 3)
                {
                    //generarTunel("triangular_tunnel-TgcScene.xml", offsetPiezas, longitudTunel);
                    this.rutaTunelActual = "triangular_tunnel-TgcScene.xml";
                    this.tunelActivo = true;
                }
            }
        }

        private void generarTunel(String nombreArchivo, float offset, int cant) //si tuviese tuenes con mas meshes deberia pasar tmb la cant meshes
        {
            Random rnd = new Random();
            Color ColorRandom = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            TGCVector3 posTunel = new TGCVector3();
            posTunel = this.posUltimaPieza; // - (new TGCVector3(0, 0, offset * cant));

            if (this.posActual.Z > this.posUltimoTunel.Z )
            {
                this.SegmentosTunel.Clear();

                for (int j = 0; j < cant; j++)
                {
                    TgcMesh MeshAux = cargarMesh(nombreArchivo, 0);
                    MeshAux.Move(posTunel + TGCVector3.Up * 5);
                    MeshAux.Scale = TGCVector3.One * 3;
                    MeshAux.setColor(ColorRandom);
                    this.SegmentosTunel.Add(MeshAux);
                    //this.SegmentosPista.RemoveAt(0);
                    posTunel += new TGCVector3(0, 0, offset);
                }

                this.posUltimoTunel = posTunel;
                //this.cantUltimoTunel = cant;
            }
        }

        TgcMesh generarSegmentoPiso(TGCVector3 Posicion)
        {
            TGCBox piso = new TGCBox();
            TGCVector3 Tamanio = new TGCVector3(30, 10, 30);
            TgcTexture texturaPiso = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Textures/gray.jpg");
            piso.setTexture(texturaPiso);
            piso.Size = Tamanio;
            piso.Transform = TGCMatrix.Translation(Posicion); 
            piso.updateValues();

            TgcMesh pisoMesh = piso.ToMesh("piso");
            agregoRecolectables(Posicion);
            return pisoMesh;
        }

        void agregoRecolectables(TGCVector3 Posicion)
        {

            if (elijoEntreTresProbabilidades(10, 50, 40) == 1)
            {
                Recolectable nuevoRecoletable = new Recolectable(MediaDir, Posicion);
                Recolectables.Add(nuevoRecoletable);
            }
        }


        int elijoEntreTresProbabilidades(int probA,int probB,int probC)
        {
            int probTotal = probA + probB + probC;
            int randomNumber = rnd.Next(probTotal);
            if (randomNumber < probA) return 1;
            else if (randomNumber < probA+probB) return 2;
            else return 3;
        }

        public void Render()
        {
            foreach(Recolectable AuxRec in Recolectables)
            {
                AuxRec.Render();
            }
            foreach (TgcMesh AuxMesh in this.SegmentosPista)
            {
                AuxMesh.Render();
            }
            foreach (TgcMesh AuxMesh in this.SegmentosTunel)
            {
                AuxMesh.Render();
            }
        }

        public void BoundingBoxRender()
        {
            foreach (TgcMesh AuxMesh in SegmentosPista)
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
