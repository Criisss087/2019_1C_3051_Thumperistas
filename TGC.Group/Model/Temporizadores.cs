using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Interpolation;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public static class Temporizadores
    {
        public static Temporizador inmunidadError = new Temporizador();
        public static Temporizador curvaOk = new Temporizador();
        static public Temporizador finDeNivel = new Temporizador();

        public static Temporizador textFinNivel = new Temporizador();
        public static Temporizador textScoreTotal = new Temporizador();
        public static Temporizador textRank = new Temporizador();
        public static Temporizador textNextLvl = new Temporizador();

        public static Temporizador textCambioMult = new Temporizador();
        
        public static void Init()
        { 
			
			inmunidadError.StopSegs = 2.5f;
            inmunidadError.Current = 2.5f;
            //curvaOk = new Temporizador();
            curvaOk.StopSegs = 1f;
            curvaOk.Current = 1f;

            //finDeNivel = new Temporizador();
            finDeNivel.StopSegs = 10f;
            finDeNivel.Current = 10f;

            //textFinNivel = new Temporizador();
            textFinNivel.StopSegs = 3f;			
            textFinNivel.Current = 3f; 
            //textScoreTotal = new Temporizador();
            textScoreTotal.StopSegs = 2f;
            textScoreTotal.Current = 2f;
            //textRank = new Temporizador();
            textRank.StopSegs = 2f;
            textRank.Current = 2f;
            //textNextLvl = new Temporizador();
            textNextLvl.StopSegs = 3f;
			textNextLvl.Current = 3f;

            //textCambioMult = new Temporizador();
            textCambioMult.StopSegs = 3f;
            textCambioMult.Current = 3f;
        }

    }
}
