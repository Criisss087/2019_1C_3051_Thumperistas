using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public class Reproductor
    {
        private const string levelPrincipal = "Thumper/Mp3/Thumper OST - Spiral.mp3";
        private const string recolectar = "Thumper/Mp3/Deploy.wav";
        private const string obstaculoDestruido = "Thumper/Mp3/beat.wav";
        private const string curvaTomada = "Thumper/Mp3/Curva.wav";
        private const string curvaFallida = "Thumper/Mp3/Fail.wav";
        private const string poderActivado = "Thumper/Mp3/Poder.wav";

        private String mediaDir;
        private TgcMp3Player mp3PlayerMusica;
        private TgcStaticSound sound;
        private TgcDirectSound device;

        public Reproductor(String _mediaDir, TgcDirectSound _device)
        {
            mediaDir = _mediaDir;
            mp3PlayerMusica = new TgcMp3Player();
            sound = new TgcStaticSound();
            device = _device;
        }

        public void ReproducirLevelPrincipal()
        {
            mp3PlayerMusica.FileName = mediaDir + levelPrincipal;
            mp3PlayerMusica.play(true);
        }

        public void Recolectar()
        {
            sound.dispose();
            sound.loadSound(mediaDir + recolectar, device.DsDevice);
            sound.play(false);
        }

        public void ObstaculoDestruido()
        {
            sound.dispose();
            sound.loadSound(mediaDir + obstaculoDestruido, device.DsDevice);
            sound.play(false);
        }

        public void CurvaTomada()
        {
            sound.dispose();
            sound.loadSound(mediaDir + curvaTomada, device.DsDevice);
            sound.play(false);
        }

        public void CurvaFallida()
        {
            sound.dispose();
            sound.loadSound(mediaDir + curvaFallida, device.DsDevice);
            sound.play(false);
        }

        public void PoderActivado()
        {
            sound.dispose();
            sound.loadSound(mediaDir + poderActivado, device.DsDevice);
            sound.play(false);
        }

        public void Dispose()
        {
            mp3PlayerMusica.closeFile();
            sound.dispose();
        }



    }
}
