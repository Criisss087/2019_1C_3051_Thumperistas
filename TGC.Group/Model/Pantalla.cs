using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private Double score = 0;
        private Int32 multiplicador = 1;

        public TgcText2D ScoreText;
        public TgcText2D MultiplicadorText;

        public Pantalla(String _mediaDir)
        {
            // Creo el texto del marcador
            ScoreText = new TgcText2D();
            ScoreText.Color = Color.Aquamarine;
            ScoreText.Align = TgcText2D.TextAlign.LEFT;
            ScoreText.Position = new Point(50, 500);
            //ScoreText.Size = new Size(300, 100);
            ScoreText.changeFont(new Font("Arial", 18, FontStyle.Bold));
            ScoreText.Text = Score.ToString();

            // Texto del Multiplicador
            MultiplicadorText = new TgcText2D();
            MultiplicadorText.Color = Color.Aquamarine;
            MultiplicadorText.Align = TgcText2D.TextAlign.CENTER;
            MultiplicadorText.Position = new Point(50, 510);
            MultiplicadorText.Size = new Size(30,100);
            MultiplicadorText.changeFont(new Font("Arial", 50, FontStyle.Bold));
            MultiplicadorText.Text = Multiplicador.ToString();
        }

        public void Render()
        {
            ScoreText.render();
            MultiplicadorText.render();
        }

        public void Dispose()
        {
            ScoreText.Dispose();
            MultiplicadorText.Dispose();
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
                MultiplicadorText.Text = value.ToString();
            }

        }


    }
}
