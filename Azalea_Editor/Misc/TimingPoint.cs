using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azalea_Editor.Misc
{
    [Serializable]
    class TimingPoint
    {
        public float Bpm;
        public long Ms;

        public TimingPoint(float bpm, long ms)
        {
            Bpm = bpm;
            Ms = ms;
        }
    }
}
