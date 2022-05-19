using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Azalea_Editor.GUI
{
    class Slider : Gui
    {
        public bool dragging;
        public int settingindex;
        public bool timeline;
        public RectangleF originrect;
        public bool Visible = true;
        public bool reverse = false;

        public Slider(float posx, float posy, float sizex, float sizey, string Setting, bool Reverse = false,  bool Timeline = false) : base(posx, posy, sizex, sizey)
        {
            settingindex = Settings.FindSlider(Setting);
            timeline = Timeline;
            originrect = new RectangleF(posx, posy, sizex, sizey);
            reverse = Reverse;
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            if (Visible)
            {
                var horizontal = rect.Width > rect.Height;

                if (dragging)
                {
                    float step = Settings.sliders[settingindex].Step / Settings.sliders[settingindex].Max;
                    var pos = horizontal ? rect.X : rect.Y;
                    var mouse = horizontal ? mousex : mousey;
                    var width = horizontal ? rect.Width : rect.Height;
                    var prog = (float)Math.Round((horizontal ? mouse - pos : reverse ? (width - (mouse - pos)) : (mouse - pos)) / width / step) * step;

                    Settings.sliders[settingindex].Value = MathHelper.Clamp(Settings.sliders[settingindex].Max * prog, 0, Settings.sliders[settingindex].Max);

                    switch (Settings.sliders[settingindex].Name)
                    {
                        case "Timeline":
                            MainWindow.inst.MusicPlayer.Pause();
                            MainWindow.inst.currentTime = TimeSpan.FromMilliseconds(Settings.sliders[settingindex].Value);
                            break;
                        case "Tempo":
                            MainWindow.inst.tempo = Settings.sliders[settingindex].Value + 0.1f;
                            MainWindow.inst.MusicPlayer.Tempo = MainWindow.inst.tempo;
                            break;
                        case "BeatDivisor":
                            MainWindow.inst.BeatDivisor = (int)(Settings.sliders[settingindex].Value + 0.5f) + 1;
                            break;
                        case "SFXVolume":
                            MainWindow.inst.SoundPlayer.Volume = Settings.sliders[settingindex].Value;
                            break;
                        case "MusicVolume":
                            MainWindow.inst.MusicPlayer.Volume = Settings.sliders[settingindex].Value;
                            break;
                    }
                }

                if (Settings.sliders[settingindex].Name == "Timeline")
                    Settings.sliders[settingindex].Value = (float)MainWindow.inst.currentTime.TotalMilliseconds;

                var progress = Settings.sliders[settingindex].Value / Settings.sliders[settingindex].Max;
                var pos1 = new Vector2(horizontal ? rect.X + rect.Width * progress : rect.X + rect.Width / 2f, horizontal ? rect.Y + rect.Height / 2f : rect.Y + rect.Height * (reverse ? (1f - progress) : progress));
                var pos2 = new Vector2(mousex, mousey);
                var hovering = dragging || (pos1 - pos2).Length <= 12f;

                GL.LineWidth(2f);
                GL.Color3(1f, 1f, 1f);

                if (horizontal)
                    GLSpecial.Line(rect.X, rect.Y + rect.Height / 2f, rect.Right, rect.Y + rect.Height / 2f);
                else
                    GLSpecial.Line(rect.X + rect.Width / 2f, rect.Y, rect.X + rect.Width / 2f, rect.Bottom);

                GL.Color3(0.8f, 0.8f, 0.8f);
                GLSpecial.Circle(pos1.X, pos1.Y, 6f, 16);

                if (hovering)
                    GLSpecial.Circle(pos1.X, pos1.Y, 12f, 6, true);
            }
        }
    }
}
