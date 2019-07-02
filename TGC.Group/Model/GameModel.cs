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
using System.Windows.Forms;
using TGC.Group.Form;
using TGC.Core;

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

        public GameState GameState { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        public override void Init()
        {
            GameState = new StartMenu(this);
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
            GameState.Update();
            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
         
            ClearTextures();
            /*
            D3DDevice.Instance.Device.BeginScene();
            RenderFPS();
            RenderAxis();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
            */
            GameState.Render();


        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            GameState.Dispose();
        }

        /// <summary>
        ///     Se llama desde algún estado de juego
        ///     Hace que se cierre el form
        /// </summary>
        public void Exit()
        {
            var formEnumerator = Application.OpenForms.GetEnumerator();
            formEnumerator.MoveNext();
            var gameForm = (GameForm)formEnumerator.Current;
            gameForm.ShutDown();
            gameForm.Close();
        }
    }
}