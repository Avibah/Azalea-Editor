using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azalea_Editor.Misc
{
    [Serializable]
    class Note
    {
        public long StartMs;
        public long EndMs;
        public long Lane;
        public long DragStartMsDifference;
        public long DragEndMsDifference;
        public long DragLaneDifference;

        public Note(long startms, long endms, long lane)
        {
            StartMs = startms;
            EndMs = endms;
            Lane = lane;
        }
    }
}
