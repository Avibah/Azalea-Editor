using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Azalea_Editor.GUI
{
    class Button : Gui
    {
        public int ID;
        public bool hovering;
        public string text;
        public int textsize;
        public RectangleF originrect;
        public bool Visible = true;
        public int settingindex = -1;

        public Button(float posx, float posy, float sizex, float sizey, int id, string Text, int TextSize, string colorname = "") : base(posx, posy, sizex, sizey)
        {
            ID = id;
            text = Text;
            textsize = TextSize;
            originrect = new RectangleF(posx, posy, sizex, sizey);
            if (colorname != "")
                settingindex = Settings.FindColor(colorname);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            if (Visible)
            {
                hovering = rect.Contains(mousex, mousey);

                var alpha = hovering ? 0.6f : 1f;
                GL.Color4(0.3f, 0.3f, 0.3f, alpha);
                GLSpecial.Rect(rect);
                GL.LineWidth(2f);
                GL.Color3(1f, 1f, 1f);
                GLSpecial.Outline(rect);

                var width = TextWidth(text, textsize);
                var height = TextHeight(textsize) * text.Split('\n').Count();
                GL.LineWidth(1f);
                GL.Color3(1f, 1f, 1f);
                RenderText(text, rect.X + rect.Width / 2f - width / 2f, rect.Y + rect.Height / 2f - height / 2f, textsize);
            }
        }

        public override void OnMouseClick(Point pos, bool right = false)
        {
            var index = Settings.FindString("ClickSound");

            MainWindow.inst.SoundPlayer.Play(Settings.strings[index].Value);
        }
    }
}
