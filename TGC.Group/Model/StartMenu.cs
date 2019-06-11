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
    public class StartMenu : GameState
    {

        private GameModel GameModel;

        private Reproductor Reproductor;
        private Drawer2D drawer;
        private CustomSprite fondo;
        private CustomSprite bloqueTexto;
        private TgcText2D playText;
        private TgcText2D exitText;

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

        public StartMenu(GameModel _gameModel)
        {
            GameModel = _gameModel;

            Reproductor = new Reproductor(GameModel.MediaDir, GameModel.DirectSound);

            drawer = new Drawer2D();
            fondo = new CustomSprite();
            fondo.Bitmap = new CustomBitmap(GameModel.MediaDir + "Screens\\thumper_cover_loop.png", D3DDevice.Instance.Device);

            screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            screenWidth = D3DDevice.Instance.Device.Viewport.Width;
            var scalingFactorX = (float)screenWidth / (float)fondo.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)fondo.Bitmap.Height;
            fondo.Scaling = new TGCVector2(scalingFactorX, scalingFactorY);
            var menuFont = new Font("Arial Black", 30, FontStyle.Bold);

            bloqueTexto = new CustomSprite();
            bloqueTexto.Bitmap = new CustomBitmap(GameModel.MediaDir + "Textures\\bloqueTexto.png", D3DDevice.Instance.Device);
            bloqueTexto.Color = Color.FromArgb(200, 0, 0, 0);
            bloqueTexto.Scaling = new TGCVector2(2f, 1.3f);
            bloqueTexto.Position = new TGCVector2( 
                (screenWidth - bloqueTexto.Bitmap.Width * bloqueTexto.Scaling.X) /2, // Centro en X
                screenHeight * 0.575f
            );

            //Play
            playText = new TgcText2D();
            playText.Text = "Play";
            playText.Color = Color.Silver;
            playText.Position = new Point(0, (int)(screenHeight * 0.6f));
            playText.changeFont(menuFont);

            //Exit
            exitText = new TgcText2D();
            exitText.Text = "Exit";
            exitText.Color = Color.Silver;
            exitText.Position = new Point(0, (int)(screenHeight * 0.6f) + (int)(0.1f * screenHeight));
            exitText.changeFont(menuFont);

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

            drawer.EndDrawSprite();

            playText.render();
            exitText.render();


            if (isStart)
            {
                Reproductor.Explosion();
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
