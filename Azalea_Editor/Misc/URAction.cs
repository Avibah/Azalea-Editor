using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azalea_Editor.Misc
{
    [Serializable]
    class URAction
    {
        public string label;
        public Action undo;
        public Action redo;

        public URAction(string Label, Action Undo, Action Redo)
        {
            label = Label;
            undo = Undo;
            redo = Redo;
        }
    }
}
