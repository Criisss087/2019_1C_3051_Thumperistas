using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public class Temporizadores
    {
        public Temporizador inmunidadError;
		public Temporizador curvaOk;
		public Temporizador finDeNivel;

        public Temporizadores()
        {            		
			inmunidadError = new Temporizador();
			inmunidadError.StopSegs = 2f;
			
			curvaOk = new Temporizador();
			curvaOk.StopSegs = 0.5f;
			
			finDeNivel = new Temporizador();
			finDeNivel.StopSegs = 10f;
        }

    }
}
