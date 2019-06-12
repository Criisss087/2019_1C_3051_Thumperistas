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
using TGC.Core.Shaders;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Group
{

    public class Pista
    {
        Effect efectoTunel;
        float time;

        public List<TgcMesh> SegmentosPista = new List<TgcMesh>();
        public List<TgcMesh> SegmentosTunel = new List<TgcMesh>();
        public List<Recolectable> Recolectables = new List<Recolectable>();
        public List<Obstaculo> Obstaculos = new List<Obstaculo>();
        public String MediaDir;
        public String ShadersDir;
        public TGCVector3 posActual = new TGCVector3(0, 0, 0);
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
        public TipoCurva tipoCurva = 0;
        private const float OFFSETPIEZAS = 60;

        public bool obstaculosActivos;
        public int cantObsActual = 0;

        public enum TipoCurva
        {
            Derecha = 1,
            Izquierda = 2
        }

        public Pista(String _MediaDir, String _ShadersDir)
        {        
            
            this.MediaDir = _MediaDir;
            this.ShadersDir = _ShadersDir;



            this.efectoTunel = CargarEfecto("TunelShader.fx");
            this.time = 0;
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
        {
            ;
            // Primero agrego un camino de 50 para el inicio 
            for (int j = 0; j < 50; j++)
            {
                this.SegmentosPista.Add(generarSegmentoPiso(true));
                this.posUltimaPieza += new TGCVector3(0, 0, OFFSETPIEZAS);
            }
        }

        public void UpdatePista(bool soloPista)
        {
            // La pista se mantiene infinita, agrego un elemento al final, y remuevo el primero, que ya lo paso el beetle
            this.SegmentosPista.Add(generarSegmentoPiso(soloPista));
            this.SegmentosPista.RemoveAt(0);

            if (this.tunelActivo)
            {
                TgcMesh MeshAux = cargarMesh(this.rutaTunelActual, 0);
                MeshAux.Move(this.posUltimaPieza + TGCVector3.Up * 5);
                MeshAux.Scale = new TGCVector3(3, 3, 6); //.One * 3;
				
				// Copio rotacion del bloque del piso
				TGCVector3 rot = new TGCVector3(0, 0, 0);
				rot.X = Geometry.DegreeToRadian(this.rotCurvaActual.X);
				if (cantCurvaSuaveActual != 0)
					rot.Y = Geometry.DegreeToRadian(this.rotCurvaActual.Y / curvar(cantCurvaSuaveActual));
				else
					rot.Y = Geometry.DegreeToRadian(this.rotCurvaActual.Y);
				rot.Z = Geometry.DegreeToRadian(this.rotCurvaActual.Z);
				MeshAux.Transform = TGCMatrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z);
				
                MeshAux.setColor(this.colorTunelActual);
                this.SegmentosTunel.Add(MeshAux);
                this.cantTunelActual--;

                if (this.cantTunelActual == 0)
                {
                    this.tunelActivo = false;
                }

                if (this.SegmentosTunel.ElementAt(0).Position.Z < (this.posActual.Z - 100))
                    this.SegmentosTunel.RemoveAt(0);
            }

            if (this.obstaculosActivos && !curvaSuaveActiva)
            {
                Obstaculo obs = new Obstaculo(this.posUltimaPieza);
                obs.boxMesh.Move(this.posUltimaPieza + TGCVector3.Up * 5);
                obs.boxMesh.Scale = new TGCVector3(3, 3, 6);
 
                this.Obstaculos.Add(obs);
                this.cantObsActual--;

                if (this.cantObsActual == 0)
                {
                    this.obstaculosActivos = false;
                }

                // Revisar si hay que eliminarlo asi nomas
                //if (this.Obstaculos.ElementAt(0).Position.Z < (this.posActual.Z - 100))
                   //this.Obstaculos.RemoveAt(0);

            }

            this.rotCurvaActual = new TGCVector3(0, 0, 0);
            this.trasCurvaActual = new TGCVector3(0, 0, 0);
            if (this.curvaSuaveActiva)
            {
                this.trasCurvaActual += new TGCVector3(getTrasXCurva(), 0, 0);
                this.rotCurvaActual += new TGCVector3(0, getAnguloYCurva(), 0);

                cantCurvaSuaveActual--;
                if (cantCurvaSuaveActual == 0)
                {
                    this.curvaSuaveActiva = false;
                }
            }

            this.posUltimaPieza += (new TGCVector3(0, 0, OFFSETPIEZAS) + this.trasCurvaActual);
            this.SegmentosPista.ElementAt(0).GetPosition(this.posActual);

            // Remuevo recolectables que ya pasaron al beetle
            Recolectables.RemoveAll(rec => rec.Position.Z < (posActual.Z - 300));
			
			//Revisar
			//Obstaculos.RemoveAll(obs => obs.Position.Z < (posActual.Z - 2000));

        }

        public void UpdateCurvaSuave()
        {
            if (!this.curvaSuaveActiva)
            {
                tipoCurva = (TipoCurva) rnd.Next(150);

                // Bloques de la curva
                this.cantCurvaSuaveActual = 5;

                switch (tipoCurva)
                {
                    case TipoCurva.Derecha:
                    case TipoCurva.Izquierda:
                        this.curvaSuaveActiva = true;
                        break;

                    default:
                        curvaSuaveActiva = false;
                        break;
                }

                if (!curvaSuaveActiva)
                    cantCurvaSuaveActual = 0;

            }
        }

        private float getAnguloYCurva()
        {
            float f = 0f;

            switch(cantCurvaSuaveActual)
            {
                case 5:
                    f = 75f;
                    break;
                case 4:
                    f = 100f;
                    break;
                case 3:
                    f = 150f;
                    break;
                case 2:
                    f = 100f;
                    break;
                case 1:
                    f = 1f;
                    break;
                default:
                    f = 75f;
                    break;
            }

            return f;
        }

        private float getTrasXCurva()
        {
            float f = 0f;

            switch (cantCurvaSuaveActual)
            {
                case 5:
                    f = 2.65f;
                    break;
                case 4:
                    f = 7.8f;
                    break;
                case 3:
                    f = 10.5f;
                    break;
                case 2:
                    f = 7.3f;
                    break;
                case 1:
                    f = 2.2f;
                    break;
                default:
                    break;
            }

            return f;
        }

        public void UpdateTunel()
        {
            if (!this.tunelActivo)
            {
                // vars para tuneles
                // Generacion aleatoria de túneles sobre la pista
                int eleccionPista = elijoEntreTresProbabilidades(700, 10, 10);

                this.cantTunelActual = this.rnd.Next(31);
                if (this.cantTunelActual < 15)
                    this.cantTunelActual += 15;

                this.colorTunelActual = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));

                if (eleccionPista == 1)
                {
                    this.rutaTunelActual = "";
                    this.tunelActivo = false;
                    this.cantTunelActual = 0;
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
                    this.cantTunelActual = 0;
                }

            }
        }

        public void UpdateObstaculos()
        {
            
            if (!this.obstaculosActivos)
            {

                int generaObstaculos = elijoEntreTresProbabilidades(2, 90, 18);
                
                this.cantObsActual = this.rnd.Next(4) + 1;
                if (generaObstaculos == 1)
                {
                    this.obstaculosActivos = true;
                }
                else
                {
                    this.obstaculosActivos = false;
                    this.cantObsActual = 0;
                }
            }
        }

        public TgcMesh generarSegmentoPiso(bool soloPista)
        {
            TGCBox piso = new TGCBox();
            TGCVector3 Tamanio = new TGCVector3(30, 10, 60);
            piso.Size = Tamanio;

            TGCVector3 rot = new TGCVector3(0, 0, 0);
            rot.X = Geometry.DegreeToRadian(this.rotCurvaActual.X);
            if (cantCurvaSuaveActual != 0)
                rot.Y = Geometry.DegreeToRadian(this.rotCurvaActual.Y / curvar(cantCurvaSuaveActual));
            else
                rot.Y = Geometry.DegreeToRadian(this.rotCurvaActual.Y);
            rot.Z = Geometry.DegreeToRadian(this.rotCurvaActual.Z);

            // YAW = Y, va primero
            piso.Transform = TGCMatrix.Scaling(new TGCVector3(1, 1, 1.05f)) *
                             TGCMatrix.RotationYawPitchRoll(rot.Y, rot.X, rot.Z) *
                             TGCMatrix.Translation(this.posUltimaPieza);

            TgcTexture texturaPiso = TgcTexture.createTexture(D3DDevice.Instance.Device, MediaDir + "Textures\\segmento.jpg");
            piso.setTexture (texturaPiso);
            piso.updateValues();
            piso.AutoTransformEnable = false;

            TgcMesh pisoMesh = piso.ToMesh("piso");
                        
            pisoMesh.addDiffuseMap(texturaPiso);

            Effect effect = TGCShaders.Instance.LoadEffect(ShadersDir + "ShaderPiso.fx");            
            
            pisoMesh.Effect = effect;
            pisoMesh.Technique = "RenderScene";

            pisoMesh.AutoUpdateBoundingBox = false;
            pisoMesh.Position = this.posUltimaPieza;
            pisoMesh.Rotation = new TGCVector3(0, rot.Y, 0);

            if(!soloPista)
                agregoRecolectables(this.posUltimaPieza);

            return pisoMesh;
        }

        // buscar una funcion que de F(1)=-3 F(2)=-1.5 F(3)=1 F(4)=1.5 F(5)=3
        public float curvar(int x)
        {
            float y = 0;
            switch (x)
            {
                case 1:
                    y = 30f;
                    break;
                case 2:
                    y = 15f;
                    break;
                case 3:
                    y = 10f;
                    break;
                case 4:
                    y = 15f;
                    break;
                case 5:
                    y = 30f;
                    break;
            }

            return y;
        }

        void agregoRecolectables(TGCVector3 Posicion)
        {

            if (elijoEntreTresProbabilidades(5, 50, 49) == 1 && !obstaculosActivos)
            {
                Recolectable nuevoRecoletable = new Recolectable(MediaDir, Posicion);
                Recolectables.Add(nuevoRecoletable);
            }
        }
        
        int elijoEntreTresProbabilidades(int probA, int probB, int probC)
        {
            int probTotal = probA + probB + probC;
            int randomNumber = rnd.Next(probTotal);
            if (randomNumber < probA) return 1;
            else if (randomNumber < probA + probB) return 2;
            else return 3;
        }


        public void RenderTuneles(float time)
        {
            efectoTunel.SetValue("time", time);
            foreach (TgcMesh AuxMesh in this.SegmentosTunel)
            {              
                AuxMesh.Effect = efectoTunel;
                AuxMesh.Technique = "Deformacion2";
                AuxMesh.Render();
            }
        }

        void RenderPiso(TGCVector3 posicionLuzArbitraria)
        {
            foreach (TgcMesh AuxMesh in this.SegmentosPista)
            {
                AuxMesh.Effect.SetValue("lightPos", TGCVector3.Vector3ToVector4(posicionLuzArbitraria));
                AuxMesh.Render();
            }
        }

        Effect CargarEfecto(String nombreEfecto)
        {
            string compilationErrors;
            System.Console.WriteLine(ShadersDir + nombreEfecto);
            Effect effect = Effect.FromFile(D3DDevice.Instance.Device, this.ShadersDir + nombreEfecto, null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if(effect == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            return effect;
        }



        public void Render(TGCVector3 posicionCamara,TGCVector3 posicionLuzArbitraria,float elapsedTime)
        {
            time += elapsedTime;
            RenderPiso(posicionLuzArbitraria);
            RenderTuneles(time);

            foreach (Recolectable AuxRec in Recolectables)
            {
                AuxRec.Render(posicionCamara, posicionLuzArbitraria);
            }
           
            
            foreach (Obstaculo AuxMesh in this.Obstaculos)
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
                AuxMesh.Effect.Dispose();
                AuxMesh.Dispose();
            }

        }

    }
}
