using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azalea_Editor.Misc;

namespace Azalea_Editor
{
    class UndoRedoManager
    {
        private List<URAction> actions = new List<URAction>();
        private int index = -1;

        public void AddFunction(string label, Action undo, Action redo, bool runRedo = true)
        {
            while (index + 1 < actions.Count)
                actions.RemoveAt(index + 1);

            actions.Add(new URAction(label, undo, redo));
            index += 1;

            if (runRedo)
                actions[index].redo?.Invoke();
        }

        public void Undo()
        {
            if (index >= 0)
            {
                actions[index].undo?.Invoke();
                index -= 1;
            }
        }

        public void Redo()
        {
            if (index + 1 < actions.Count)
            {
                actions[index + 1].redo?.Invoke();
                index += 1;
            }
        }

        public void Clear()
        {
            actions.Clear();
        }
    }
}
