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
            
            

            //Device de DirectX para crear primitivas. No se usa?
            //var d3dDevice = D3DDevice.Instance.Device;

            //System.Console.WriteLine(MediaDir);
            Pantalla = new Pantalla(MediaDir);
            Beetle = new Beetle(MediaDir);
            PistaNivel = new Pista(MediaDir); 
            camaraInterna = new TgcThirdPersonCamera(Beetle.Position(),20f,-100f);
                        
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
                        
            //Capturar Input teclado
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }
            if (Input.keyPressed(Key.Z) || Input.keyPressed(Key.X))
            {
                Recolectable objetoColisionado =new Recolectable(MediaDir, TGCVector3.One);
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
            if (Input.keyPressed(Key.Space))
            {
                Beetle.ActivarPoder();
            }
            if (Beetle.poderActivado) { Beetle.DesvanecerVelocidad(ElapsedTime); }
            //muevo beetle para adelante
            Beetle.Avanza(ElapsedTime);

            

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
            
                                         
            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                PistaNivel.BoundingBoxRender();
                Beetle.Mesh.BoundingBox.Render();//Muestro mi BoundingBox
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