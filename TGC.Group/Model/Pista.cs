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
using Microsoft.DirectX.Direct3D;

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

        private bool curvaSuaveActiva;
        public int cantCurvaSuaveActual = 0;
        public TGCVector3 rotCurvaActual = new TGCVector3(0, 0, 0);
        public TGCVector3 trasCurvaActual = new TGCVector3(0, 0, 0);
        private const float OFFSETPIEZAS = 60;


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
            // Primero agrego un camino de 50 para el inicio 
            for (int j = 0; j < 50; j++)
            {
                this.SegmentosPista.Add(generarSegmentoPiso());
                this.posUltimaPieza += new TGCVector3(0, 0, OFFSETPIEZAS);
            }
        }

        public void UpdatePista()
        {
            // La pista se mantiene infinita, agrego un elemento al final, y remuevo el primero, que ya lo paso el beetle
            this.SegmentosPista.Add(generarSegmentoPiso());
            this.SegmentosPista.RemoveAt(0);

            if(this.tunelActivo)
            {
                TgcMesh MeshAux = cargarMesh(this.rutaTunelActual, 0);
                MeshAux.Move(this.posUltimaPieza + TGCVector3.Up * 5);
                MeshAux.Scale = new TGCVector3(3, 3, 6); //.One * 3;
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

            if (this.curvaSuaveActiva)
            {
                cantCurvaSuaveActual--;
                if (cantCurvaSuaveActual == 0)
                {
                    this.curvaSuaveActiva = false;
                    this.rotCurvaActual = new TGCVector3(0, 0, 0);
                    this.trasCurvaActual = new TGCVector3(0, 0, 0);
                }                    
            }

            this.posUltimaPieza += ( new TGCVector3(0, 0, OFFSETPIEZAS) + this.trasCurvaActual );
            this.SegmentosPista.ElementAt(0).GetPosition(this.posActual);
        }

        public void UpdateCurvaSuave()
        {   
            if(!this.curvaSuaveActiva)
            {
                int direccionCurvaSuave = rnd.Next(50);
                float rotationY = 0;
                float posX = this.posUltimaPieza.X;
                float anguloY = 45f;
                float trasX;

                trasX = 0f;

                // Bloques de la curva
                this.cantCurvaSuaveActual = this.rnd.Next(61);
                if (this.cantCurvaSuaveActual < 30)
                    this.cantCurvaSuaveActual += 30;

                switch (direccionCurvaSuave)
                {
                    //  Derecha                    
                    case 1:
                        rotationY = anguloY / cantCurvaSuaveActual;
                        trasX = 30f / cantCurvaSuaveActual;
                        this.curvaSuaveActiva = true;
                        break;
                    // Izquierda
                    case 2:
                        Console.WriteLine("Dobla a la izquierda?");
                        rotationY = -anguloY / cantCurvaSuaveActual;
                        trasX = -(30f / cantCurvaSuaveActual);
                        this.curvaSuaveActiva = true;
                        break;
                    
                    default:
                        
                        break;
                }

                if (!curvaSuaveActiva)
                    cantCurvaSuaveActual = 0;

                this.trasCurvaActual += new TGCVector3(trasX, 0, 0);
                this.rotCurvaActual += new TGCVector3(0, rotationY, 0);
            }
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
                    this.rutaTunelActual = "testMeshCreatorCircle-TgcScene.xml";
                    this.tunelActivo = true;
                }
                else if (eleccionPista == 3)
                {
                    this.rutaTunelActual = "triangular_tunnel-TgcScene.xml";
                    this.tunelActivo = true;
                }
                else
                {
                    this.rutaTunelActual = "";
                    this.tunelActivo = false;
                }

                if (!this.tunelActivo)
                {
                    this.cantTunelActual = 0;
                    this.tunelActivo = false;
                }
                

            }
        }
       
        private void generarTunel(String nombreArchivo, float offset, int cant) 
        {
            Random rnd = new Random();
            Color ColorRandom = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            TGCVector3 posTunel = new TGCVector3();
            posTunel = this.posUltimaPieza; 

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
                    posTunel += new TGCVector3(0, 0, offset);
                }

                this.posUltimoTunel = posTunel;
            }
        }

        TgcMesh generarSegmentoPiso()
        {
            TGCBox piso = new TGCBox();
            TGCVector3 Tamanio = new TGCVector3(30, 10, 30);
            TgcTexture texturaPiso = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Textures/gray.jpg");
            piso.AutoTransformEnable = false;
            piso.setTexture(texturaPiso);
            piso.Size = Tamanio;

            TGCVector3 rot = new TGCVector3(0,0,0);
            rot.X = Geometry.DegreeToRadian(this.rotCurvaActual.X);
            rot.Y = Geometry.DegreeToRadian(this.rotCurvaActual.Y);
            rot.Z = Geometry.DegreeToRadian(this.rotCurvaActual.Z);

            // YAW = Y, va primero
            piso.Transform = TGCMatrix.Scaling(new TGCVector3(1, 1, 2)) *
                             TGCMatrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z) *  
                             TGCMatrix.Translation(this.posUltimaPieza);
            

            piso.updateValues();

            TgcMesh pisoMesh = piso.ToMesh("piso");

            pisoMesh.AutoUpdateBoundingBox = false;
            pisoMesh.Position = this.posUltimaPieza;
            agregoRecolectables(this.posUltimaPieza);
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
