using TGC.Core.Camara;
using TGC.Core.Input;
using TGC.Core.Mathematica;

namespace TGC.Group.Model
{
    internal class TgcRotationalCamera : TgcCamera
    {
        private TGCVector3 tGCVector3;
        private float v;
        private TgcD3dInput input;

        public TgcRotationalCamera(TGCVector3 tGCVector3, float v, TgcD3dInput input)
        {
            this.tGCVector3 = tGCVector3;
            this.v = v;
            this.input = input;
        }
    }
}