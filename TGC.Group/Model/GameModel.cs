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

namespace TGC.Group.Model
{
         
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm 
    ///     <see cref="Form.GameForm.InitGraphics()" /> line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        ///<param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }
               
        //Atributos Globales
        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }
        private TgcThirdPersonCamera camaraInterna;
        private Beetle Beetle;
        private Pista PistaNivel;
        private TgcMp3Player mp3PlayerMusica;
        private TgcStaticSound sound;
        private Pantalla Pantalla;

        private bool applyMovement;
        public float posX = 0, posY = 0;
        public TGCVector3 posicionFinal = new TGCVector3(0, 0, 0);
        float beetleDang = FastMath.PI_HALF;

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        public override void Init()
        {
            
            //Instancio el reproductores sonido
            mp3PlayerMusica = new TgcMp3Player();
            mp3PlayerMusica.FileName = MediaDir + "Thumper/Mp3/Thumper OST - Spiral.mp3";
            mp3PlayerMusica.play(true);

            sound = new TgcStaticSound();
            
            Pantalla = new Pantalla(MediaDir);
            Beetle = new Beetle(MediaDir);
            PistaNivel = new Pista(MediaDir); 
           
            camaraInterna = new TgcThirdPersonCamera(Beetle.position,20f,-100f);
            Camara = camaraInterna;
           
        }


        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        /// 

        public override void Update()
        {
            PreUpdate();

            // Cuando llego al final de la pista, se actualiza, el rango de vision es alrededor de 2500 Z
            if (PistaNivel.posUltimaPieza.Z - Beetle.position.Z <= 2500)
            {
                PistaNivel.UpdateTunel();
                PistaNivel.UpdateCurvaSuave();
                PistaNivel.UpdatePista();
            }

            //Capturar Input teclado
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }
            if (Input.keyPressed(Key.Space))
            {
                Recolectable objetoColisionado = new Recolectable(MediaDir, TGCVector3.One);
                if (Beetle.ColisionandoConRecolectable(PistaNivel.Recolectables, ref objetoColisionado))
                {
                    System.Console.WriteLine("Recolecte un objeto!!!");

                    sound.dispose();
                    sound.loadSound(MediaDir + "Thumper/Mp3/laserBeat.wav", DirectSound.DsDevice);
                    sound.play(false);

                    PistaNivel.Recolectables.Remove(objetoColisionado);

                    Pantalla.AumentoPuntuacion(Beetle.poderActivado);
                    Pantalla.multiplicador += 1;
                }
                else Pantalla.PierdoCombo();
            }

            // Cambie esto para desacelerar y acelerar, para pruebas
            if (Input.keyPressed(Key.A))
            {
                Beetle.ActivarPoder();
            }
            if (Input.keyPressed(Key.S))
            {
                Beetle.DesvanecerVelocidad(ElapsedTime);
            }
            
            // Capturador de Giro
            if (Input.keyDown(Key.Q))
            {
                if (beetleDang > Geometry.DegreeToRadian(45)) //0f)
                    beetleDang -= Beetle.VELOCIDAD_ANGULAR * ElapsedTime;

                //Ver como activar arrastrado
            }
            else
            {
                if (beetleDang < FastMath.PI_HALF)
                    beetleDang += Beetle.VELOCIDAD_ANGULAR * ElapsedTime;
            }
            
            if (Input.keyDown(Key.W))
            {
                if (beetleDang < Geometry.DegreeToRadian(120)) // FastMath.PI)
                    beetleDang += Beetle.VELOCIDAD_ANGULAR * ElapsedTime;

                //Ver como activar arrastrado
            }
            else
            {
                if (beetleDang > FastMath.PI_HALF)
                    beetleDang -= Beetle.VELOCIDAD_ANGULAR * ElapsedTime;
            }
            

            // Para tests
            /*
            if (Input.keyPressed(Key.W))
            {
                applyMovement = true;
                posicionFinal = new TGCVector3(40f, 10, 0);

            }

            if (Input.keyPressed(Key.R))
            {
                applyMovement = true;
                posicionFinal = new TGCVector3(-40f, 10, 0);

            }

            if (Input.keyPressed(Key.E))
            {
                applyMovement = true;
                posicionFinal = new TGCVector3(0f, 10, 0);

            }
            */

            foreach (TgcMesh box2 in PistaNivel.SegmentosPista)
            {
                //Reviso si el beetle colisiona con algun elemento de la pista  
                if (TgcCollisionUtils.testObbAABB(Beetle.collider, box2.BoundingBox))
                {   
                    // Si no hay movimiento en X activo, capturo la posicion final a la que debe ir el beetle
                    if (!applyMovement)
                    {
                        // Color para detectar la colision, testing
                        box2.setColor(Color.Red);
                        posX = box2.Position.X - Beetle.position.X;
                        posY = box2.Position.Y - Beetle.position.Y + 8;
                        if (posX != 0 || posY != 0)
                        {
                            applyMovement = true;
                            posicionFinal = box2.Position;
                            System.Console.WriteLine("Beetle position " + TGCVector3.PrintVector3(Beetle.position));
                            System.Console.WriteLine("Posicion objetivo " + TGCVector3.PrintVector3(posicionFinal));
                        }
                        
                    }

                }
                else
                {
                    // Color para detectar la colision, testing
                    box2.setColor(Color.Blue);
                }
            }
            
            // Si todavia no alcanzó la pos final de movimiento
            if (applyMovement)
            {
                //Ver si queda algo de distancia para mover
                var posDiff = new TGCVector3(posicionFinal.X, 0f,0f) - new TGCVector3(Beetle.position.X, 0f, 0f) ;

                var posDiffLength = posDiff.LengthSq();
                /*
                System.Console.WriteLine("long diferencia "+ posDiffLength);
                System.Console.WriteLine("epsilon " + float.Epsilon);
                */
                System.Console.WriteLine("Beetle position " + TGCVector3.PrintVector3(Beetle.position));
                System.Console.WriteLine("Posicion objetivo " + TGCVector3.PrintVector3(posicionFinal));
                System.Console.WriteLine("Posicion diff " + TGCVector3.PrintVector3(posDiff));

                //Si esta a menos de 1 asumo que esta en la misma posicion
                if (posDiffLength > 1)
                {
                    //Intento mover el beetle interpolando por la velocidad
                    //var currentVelocity = Beetle.VELOCIDADX * ElapsedTime;
                    var currentVelocity = 100f * ElapsedTime;
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
            PistaNivel.posActual = Beetle.Avanza(ElapsedTime, posX, posY, beetleDang);            

            camaraInterna.Target = Beetle.position;
            
            Pantalla.Update(camaraInterna.Position);      
            
            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {   
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();
            
            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Posicion actual del jugador: " + TGCVector3.PrintVector3(Beetle.position), 0, 30, Color.OrangeRed);
            DrawText.drawText("Posicion actual ultima pieza: " + TGCVector3.PrintVector3(PistaNivel.posUltimaPieza), 0, 40, Color.OrangeRed);
            DrawText.drawText("Giro de bloque: " + (PistaNivel.rotCurvaActual.Y), 0, 50, Color.OrangeRed);
            DrawText.drawText("Cantidad de bloques a girar: " + (PistaNivel.cantCurvaSuaveActual), 0, 60, Color.OrangeRed);
            DrawText.drawText("X diff: " + posX, 0, 70, Color.OrangeRed);
            DrawText.drawText("Y Diff: " + posY, 0, 80, Color.OrangeRed);
            DrawText.drawText("Beetle distAng: " + this.beetleDang.ToString(), 0, 90, Color.OrangeRed);
            DrawText.drawText("RAD 45: " + (Geometry.DegreeToRadian(45)), 0, 100, Color.OrangeRed);
            

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                PistaNivel.BoundingBoxRender();
                foreach(var mesh in Beetle.beetle.Meshes)
                {
                    mesh.BoundingBox.Render();
                }
            }
                        
            Beetle.Render();
            PistaNivel.Render();
            Pantalla.Render();

            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            PistaNivel.Dispose();
            Beetle.Dispose();
            Pantalla.Dispose();
            //Cierra el reproductor
            mp3PlayerMusica.closeFile();
            sound.dispose();

        }
    }
}