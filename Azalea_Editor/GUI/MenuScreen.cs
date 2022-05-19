using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.IO;
using System.Windows.Forms;
using Azalea_Editor.Properties;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;

namespace Azalea_Editor.GUI
{
    class MenuScreen : Gui
    {
        public readonly Button importAudio = new Button(1280f / 2f - 128f, 720f / 2f, 256f, 32f, 0, "Import Audio", 24);
        public readonly Button createChart = new Button(1280f / 2f - 128f, 720f / 2f + 32f + 10f, 256f, 32f, 1, "Create Chart", 24);
        public readonly Button loadChart = new Button(1280f / 2f - 128f, 720f / 2f + (32f + 10f) * 2f, 256f, 32f, 2, "Load Chart", 24);
        public readonly Button importChart = new Button(1280f / 2f - 128f, 720f / 2f + (32f + 10f) * 3f, 256f, 32f, 3, "Import From Data", 24);
        public readonly Button settingsMenu = new Button(1280f / 2f - 128f, 720f / 2f + (32f + 10f) * 4f, 256f, 32f, 4, "Settings", 24);

        private readonly int txID;

        public MenuScreen() : base(0, 0, MainWindow.inst.ClientSize.Width, MainWindow.inst.ClientSize.Height)
        {
            txID = GLSpecial.LoadTexture("misc/images/azalealogo.png");

            buttons.Add(importAudio);
            buttons.Add(createChart);
            buttons.Add(loadChart);
            buttons.Add(importChart);
            buttons.Add(settingsMenu);

            OnResize(MainWindow.inst.ClientSize);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            

            GLSpecial.Image(rect.Width / 4f, rect.Height / 16f, rect.Width / 2f, rect.Height / 2f, txID);

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
                            string filename = Path.GetFileNameWithoutExtension(dialogimport.SafeFileName);
                            File.Copy(dialogimport.FileName, $"cache/{filename}.azl", true);
                            MainWindow.inst.LoadChart(false, filename);
                        }
                    }
                    break;
                case 1:
                    MainWindow.inst.SwitchScreen(new CreateScreen());
                    break;
                case 2:
                    using (var dialogload = new OpenFileDialog
                    {
                        Title = "Select Chart File",
                        Filter = "Text Documents (*.txt)|*.txt"
                    })
                    {
                        if (dialogload.ShowDialog() == DialogResult.OK)
                            MainWindow.inst.LoadChart(true, dialogload.FileName);
                    }
                    break;
                case 3:
                    if (Clipboard.ContainsText())
                        MainWindow.inst.LoadChart(false, Clipboard.GetText());
                    break;
                case 4:
                    MainWindow.inst.SwitchScreen(new SettingsScreen());
                    break;
            }

            base.OnButtonClicked(id);
        }
    }
}
