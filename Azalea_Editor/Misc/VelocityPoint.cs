using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azalea_Editor.Misc
{
    [Serializable]
    class VelocityPoint
    {
        public float Velocity;
        public long Ms;

        public VelocityPoint(float velocity, long ms)
        {
            Velocity = velocity;
            Ms = ms;
        }
    }
}
