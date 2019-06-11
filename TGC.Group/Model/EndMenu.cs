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

        private Drawer2D drawer;
        private CustomSprite fondo;
        private CustomSprite bloqueTexto;
        private TgcText2D playText;
        private TgcText2D exitText;

        private bool isStart = false;
        private bool isExit = false;

        public enum Fase
        {
            Start = 0,
            Exit = 1
        }

        private Fase FaseSeleccionada = Fase.Start;

        public EndMenu(GameModel _gameModel, Pantalla _pantalla)
        {
            GameModel = _gameModel;
            Pantalla = _pantalla;

            drawer = new Drawer2D();
            fondo = new CustomSprite();
            fondo.Bitmap = new CustomBitmap(GameModel.MediaDir + "Screens\\purple_tentacles.png", D3DDevice.Instance.Device);

            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;
            Size maxSize = new Size(1920, 1017);

            var scalingFactorX = (float)screenWidth / (float)fondo.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)fondo.Bitmap.Height;
            fondo.Scaling = new TGCVector2(scalingFactorX, scalingFactorY);
            var menuFont = new Font("Arial Black", 30, FontStyle.Bold);

            // Tengo que mover esto al lugar vacio de la imagen
            bloqueTexto = new CustomSprite();
            bloqueTexto.Bitmap = new CustomBitmap(GameModel.MediaDir + "Textures\\bloqueTexto.png", D3DDevice.Instance.Device);
            bloqueTexto.Color = Color.FromArgb(188, 0, 0, 0);
            bloqueTexto.Scaling = new TGCVector2(1, .1f);
            bloqueTexto.Position = new TGCVector2(
                (screenWidth - bloqueTexto.Bitmap.Width * bloqueTexto.Scaling.X) / 2,
                (screenHeight - bloqueTexto.Bitmap.Height * bloqueTexto.Scaling.Y) / 2
            );

            bloqueTexto.Position = new TGCVector2(
                bloqueTexto.Position.X,
                screenHeight * (3f / 4)
            );

            //Play
            playText = new TgcText2D();
            playText.Text = "Restart";
            playText.Color = Color.Silver;
            playText.Position = new Point(-(int)(screenWidth * 0.194f), (int)(screenHeight * 0.52f));
            playText.changeFont(menuFont);

            //Exit
            exitText = new TgcText2D();
            exitText.Text = "Exit";
            exitText.Color = Color.Silver;
            exitText.Position = new Point(-(int)(screenWidth * 0.194f), (int)(screenHeight * 0.52f) + (int)(0.147f * screenHeight));
            exitText.changeFont(menuFont);

            this.GameModel.Camara = new TgcThirdPersonCamera();

        }

        public void Update()
        {
            var Input = GameModel.Input;

            if (Input.keyPressed(Key.DownArrow) || Input.keyPressed(Key.UpArrow))
            {
                FaseSeleccionada = (FaseSeleccionada == Fase.Start) ? Fase.Exit : Fase.Start;
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

            drawer.EndDrawSprite();

            playText.render();
            exitText.render();


            if (isStart)
            {
                GameModel.GameState = new StartGame(GameModel);
                this.Dispose();
            }
        }

        public void Dispose()
        {
            fondo.Dispose();
            bloqueTexto.Dispose();
            playText.Dispose();
            exitText.Dispose();
        }

    }
}
