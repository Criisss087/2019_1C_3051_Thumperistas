using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Terrain;
using TGC.Core.Text;
using TGC.Examples.Camara;

namespace TGC.Group.Model
{
    public class EndMenu : GameState
    {

        private GameModel GameModel;
        private Pantalla Pantalla;
        private Reproductor Reproductor;

        private Drawer2D drawer;
        private CustomSprite fondo;
        private CustomSprite bloqueTexto;
        private CustomSprite bloqueMenu;
        private TgcText2D resultText;
        private TgcText2D playText;
        private TgcText2D exitText;

        private TgcText2D levelText;
        private TgcText2D scoreText;
        private TgcText2D rankText;

        private int screenWidth;
        private int screenHeight;
        private bool isStart = false;
        private bool isExit = false;

        public enum Fase
        {
            Start = 0,
            Exit = 1
        }

        private Fase FaseSeleccionada = Fase.Start;

        public EndMenu(GameModel _gameModel, Pantalla _pantalla, bool win)
        {
            GameModel = _gameModel;
            Pantalla = _pantalla;

            Reproductor = new Reproductor(GameModel.MediaDir, GameModel.DirectSound);
            drawer = new Drawer2D();
            fondo = new CustomSprite();
            fondo.Bitmap = new CustomBitmap(GameModel.MediaDir + "Screens\\purple_tentacles.png", D3DDevice.Instance.Device);

            screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            screenWidth = D3DDevice.Instance.Device.Viewport.Width;

            var scalingFactorX = (float)screenWidth / (float)fondo.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)fondo.Bitmap.Height;
            fondo.Scaling = new TGCVector2(scalingFactorX, scalingFactorY);
            var menuFont = new Font("Arial Black", 30, FontStyle.Bold);

            bloqueTexto = new CustomSprite();
            bloqueTexto.Bitmap = new CustomBitmap(GameModel.MediaDir + "Textures\\bloqueTexto.png", D3DDevice.Instance.Device);
            bloqueTexto.Color = Color.FromArgb(200, 0, 0, 0);
            bloqueTexto.Scaling = new TGCVector2(4.5f, 4f);
            bloqueTexto.Position = new TGCVector2(
                (screenWidth - bloqueTexto.Bitmap.Width * bloqueTexto.Scaling.X) * 0.88f,
                (screenHeight - bloqueTexto.Bitmap.Height * bloqueTexto.Scaling.Y) / 2
            );
            
            bloqueMenu = new CustomSprite();
            bloqueMenu.Bitmap = new CustomBitmap(GameModel.MediaDir + "Textures\\bloqueTexto.png", D3DDevice.Instance.Device);
            bloqueMenu.Color = Color.FromArgb(200, 0, 0, 0);
            bloqueMenu.Scaling = new TGCVector2(2.5f, 1.3f);
            bloqueMenu.Position = new TGCVector2(
                screenWidth  * 0.105f,
                screenHeight * 0.28f
            );

            //Result
            resultText = new TgcText2D();
            if(win)
            {
                resultText.Text = "¡You Win!";
                resultText.Color = Color.Yellow;
            }
            else
            {
                resultText.Text = "Game Over";
                resultText.Color = Color.Red;
            }                        
            resultText.Position = new Point(-(int)(screenWidth * 0.28f), (int)(screenHeight * 0.15f));
            resultText.changeFont(new Font("Arial Black", 40, FontStyle.Bold));

            //Restart
            playText = new TgcText2D();
            playText.Text = "Restart";
            playText.Color = Color.Silver;
            playText.Position = new Point(-(int)(screenWidth * 0.28f), (int)(screenHeight * 0.3f));
            playText.changeFont(menuFont);

            //Exit
            exitText = new TgcText2D();
            exitText.Text = "Exit";
            exitText.Color = Color.Silver;
            exitText.Position = new Point(-(int)(screenWidth * 0.28f), (int)(screenHeight * 0.3f) + (int)(0.1f * screenHeight));
            exitText.changeFont(menuFont);

            var tableFont = new Font("Arial Black", 17, FontStyle.Bold);

            levelText = new TgcText2D();
            levelText.changeFont(tableFont);
            scoreText = new TgcText2D();
            scoreText.changeFont(tableFont);
            rankText = new TgcText2D();
            rankText.changeFont(tableFont);
            levelText.Color = Color.Silver;
            scoreText.Color = Color.Silver;
            rankText.Color = Color.Silver;

            this.GameModel.Camara = new TgcThirdPersonCamera();

        }

        public void Update()
        {
            var Input = GameModel.Input;

            if (Input.keyPressed(Key.DownArrow) || Input.keyPressed(Key.UpArrow))
            {
                FaseSeleccionada = (FaseSeleccionada == Fase.Start) ? Fase.Exit : Fase.Start;
                Reproductor.Recolectar();
            }

            if (Input.keyPressed(Key.Return))
            {
                switch (FaseSeleccionada)
                {
                    case Fase.Start:
                        isStart = true;
                        break;
                    case Fase.Exit:
                        isExit = true;
                        break;
                    default:
                        break;
                }
            }

            switch (FaseSeleccionada)
            {
                case Fase.Start:
                    playText.Color = Color.White;
                    exitText.Color = Color.Silver;
                    break;
                case Fase.Exit:
                    playText.Color = Color.Silver;
                    exitText.Color = Color.White;
                    break;
                default:
                    break;
            }
        }

        public void Render()
        {
            if (isExit)
            {
                GameModel.Exit();
                return;
            }

            drawer.BeginDrawSprite();

            drawer.DrawSprite(fondo);
            drawer.DrawSprite(bloqueTexto);
            drawer.DrawSprite(bloqueMenu);

            drawer.EndDrawSprite();

            // Dibuja la tabla con los mismos textos
            scoreText.Text = "Score";
            rankText.Text = "Rank";
            rankText.Color = Color.White;

            scoreText.Position = new Point((int)(screenWidth * 0.2f), (int)(screenHeight * 0.18f));
            rankText.Position = new Point((int)(screenWidth * 0.35f), (int)(screenHeight * 0.18f));
            scoreText.render();
            rankText.render();

            float heightAdd = 0f;

            for (int i = 0; i < Pantalla.Puntuaciones.Count ;i++)
            {
                scoreText.Text = Pantalla.Puntuaciones[Pantalla.Puntuaciones.Keys.ElementAt(i)].ToString();
                rankText.Text = Pantalla.Rangos[Pantalla.Rangos.Keys.ElementAt(i)].ToString();

                rankText.Color = getRankColor(rankText);

                levelText.Text = "Level 1-" + Pantalla.Puntuaciones.Keys.ElementAt(i).ToString();

                heightAdd += 0.09f;

                scoreText.Position = new Point((int)(screenWidth * 0.2f), (int)(screenHeight * 0.18f) + (int)(screenHeight * heightAdd));
                rankText.Position = new Point((int)(screenWidth * 0.35f), (int)(screenHeight * 0.18f) + (int)(screenHeight * heightAdd));
                levelText.Position = new Point((int)(screenWidth * 0.083f), (int)(screenHeight * 0.18f) + (int)(screenHeight * heightAdd));

                scoreText.render();
                rankText.render();
                levelText.render();

            }

            double sum = 0;

            foreach(KeyValuePair<int, double> item in Pantalla.Puntuaciones)
            {
                sum += item.Value;
            }

            levelText.Text = "TOTAL";
            scoreText.Text = sum.ToString();

            heightAdd += 0.09f;

            scoreText.Position = new Point((int)(screenWidth * 0.2f), (int)(screenHeight * 0.18f) + (int)(screenHeight * heightAdd));
            levelText.Position = new Point((int)(screenWidth * 0.083f), (int)(screenHeight * 0.18f) + (int)(screenHeight * heightAdd));
            scoreText.render();
            levelText.render();

            resultText.render();
            playText.render();
            exitText.render();


            if (isStart)
            {
                Reproductor.Explosion();
                GameModel.GameState = new StartGame(GameModel);
                this.Dispose();
            }
        }

        private Color getRankColor(TgcText2D rankText)
        {
            Color returnColor = Color.White;

            if(rankText.Text == "S")
                returnColor = Color.Yellow;

            if (rankText.Text == "D")
                returnColor = Color.Red;

            return returnColor;
        }

        public void Dispose()
        {
            fondo.Dispose();
            bloqueTexto.Dispose();
            playText.Dispose();
            exitText.Dispose();
            Reproductor.Dispose();

        }

    }
}
