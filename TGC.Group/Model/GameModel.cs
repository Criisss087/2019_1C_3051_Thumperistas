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
		private bool help { get; set; }
        private TgcThirdPersonCamera camaraInterna;
        private Beetle Beetle;
        private Pista PistaNivel;
        private Pantalla Pantalla;
        private Reproductor Reproductor;
		private Disparo Disparo;
		public Temporizadores Temporizadores;
		private Pantalla.FaseTexto FaseTexto { get; set; } = Pantalla.FaseTexto.Nada;
        
        private bool applyMovement { get; set; }
		private bool disparoActivo { get; set; }
        private bool curvaActiva { get; set; }
		private bool finDeNivel { get; set; }
        private bool finDeJuego { get; set; }
        private bool soloPista { get; set; }
        public float posX = 0, posY = 0;
        public TGCVector3 posicionFinal = new TGCVector3(0, 0, 0);
        private Random rnd = new Random();

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        public override void Init()
        {

            //Instancio el reproductores sonido
            Reproductor = new Reproductor(MediaDir, DirectSound);
            Pantalla = new Pantalla(MediaDir);
            Beetle = new Beetle(MediaDir);
            PistaNivel = new Pista(MediaDir);
			Temporizadores = new Temporizadores();
			
            Reproductor.ReproducirLevelPrincipal();
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
			
			UpdateTemporizadores();

            if (!finDeJuego)
            {
                // Cuando llego al final de la pista, se actualiza, el rango de vision es alrededor de 2500 Z
                if (PistaNivel.posUltimaPieza.Z - Beetle.position.Z <= 2500)
                {
                    if (finDeNivel)
                    {
                        Temporizadores.finDeNivel.reset();
                        soloPista = true;
                    }
                    //Console.WriteLine(Temporizadores.finDeNivel.Current + "asa" + finDeNivel.ToString());

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

                if (Input.keyPressed(Key.D) && Pantalla.AcumuladorPoder > 5)
                {
                    Disparo = new Disparo(MediaDir, Beetle.position);
                    Reproductor.Disparar();
                    Pantalla.AcumuladorPoder = 0;
                    disparoActivo = true;
                }

                if (Input.keyPressed(Key.H))
                    help = !help;

                Beetle.Update(Input, ElapsedTime);
                Colisiones();

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
                        var currentVelocity = Beetle.VELOCIDADX * ElapsedTime;
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
                PistaNivel.posActual = Beetle.Avanza(ElapsedTime, posX, posY);

                if (disparoActivo)
                {
                    var dist = Disparo.Avanza(ElapsedTime, posX, posY);
                    if (dist.Z - Beetle.position.Z > 2000)
                    {
                        Reproductor.Explosion();
                        disparoActivo = false;
                        Pantalla.scoreTemporal += 1000;
                    }
                }

                camaraInterna.Target = Beetle.position;
                Pantalla.Update(camaraInterna.Position);
            }
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
			
			if(help)
			{
				//Dibuja un texto por pantalla
				DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
				DrawText.drawText("Posicion actual del jugador: " + TGCVector3.PrintVector3(Beetle.position), 0, 30, Color.OrangeRed);
				DrawText.drawText("Posicion actual ultima pieza: " + TGCVector3.PrintVector3(PistaNivel.posUltimaPieza), 0, 40, Color.OrangeRed);
				DrawText.drawText("Cant Aciertos: " + (Pantalla.AcumuladorAciertos), 0, 50, Color.OrangeRed);
				DrawText.drawText("Cant Eventos: " + (Pantalla.AcumuladorEventos), 0, 60, Color.OrangeRed);
				DrawText.drawText("Cant para poder: " + Pantalla.AcumuladorPoder, 0, 70, Color.OrangeRed);
				DrawText.drawText("Nivel: " + Pantalla.level, 0, 80, Color.OrangeRed);
				DrawText.drawText("Inmunidad temporal: " + Beetle.inmunidadTemp.ToString(), 0, 90, Color.OrangeRed);
				DrawText.drawText("Escudo: " + (Beetle.escudo.ToString()), 0, 100, Color.OrangeRed);
				DrawText.drawText("GodMode: " + (Beetle.godMode.ToString()), 0, 110, Color.OrangeRed);
                DrawText.drawText("Fin de Nivel: " + finDeNivel, 0, 120, Color.OrangeRed);
                DrawText.drawText("Score total: " + Pantalla.Score, 0, 130, Color.OrangeRed);
                DrawText.drawText("Multiplcador: " + Pantalla.Multiplicador, 0, 140, Color.OrangeRed);
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

            if(!finDeJuego)
                Beetle.Render();

            PistaNivel.Render();
            Pantalla.Render(FaseTexto);
			
			if(disparoActivo)
				Disparo.Render();

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
            Reproductor.Dispose();
        }

        private void Colisiones()
        {
            Beetle.TipoColision col; 

            // Colision con recolectable 

            Recolectable recolectableColisionado = new Recolectable(MediaDir, TGCVector3.One);

            col = Beetle.ColisionandoConRecolectable(PistaNivel.Recolectables, ref recolectableColisionado);
            
            if (col == Beetle.TipoColision.Colision) 
            {
                if(Beetle.godMode || (Input.keyPressed(Key.Space)))
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
            else if(col == Beetle.TipoColision.Error)
            {
                Reproductor.NoRecolectar();
                PistaNivel.Recolectables.Remove(recolectableColisionado);
                finDeNivel = Pantalla.Error();
            }
            

            // Colision con obstaculo            
            Obstaculo objetoColisionado = new Obstaculo(TGCVector3.One);
            col = Beetle.ColisionandoConObstaculo(PistaNivel.Obstaculos, ref objetoColisionado);

            if (col == Beetle.TipoColision.Colision) 
            {   
                if(Beetle.Sliding()) 
                {
                    // Cambiar sonido por obstaculo destruido
                    Reproductor.ObstaculoDestruido();

                    // Emitir particulas?
                    PistaNivel.Obstaculos.Remove(objetoColisionado);
                    finDeNivel = Pantalla.Acierto();
                }
                else if(Beetle.Inmunidad())
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
			if(Temporizadores.inmunidadError.update(ElapsedTime))
				Beetle.inmunidadTemp = false;

            if (Temporizadores.finDeNivel.update(ElapsedTime))
				soloPista = false;

            curvaActiva = !Temporizadores.curvaOk.update(ElapsedTime);

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
            }
            
        }
		
		private void AccionesEnCurva(float rotacion)
		{
			if(Beetle.Inmunidad())
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