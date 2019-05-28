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
        public Double scoreTemporal { get; set; } = 0;
        public Int32 level { get; set; } = 1;

        //Texts
        public TgcText2D ScoreText;
        public TgcText2D MultiplicadorText;
        public TgcText2D ScoreGanadoText;
        public TgcText2D RangoLevelText;
        public TgcText2D MultiplicadorFinalText;
        public TgcText2D LevelText;
        public static String font = "Arial Black";


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

            // Creo el texto del score total
            ScoreText = new TgcText2D();
            ScoreText.Color = Color.Aquamarine;
            ScoreText.Align = TgcText2D.TextAlign.LEFT;
            ScoreText.Position = new Point(50, 500);
            //ScoreText.Size = new Size(300, 100);
            ScoreText.changeFont(new Font(font, 23, FontStyle.Bold));
            ScoreText.Text = Score.ToString();

            // Texto del Multiplicador
            MultiplicadorText = new TgcText2D();
            MultiplicadorText.Color = Color.Aquamarine;
            MultiplicadorText.Align = TgcText2D.TextAlign.LEFT;
            MultiplicadorText.Position = new Point(50, 410);
            MultiplicadorText.Size = new Size(100, 600);
            MultiplicadorText.changeFont(new Font(font, 40, FontStyle.Bold));
            MultiplicadorText.Text = "x" + Multiplicador.ToString();

            //Score temporal
            ScoreGanadoText = new TgcText2D();
            ScoreGanadoText.Color = Color.Aquamarine;
            ScoreGanadoText.Align = TgcText2D.TextAlign.LEFT;
            ScoreGanadoText.Position = new Point(50, 500);
            ScoreGanadoText.changeFont(new Font(font, 23, FontStyle.Bold));

            // Rango
            RangoLevelText = new TgcText2D();
            RangoLevelText.Color = Color.Aquamarine;
            RangoLevelText.Align = TgcText2D.TextAlign.LEFT;
            RangoLevelText.Position = new Point(50, 500);
            //ScoreText.Size = new Size(300, 100);
            RangoLevelText.changeFont(new Font(font, 23, FontStyle.Bold));

            //Multiplicador final MultiplicadorFinalText
            MultiplicadorFinalText = new TgcText2D();
            MultiplicadorFinalText.Color = Color.Aquamarine;
            MultiplicadorFinalText.Align = TgcText2D.TextAlign.LEFT;
            MultiplicadorFinalText.Position = new Point(50, 500);
            MultiplicadorFinalText.changeFont(new Font(font, 23, FontStyle.Bold));

            // Level
            LevelText = new TgcText2D();
            LevelText.Color = Color.Aquamarine;
            LevelText.Align = TgcText2D.TextAlign.LEFT;
            LevelText.Position = new Point(50, 500);
            LevelText.changeFont(new Font(font, 23, FontStyle.Bold));

        }

        // Cada frame eleva el 
        public void ActualizarScore()
        {
            scoreTemporal += 1 * this.Multiplicador;
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

            return FinDeNivel();
        }

        private bool FinDeNivel()
        {
            if (AcumuladorEventos == 20)
            {
                score += scoreTemporal;

                ScoreText.Text = score.ToString();
                MultiplicadorText.Text = Multiplicador.ToString();
                ScoreGanadoText.Text = scoreTemporal.ToString();
                //RangoLevelText.Text = "Rank: " + obtenerRango(level, scoreTemporal);
                //MultiplicadorFinalText.Text = ;

                level++;

                LevelText.Text = "Level: " + level.ToString();

                Multiplicador = 1;
                AcumuladorAciertos = 0;
                AcumuladorEventos = 0;

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

        public void Render()
        {
            fondo.Render();
            ScoreText.render();
            MultiplicadorText.render();
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
