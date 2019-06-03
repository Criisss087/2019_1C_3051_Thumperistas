using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.Terrain;
using TGC.Core.Text;

namespace TGC.Group.Model
{
    /// <summary>
    ///     En esta clase vamos a implementar todos los objetos 2D tipo textos o imágenes que se posicionen
    ///     en la pantalla a medida que el nivel avance, sea score, multiplicador, teclas a presionar, etc
    /// </summary>
    class Pantalla
    {
        //Datos internos
        private String texturesPath;
        private TgcSkyBox fondo;
        private Double score = 0;
        private Dictionary<int, Double> Puntuaciones = new Dictionary<int, Double>();

        public Int32 multiplicador = 1;
        public Int32 AcumuladorAciertos { get; set; } = 0;
        public Int32 AcumuladorEventos { get; set; } = 0;
		public Int32 AcumuladorPoder { get; set; } = 0;
		public bool Danio { get; set; }
        public Double scoreTemporal { get; set; } = 0;
        public Int32 level { get; set; } = 1;

        //Texts
		//Fase 1
        public TgcText2D LevelText;		
		//Fase 2
		public TgcText2D ScoreText;
		public TgcText2D AddAciertosText;
		public TgcText2D AciertosText;
		public TgcText2D AddMultiplicadorText;
		public TgcText2D MultiplicadorText;		
		public TgcText2D AddFallosText;
		public TgcText2D FallosText;
		public TgcText2D AddDanioText;
		public TgcText2D DanioText;		
		// Fase 3
		public TgcText2D RangoLevelText;
		public TgcText2D RankText;
		// Fase 4
        public TgcText2D CambioMultiplicadorText;
		
        public static String font = "Arial Black";
		private static Int32 PuntosAcierto = 100;
		private static Int32 PuntosSinFallos = 1000;
		private static Int32 PuntosSinDanio = 1000;

        private static int tamanioSubtitulo = 25;
        private static int tamanioDetalles = 12;

        public enum FaseTexto
        {
            Nada = 0,
            Level = 1,
            Resultados = 2,
			Rank = 3,
			CambioMult = 4
        }

        public Pantalla(String _mediaDir)
        {
            //Crear SkyBox
            fondo = new TgcSkyBox();
            fondo.Center = new TGCVector3(0, 500, 0);
            fondo.Size = new TGCVector3(10000, 10000, 10000);
            texturesPath = _mediaDir + "/Thumper/Textures/SkyBox1/";
            fondo.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "1.jpg"); // "lostatseaday_up.jpg");
            fondo.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "1.jpg"); //"lostatseaday_dn.jpg");
            fondo.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "1.jpg"); //"lostatseaday_lf.jpg");
            fondo.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "1.jpg"); //"lostatseaday_rt.jpg");
            fondo.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "1.jpg"); //"lostatseaday_bk.jpg");
            fondo.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "1.jpg"); //"lostatseaday_ft.jpg");
            fondo.Init();

            // Textos
			LevelText = nuevoTexto(20,20, tamanioSubtitulo, Color.White); 
			
            ScoreText = nuevoTexto(20,20, tamanioSubtitulo, Color.White);         
			
			AddAciertosText = nuevoTexto(20,75, tamanioDetalles, Color.Aquamarine); 
			AciertosText = nuevoTexto(120,75, tamanioDetalles, Color.White);			
			AddMultiplicadorText = nuevoTexto(20,100, tamanioDetalles, Color.Aquamarine); 
			MultiplicadorText = nuevoTexto(120,100, tamanioDetalles, Color.White); 			
			AddFallosText = nuevoTexto(20,125, tamanioDetalles, Color.Aquamarine); 
			FallosText = nuevoTexto(120,125, tamanioDetalles, Color.White); 
			AddDanioText = nuevoTexto(20,150, tamanioDetalles, Color.Aquamarine); 
			DanioText = nuevoTexto(120,150, tamanioDetalles, Color.White);            
            
            MultiplicadorText.Text = "Multiplicador";
            AddFallosText.Text = "+" + PuntosSinFallos.ToString();
            FallosText.Text = "Sin Fallos";
            AddDanioText.Text = "+" + PuntosSinDanio.ToString();
            DanioText.Text = "Sin Daño";
            
            RangoLevelText = nuevoTexto(20,65, 20, Color.Yellow); 
			RankText = nuevoTexto(50,75, tamanioDetalles, Color.White);

            RangoLevelText.Text = "S";
            RankText.Text = "Rank";

            CambioMultiplicadorText = nuevoTexto(1200,100, tamanioSubtitulo, Color.Red);
            CambioMultiplicadorText.Text = "x1";
        }
		
		private TgcText2D nuevoTexto(int x, int y, int tamanio, Color color)
		{
			TgcText2D text = new TgcText2D();
            text.Color = color;
            text.Align = TgcText2D.TextAlign.LEFT;
            text.Position = new Point(x, y);
            text.Size = new Size(300, 100);
            text.changeFont(new Font(font, tamanio, FontStyle.Bold));
            			
			return text;
		}

        public bool Acierto()
        {
            AcumuladorAciertos++;
            AcumuladorEventos++;
			AcumuladorPoder++;
            scoreTemporal += 100;

            if ((AcumuladorAciertos % 5 == 0) && (Multiplicador < 8))
            {
                CambioMultiplicadorText.Color = Color.White;
                Multiplicador++;
            }
                

            return FinDeNivel();
        }

        public bool Error()
        {
            CambioMultiplicadorText.Color = Color.Red;

            if(Multiplicador!= 1)
                Multiplicador = 1;  

            AcumuladorAciertos = 0;
            AcumuladorEventos++;
            
            return FinDeNivel();
        }

        private bool FinDeNivel()
        {
            if (AcumuladorEventos > 20)
            {
                //score += scoreTemporal;
                //ScoreText.Text = score.ToString();
                AcumuladorEventos = 0;
                level++;
                LevelText.Text = "Level: " + level.ToString();			
				return true;
            }
			
			return false;
        }

        private String obtenerRango(Int32 level, Double scoreTemporal)
        {
            String R = "0";

            Puntuaciones[level + 1] = scoreTemporal;
            if (AcumuladorAciertos < 5)
                R = "D";
            else if (AcumuladorAciertos < 10)
                R = "C";
            else if (AcumuladorAciertos < 15)
                R = "B";
            else if (AcumuladorAciertos < 20)
                R = "A";
            else
                R = "S";

            return R;

        }

        private void FinDeJuego()
        {
            //Implementar
        }

        public void Update(TGCVector3 posicionCamara)
        {
            fondo.Center = posicionCamara;
        }

        public void SetTextFinDeNivel()
        {
            ScoreText.Text = Score.ToString();
            AddAciertosText.Text = scoreTemporal.ToString();
            AciertosText.Text = "x" + AcumuladorAciertos.ToString();
            AddMultiplicadorText.Text = "x" + Multiplicador.ToString();

            scoreTemporal += AcumuladorAciertos * multiplicador;

            if (AcumuladorAciertos == 20)
            {
                scoreTemporal += PuntosSinFallos;
            }

            if (!Danio)
            {
                scoreTemporal += PuntosSinDanio;
            }

            Puntuaciones.Add(level+1,scoreTemporal);
            Score += scoreTemporal;

            scoreTemporal = 0;
            AcumuladorAciertos = 0;
        }

        public void Render(float ElapsedTime)
        {
            fondo.Render();

            if (!Temporizadores.textScoreTotal.update(ElapsedTime))
            {
                ScoreText.render();
                AddAciertosText.render();
                AciertosText.render();
                AddMultiplicadorText.render();
                MultiplicadorText.render();

                if (AcumuladorAciertos >= 20)
                {
                    AddFallosText.render();
                    FallosText.render();
                }

                if (!Danio)
                {
                    AddDanioText.render();
                    DanioText.render();
                }
                
            }

            if (!Temporizadores.textRank.update(ElapsedTime))
            {
                ScoreText.Text = Score.ToString();
                ScoreText.render();
                RangoLevelText.render();
                RankText.render();
            }

            if (!Temporizadores.textNextLvl.update(ElapsedTime))
            {
                LevelText.Text = "Level 1-" + level.ToString();
                LevelText.render();
            }
            
            if (!Temporizadores.textCambioMult.update(ElapsedTime))
            {
                CambioMultiplicadorText.render();
            }
            
        }

        public void Dispose()
        {
            ScoreText.Dispose();
            MultiplicadorText.Dispose();
            fondo.Dispose();
        }

        public Double Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
                //ScoreText.Text = value.ToString();
            }

        }

        public Int32 Multiplicador
        {
            get
            {
                return multiplicador;
            }
            set
            {
                multiplicador = value;
                CambioMultiplicadorText.Text = "x" + value.ToString();
                Temporizadores.textCambioMult.reset();
            }

        }


    }
}
