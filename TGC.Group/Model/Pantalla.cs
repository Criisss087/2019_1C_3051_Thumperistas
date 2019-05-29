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
        private Double[] Puntuaciones = new Double[4];
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
			LevelText = nuevoTexto(20,20,25); 
			
            ScoreText = nuevoTexto(20,20,25);         
			
			AddAciertosText = nuevoTexto(20,40,20); 
			AciertosText = nuevoTexto(80,40,20);			
			AddMultiplicadorText = nuevoTexto(20,50,20); 
			MultiplicadorText = nuevoTexto(80,50,20); 			
			AddFallosText = nuevoTexto(20,60,20); 
			FallosText = nuevoTexto(80,60,20); 
			AddDanioText = nuevoTexto(20,60,20); 
			DanioText = nuevoTexto(80,60,20); 
			
			RangoLevelText = nuevoTexto(20,40,20); 
			RankText = nuevoTexto(30,40,20); 
			
			CambioMultiplicadorText = nuevoTexto(200,20,25); 

        }
		
		private TgcText2D nuevoTexto(int x, int y, int tamanio)
		{
			TgcText2D text = new TgcText2D();
            text.Color = Color.White;
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
            scoreTemporal += 100 * multiplicador;

            if ((AcumuladorAciertos % 3 == 0) && (Multiplicador < 8))
                Multiplicador++;

            return FinDeNivel();
        }

        public bool Error()
        {
            Multiplicador = 1;
            AcumuladorAciertos = 0;
            AcumuladorEventos++;
			Danio = true;

            return FinDeNivel();
        }

        private bool FinDeNivel()
        {
            if (AcumuladorEventos == 20)
            {
                score += scoreTemporal;

                ScoreText.Text = score.ToString();
                
                level++;

                LevelText.Text = "Level: " + level.ToString();

                Multiplicador = 1;
                AcumuladorAciertos = 0;
                AcumuladorEventos = 0;
				Danio = false;

                if (level == 6)
                    FinDeJuego();
				
				return true;
            }
			
			return false;
        }

        private char obtenerRango(Int32 level, Double scoreTemporal)
        {
            char R = '0';

            Puntuaciones[level + 1] = scoreTemporal;
            if (AcumuladorAciertos < 5)
                R = 'D';
            else if (AcumuladorAciertos < 10)
                R = 'C';
            else if (AcumuladorAciertos < 15)
                R = 'B';
            else if (AcumuladorAciertos < 20)
                R = 'A';
            else
                R = 'S';

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

        public void Render(FaseTexto fase)
        {
            fondo.Render();
			
			switch(fase)
			{
				case FaseTexto.Level:
			        LevelText.render();
					break;
				case FaseTexto.Resultados:
					ScoreText.render();
					AddAciertosText.render();
					AciertosText.render();
					AddMultiplicadorText.render();
					MultiplicadorText.render();
					AddFallosText.render();
					FallosText.render();
					AddDanioText.render();
					DanioText.render();				
					break;
				case FaseTexto.Rank:
					ScoreText.render();
					RangoLevelText.render();
					RankText.render();
					break;
				case FaseTexto.CambioMult:
				    CambioMultiplicadorText.render();
					break;
				default:
					break;
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
                ScoreText.Text = value.ToString();
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
                MultiplicadorText.Text = "x" + value.ToString();
            }

        }


    }
}
