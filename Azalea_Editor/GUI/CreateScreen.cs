using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Azalea_Editor.GUI
{
    class CreateScreen : Gui
    {
        public readonly TextBox audioID = new TextBox(1280f / 2f - 192f, 720f / 2f, 384f, 64f, true, "", 32);
        public readonly Button importAudio = new Button(1280f / 2f - 192f, 720f / 2f + 64f + 10f, 384f, 64f, 0, "Import Audio", 32);
        public readonly Button createChart = new Button(1280f / 2f - 192f, 720f / 2f + (64f + 10f) * 2f, 384f, 64f, 1, "Create Chart", 32);
        public readonly Button Return = new Button(1280f / 2f - 192f, 720f / 2f + (64f + 10f) * 3f, 384f, 64f, 2, "Return to Menu", 32);

        public CreateScreen() : base(0, 0, MainWindow.inst.ClientSize.Width, MainWindow.inst.ClientSize.Height)
        {
            boxes.Add(audioID);
            buttons.Add(importAudio);
            buttons.Add(createChart);
            buttons.Add(Return);

            OnResize(MainWindow.inst.ClientSize);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            base.Render(mousex, mousey, frametime);
        }

        public override void OnResize(Size size)
        {
            rect = new RectangleF(0, 0, size.Width, size.Height);

            base.OnResize(size);
        }

        public override void OnButtonClicked(int id)
        {
            switch (id)
            {
                case 0:
                    using (var dialogimport = new OpenFileDialog
                    {
                        Title = "Select Audio File",
                        Filter = "Audio Files (*.mp3;*.ogg;*.wav;*.azl)|*.mp3;*.ogg;*.wav;*.azl"
                    })
                    {
                        if (dialogimport.ShowDialog() == DialogResult.OK)
                        {
                            File.Copy(dialogimport.FileName, $"cache/{audioID.text}.azl");
                            MainWindow.inst.LoadChart(false, audioID.text);
                        }
                    }
                    break;
                case 1:
                    MainWindow.inst.LoadChart(false, audioID.text);
                    break;
                case 2:
                    MainWindow.inst.SwitchScreen(new MenuScreen());
                    break;
            }

            base.OnButtonClicked(id);
        }
    }
}
