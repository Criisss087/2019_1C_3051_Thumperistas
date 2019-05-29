using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Interpolation;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public class Temporizadores
    {
        public Temporizador inmunidadError;
		public Temporizador curvaOk;
		public Temporizador finDeNivel;
		
		public Temporizador textFinNivel;
		public Temporizador textScoreTotal;
		public Temporizador textRank;
		public Temporizador textNextLvl;
		

        public Temporizadores()
        {            		
			inmunidadError = new Temporizador();
			inmunidadError.StopSegs = 2.5f;
			
			curvaOk = new Temporizador();
			curvaOk.StopSegs = 1f;
			
			finDeNivel = new Temporizador();
			finDeNivel.StopSegs = 10f;
			
			textFinNivel = new Temporizador();
			textFinNivel.StopSegs = 3f;			
			textScoreTotal = new Temporizador();
			textScoreTotal.StopSegs = 2f;
			textRank = new Temporizador();
			textRank.StopSegs = 2f;
			textNextLvl = new Temporizador();
			textNextLvl.StopSegs = 3f;
        }

    }
}
