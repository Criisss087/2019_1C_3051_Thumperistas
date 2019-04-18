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


namespace TGC.Group.Model
{


   
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        /// 

        //Aca la empiezo a cagar yo

        TgcThirdPersonCamera camaraInterna;
        //private TgcMesh TunnelMesh { get; set; }
        //private TgcMesh TriangularMesh { get; set; }

        private TgcMesh BeetleMesh { get; set; }
        float zSpeed { get; set; }
        TGCVector3 forward = new TGCVector3(0f, 0f, 1f);
        Pista SegmentosPista;
        public override void Init()
        {
            zSpeed = 0;
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            
            //Defino una escala en el modelo logico del mesh que es muy grande.

            var loader = new TgcSceneLoader();

            //Loadeo todas mis meshes
            //TunnelMesh = loader.loadSceneFromFile(MediaDir + "/Thumper/circular_tunnel-TgcScene.xml").Meshes[0];
            //TriangularMesh = loader.loadSceneFromFile(MediaDir + "/Thumper/triangular_tunnel-TgcScene.xml").Meshes[0];    
            System.Console.WriteLine(MediaDir);

            BeetleMesh = loader.loadSceneFromFile(MediaDir+ "/Thumper/beetle-TgcScene.xml").Meshes[7];


            //Modifico como quiero que empiece el mesh
            BeetleMesh.Position = TGCVector3.Empty;

            BeetleMesh.Scale = TGCVector3.One * .5f; //Escalo a la mitad del beetle
            
            BeetleMesh.RotateY(FastMath.PI_HALF); //Lo roto
            
            BeetleMesh.BoundingBox.transform(TGCMatrix.RotationY(FastMath.PI_HALF));
            //tambien tengo que rota el boundingbox porque eso no se actuliza

            BeetleMesh.Position += new TGCVector3(2f, 8f, 0f);



            SegmentosPista = new Pista(MediaDir); 


            camaraInterna = new TgcThirdPersonCamera(BeetleMesh.Position,30f,-100f);
            
            Camara = camaraInterna;
           // cameraOffset = new TGCVector3(0f, 10f, -200f);
           // Camara.SetCamera(BeetleMesh.BoundingBox.calculateBoxCenter() + cameraOffset,  BeetleMesh.Position);
            

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
            if (Input.keyDown(Key.UpArrow))
            {
                zSpeed += 5f;
            }
            if (Input.keyDown(Key.DownArrow))
            {
                zSpeed -= 5f;
            }
            camaraInterna.Target = BeetleMesh.Position;
            BeetleMesh.Position += forward * ElapsedTime *zSpeed;
            BeetleMesh.BoundingBox.transform(BeetleMesh.Transform);



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
            DrawText.drawText("Posicion actual del jugador: " + TGCVector3.PrintVector3(BeetleMesh.Position), 0, 30, Color.OrangeRed);
            
            //Siempre antes de renderizar el modelo necesitamos actualizar la matriz de transformacion.
            //Debemos recordar el orden en cual debemos multiplicar las matrices, en caso de tener modelos jerárquicos, tenemos control total.
            // Box.Transform = TGCMatrix.Scaling(Box.Scale) * TGCMatrix.RotationYawPitchRoll(Box.Rotation.Y, Box.Rotation.X, Box.Rotation.Z) * TGCMatrix.Translation(Box.Position);
            //A modo ejemplo realizamos toda las multiplicaciones, pero aquí solo nos hacia falta la traslación.
            //Finalmente invocamos al render de la caja
            // Box.Render();

            //Cuando tenemos modelos mesh podemos utilizar un método que hace la matriz de transformación estándar.
            //Es útil cuando tenemos transformaciones simples, pero OJO cuando tenemos transformaciones jerárquicas o complicadas.
            //  Mesh.UpdateMeshTransform();
            //Render del mesh
            //  Mesh.Render();





            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                foreach (TgcMesh AuxMesh in SegmentosPista.GetSegmentosPista())
                {
                    AuxMesh.BoundingBox.Render();
                }
                BeetleMesh.BoundingBox.Render();//Muestro mi BoundingBox
            }
            BeetleMesh.UpdateMeshTransform();


            
            BeetleMesh.Render();

            foreach(TgcMesh AuxMesh in SegmentosPista.GetSegmentosPista())
            {
                AuxMesh.Render();
            }

            //Aca termine de cagarla porque lo de abajo es importante.
            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            //TODO sacar pista
            //TriangularMesh.Dispose();
            //TunnelMesh.Dispose();
            foreach (TgcMesh AuxMesh in SegmentosPista.GetSegmentosPista())
            {
                AuxMesh.Dispose();
            }
            BeetleMesh.Dispose(); //dispongo de mis mesh 
        }
    }
}