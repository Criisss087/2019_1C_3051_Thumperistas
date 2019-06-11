using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using System;
using System.Collections.Generic;
using TGC.Examples.Camara;
using TGC.Core.Sound;
using TGC.Core.Collision;
using Microsoft.DirectX.Direct3D;
using TGC.Core.Terrain;
using System.Threading;
using TGC.Core.Interpolation;
using TGC.Core.Utils;
using TGC.Core.Text;

namespace TGC.Group.Model
{

    public class StartGame : GameState
    {
        //Atributos Globales

        private GameModel GameModel;

        private bool BoundingBox { get; set; }
        private bool help { get; set; }
        private TgcThirdPersonCamera camaraInterna;
        private Beetle Beetle;
        private Pista PistaNivel;
        private Pantalla Pantalla;
        private Reproductor Reproductor;
        private Disparo Disparo;
        private Pantalla.FaseTexto FaseTexto { get; set; } = Pantalla.FaseTexto.Nada;
        private TgcText2D pauseText;

        private bool applyMovement { get; set; }
        private bool disparoActivo { get; set; }
        private bool curvaActiva { get; set; }
        private bool recolectableActivo { get; set; }
        private bool finDeNivel { get; set; }
        private bool finDeJuego { get; set; }
        private bool soloPista { get; set; }
        private bool pausa { get; set; } = false;
        private bool isWin = false;

        public float posX = 0, posY = 0;
        public TGCVector3 posicionFinal = new TGCVector3(0, 0, 0);
        private Random rnd = new Random();

        public StartGame(GameModel _gameModel)
        {
            GameModel = _gameModel;

            //Instancio el reproductores sonido
            Reproductor = new Reproductor(GameModel.MediaDir, GameModel.DirectSound);
            Pantalla = new Pantalla(GameModel.MediaDir);
            Pantalla.Score = 0;
            Pantalla.scoreTemporal = 0;
            Beetle = new Beetle(GameModel.MediaDir, GameModel.ShadersDir);
            PistaNivel = new Pista(GameModel.MediaDir, GameModel.ShadersDir);
            Temporizadores.Init();

            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            pauseText = new TgcText2D();
            pauseText.Color = Color.White;
            pauseText.Position = new Point(0, (int)(screenHeight * 0.5f));
            pauseText.changeFont(new System.Drawing.Font("Arial Black", 30, FontStyle.Bold));
            pauseText.Text = "Pause";

            Reproductor.ReproducirLevelPrincipal();
            camaraInterna = new TgcThirdPersonCamera(Beetle.position, 20f, -100f);
            GameModel.Camara = camaraInterna;

            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();
        }

        public void Update()
        {
            UpdateTemporizadores();

            var Input = GameModel.Input;

            if (!finDeJuego)
            {
                // Cuando llego al final de la pista, se actualiza, el rango de vision es alrededor de 2500 Z
                if (PistaNivel.posUltimaPieza.Z - Beetle.position.Z <= 2500)
                {
                    if (finDeNivel)
                    {
                        Pantalla.SetTextFinDeNivel();
                        Temporizadores.finDeNivel.reset();
                        soloPista = true;
                        finDeNivel = false;
                    }

                    if (!soloPista)
                    {
                        PistaNivel.UpdateObstaculos();
                        PistaNivel.UpdateTunel();
                        PistaNivel.UpdateCurvaSuave();
                    }
                    PistaNivel.UpdatePista(soloPista);
                }

                //Capturar Input teclado
                if (Input.keyPressed(Key.F))
                {
                    BoundingBox = !BoundingBox;
                }

                if (Input.keyPressed(Key.D) && Pantalla.AcumuladorPoder > 10)
                {
                    Disparo = new Disparo(GameModel.MediaDir, Beetle.position);
                    Reproductor.Disparar();
                    Pantalla.AcumuladorPoder = 0;
                    Pantalla.AcumuladorDisparos += 1;
                    disparoActivo = true;
                }

                if (Input.keyPressed(Key.H))
                    help = !help;

                Colisiones();
                Beetle.Update(Input, GameModel.ElapsedTime);


                // Deteccion de curva INTENTAR MEJORAR Y DELEGAR
                foreach (TgcMesh box2 in PistaNivel.SegmentosPista)
                {
                    //Reviso si el beetle colisiona con algun elemento de la pista  
                    if (TgcCollisionUtils.testObbAABB(Beetle.colliderPista, box2.BoundingBox))
                    {
                        // Si no hay movimiento en X activo, capturo la posicion final a la que debe ir el beetle
                        if (!applyMovement)
                        {
                            // Color para detectar la colision, testing
                            //box2.setColor(Color.Red);
                            posX = box2.Position.X - Beetle.position.X;
                            posY = box2.Position.Y - Beetle.position.Y + 8;
                            if (posX != 0 || posY != 0)
                            {
                                applyMovement = true;
                                posicionFinal = box2.Position;
                            }
                        }

                        //Si esta en una curva
                        if (box2.Rotation.Y != 0f
                        && !curvaActiva)
                        {
                            AccionesEnCurva(box2.Rotation.Y);
                        }


                    }
                    else
                    {
                        // Color para detectar la colision, testing
                        //box2.setColor(Color.Blue);
                    }
                }

                // Mantiene al Beetle en la pista
                if (applyMovement)
                {
                    //Ver si queda algo de distancia para mover
                    var posDiff = new TGCVector3(posicionFinal.X, 0f, 0f) - new TGCVector3(Beetle.position.X, 0f, 0f);

                    var posDiffLength = posDiff.LengthSq();
                    //Si esta a menos de 1 asumo que esta en la misma posicion
                    if (posDiffLength > 1)
                    {
                        //Intento mover el beetle interpolando por la velocidad
                        var currentVelocity = Beetle.VELOCIDADX * GameModel.ElapsedTime;
                        posDiff.Normalize();
                        posDiff.Multiply(currentVelocity);

                        //Ajustar cuando llegamos al final del recorrido
                        var newPos = Beetle.position + posDiff;
                        if (posDiff.LengthSq() > posDiffLength)
                        {
                            newPos = posicionFinal;
                        }

                        posX = posDiff.X;
                        //posY = posDiff.Y;

                    }
                    //Se acabo el movimiento
                    else
                    {
                        posX = 0;
                        posY = 0;
                        applyMovement = false;
                    }
                }

                //muevo beetle para adelante
                PistaNivel.posActual = Beetle.Avanza(GameModel.ElapsedTime, posX, posY);

                if (disparoActivo)
                {
                    var dist = Disparo.Avanza(GameModel.ElapsedTime, posX, posY);
                    if (dist.Z - Beetle.position.Z > 2500)
                    {
                        Reproductor.Explosion();
                        disparoActivo = false;
                    }
                }

                camaraInterna.Target = Beetle.position;
                Pantalla.Update(camaraInterna.Position);

                if (Input.keyPressed(Key.Escape))
                {
                    pausa = !pausa;
                    

                    if (pausa)
                    {
                        Beetle.speed = 0f;
                        //Reproductor.pausarReproduccion();
                    }
                    else
                    {
                        Beetle.speed = Beetle.VELOCIDAD;
                        //Reproductor.reanudarReproduccion();
                    }

                }

            }
            else
            {
                GameModel.GameState = new EndMenu(GameModel, Pantalla, isWin);
                this.Dispose();
            }

        }

        public void Render()
        {
            if (help)
            {
                var DrawText = GameModel.DrawText;

                //Dibuja un texto por pantalla
                DrawText.drawText("Con la tecla F se dibuja el bounding box.", 1000, 20, Color.OrangeRed);
                DrawText.drawText("Posicion actual del jugador: " + TGCVector3.PrintVector3(Beetle.position), 1000, 30, Color.OrangeRed);
                DrawText.drawText("Posicion actual ultima pieza: " + TGCVector3.PrintVector3(PistaNivel.posUltimaPieza), 1000, 40, Color.OrangeRed);
                DrawText.drawText("Cant Aciertos: " + (Pantalla.AcumuladorAciertos), 1000, 50, Color.OrangeRed);
                DrawText.drawText("Cant Eventos: " + (Pantalla.AcumuladorEventos), 1000, 60, Color.OrangeRed);
                DrawText.drawText("Cant para poder: " + Pantalla.AcumuladorPoder, 1000, 70, Color.OrangeRed);
                DrawText.drawText("Nivel: " + Pantalla.level, 1000, 80, Color.OrangeRed);
                DrawText.drawText("Inmunidad temporal: " + Beetle.inmunidadTemp.ToString(), 1000, 90, Color.OrangeRed);
                DrawText.drawText("Escudo: " + (Beetle.escudo.ToString()), 1000, 100, Color.OrangeRed);
                DrawText.drawText("GodMode: " + (Beetle.godMode.ToString()), 1000, 110, Color.OrangeRed);
                DrawText.drawText("Fin de Nivel: " + finDeNivel, 1000, 120, Color.OrangeRed);
                DrawText.drawText("Score total: " + Pantalla.Score, 1000, 130, Color.OrangeRed);
                DrawText.drawText("Multiplcador: " + Pantalla.Multiplicador, 1000, 140, Color.OrangeRed);
                DrawText.drawText("SoloPista: " + soloPista, 1000, 150, Color.OrangeRed);
                DrawText.drawText("Acum Disparo: " + Pantalla.AcumuladorDisparos, 1000, 160, Color.OrangeRed);
            }

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                PistaNivel.BoundingBoxRender();
                foreach (var mesh in Beetle.beetle.Meshes)
                {
                    mesh.BoundingBox.Render();
                }
            }

            if (!finDeJuego)
            {
                if (pausa)
                {
                    pauseText.render();
                }
                else
                {
                    Beetle.Render(GameModel.ElapsedTime, Pantalla.AcumuladorPoder, camaraInterna.Position);
                }
            }

            PistaNivel.Render(GameModel.Camara.Position, Beetle.position + TGCVector3.Up * 20);
            Pantalla.Render(GameModel.ElapsedTime);

            if (disparoActivo)
                Disparo.Render(GameModel.ElapsedTime, GameModel.Camara.Position, Beetle.position);

        }

        public void Dispose()
        {
            PistaNivel.Dispose();
            Beetle.Dispose();
            Pantalla.Dispose();
            Reproductor.Dispose();
            pauseText.Dispose();
        }

        private void Colisiones()
        {
            Beetle.TipoColision col;

            // Colision con recolectable 
            Recolectable recolectableColisionado = new Recolectable(GameModel.MediaDir, TGCVector3.One);

            col = Beetle.ColisionandoConRecolectable(PistaNivel.Recolectables, ref recolectableColisionado);

            if (col == Beetle.TipoColision.Colision)
            {
                if (Beetle.godMode || (GameModel.Input.keyPressed(Key.Space)))
                {
                    Reproductor.Recolectar();

                    // 1/10 en recuperar escudo al colectar
                    if (this.rnd.Next(10) == 1)
                    {
                        Reproductor.GanarEscudo();
                        Beetle.GanarEscudo();
                    }

                    PistaNivel.Recolectables.Remove(recolectableColisionado);
                    finDeNivel = Pantalla.Acierto();
                }

            }
            else if (col == Beetle.TipoColision.Error)
            {
                if (!recolectableActivo)
                {
                    Reproductor.NoRecolectar();
                    PistaNivel.Recolectables.Remove(recolectableColisionado);
                    finDeNivel = Pantalla.Error();
                    Temporizadores.recolectableOk.reset();
                }
            }


            // Colision con obstaculo            
            Obstaculo objetoColisionado = new Obstaculo(TGCVector3.One);
            col = Beetle.ColisionandoConObstaculo(PistaNivel.Obstaculos, ref objetoColisionado);
            if (col == Beetle.TipoColision.Colision)
            {
                if (Beetle.Sliding())
                {
                    // Cambiar sonido por obstaculo destruido
                    Reproductor.ObstaculoDestruido();

                    PistaNivel.Obstaculos.Remove(objetoColisionado);
                    finDeNivel = Pantalla.Acierto();
                }
                else if (Beetle.Inmunidad())
                {
                    // Nada, simplemente no moris
                }
                else
                {
                    CurvaError();
                }

            }

        }

        private void UpdateTemporizadores()
        {
            if (Temporizadores.inmunidadError.update(GameModel.ElapsedTime))
                Beetle.inmunidadTemp = false;

            if (Temporizadores.finDeNivel.update(GameModel.ElapsedTime))
            {
                soloPista = false;
            }

            curvaActiva = !Temporizadores.curvaOk.update(GameModel.ElapsedTime);
            recolectableActivo = !Temporizadores.recolectableOk.update(GameModel.ElapsedTime);

            if (soloPista)
            {
                if (ValidationUtils.validateFloatRange(Temporizadores.finDeNivel.Current.ToString(), 5f, 6f))
                {
                    Temporizadores.textScoreTotal.reset();
                }

                if (ValidationUtils.validateFloatRange(Temporizadores.finDeNivel.Current.ToString(), 8f, 9f))
                {
                    Temporizadores.textRank.reset();
                }

                if (ValidationUtils.validateFloatRange(Temporizadores.finDeNivel.Current.ToString(), 11f, 12f))
                {
                    Temporizadores.textNextLvl.reset();
                    Pantalla.restartLevel();
                    if (Pantalla.level > 5)
                    {
                        finDeJuego = true;
                        isWin = true;
                    }
                }

            }

        }

        private void CurvaOk()
        {
            Reproductor.CurvaTomada();
            finDeNivel = Pantalla.Acierto();
            Temporizadores.curvaOk.reset();
        }

        private void CurvaError()
        {
            if (!Beetle.escudo)
            {
                // Perdiste!
                //Aca hay que ver como mantener la inmunidad hasta que se termine la curva!
                Reproductor.Perder();
                Beetle.speed = 0f;
                finDeJuego = true;
            }
            else
            {
                Beetle.PerderEscudo();
                Reproductor.CurvaFallida();
                Temporizadores.inmunidadError.reset();
                Beetle.inmunidadTemp = true;
                Temporizadores.curvaOk.reset();
                finDeNivel = Pantalla.Error();
                Pantalla.Danio = true;
            }

        }

        private void AccionesEnCurva(float rotacion)
        {
            if (Beetle.Inmunidad())
            {
                CurvaOk();
            }
            else
            {
                //DERECHA
                if (rotacion > 0f)
                {
                    if (Beetle.derecha)
                        CurvaOk();
                    else
                        CurvaError();
                }
                // IZQUIERDA
                else if (rotacion < 0f)
                {
                    if (Beetle.izquierda)
                        CurvaOk();
                    else
                        CurvaError();
                }
            }


        }
    }
}