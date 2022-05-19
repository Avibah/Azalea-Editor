using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Azalea_Editor.GUI
{
    class CheckBox : Gui
    {
        public string text;
        public int textsize;
        public int settingindex;
        public RectangleF originrect;
        public bool Visible = true;
        public bool keybinds = false;
        public int keybindindex;
        public string keybindtype;

        public CheckBox(float posx, float posy, float sizex, float sizey, string Text, int TextSize, string Setting, bool Keybinds = false, string KeybindName = "", string KeybindType = "") : base(posx, posy, sizex, sizey)
        {
            text = Text;
            textsize = TextSize;
            if (!Keybinds)
                settingindex = Settings.FindToggle(Setting);
            originrect = new RectangleF(posx, posy, sizex, sizey);
            keybinds = Keybinds;
            if (Keybinds)
                keybindindex = Settings.FindKeybind(KeybindName);
            keybindtype = KeybindType;
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            if (Visible)
            {
                GL.Color3(0.1f, 0.1f, 0.1f);
                GLSpecial.Rect(rect);
                GL.LineWidth(2f);
                GL.Color3(1f, 1f, 1f);
                GLSpecial.Outline(rect);

                var toggle = keybinds ? ((Settings.keybinds[keybindindex].CTRL && keybindtype == "CTRL") || (Settings.keybinds[keybindindex].ALT && keybindtype == "ALT") || (Settings.keybinds[keybindindex].SHIFT && keybindtype == "SHIFT")) : Settings.toggles[settingindex].Toggle;
                if (toggle)
                {
                    GL.Color3(0.5f, 0.5f, 0.5f);
                    GLSpecial.Rect(rect.X + 3f, rect.Y + 3f, rect.Width - 6f, rect.Height - 6f);
                }

                var height = TextHeight(textsize);
                GL.LineWidth(1f);
                GL.Color3(1f, 1f, 1f);
                RenderText(text, rect.Right + 3f, rect.Y + rect.Height / 2f - height / 2f, textsize);
            }
        }

        public override void OnMouseClick(Point pos, bool right = false)
        {
            var index = Settings.FindString("ClickSound");

            MainWindow.inst.SoundPlayer.Play(Settings.strings[index].Value);

            if (!keybinds)
                Settings.toggles[settingindex].Toggle = !Settings.toggles[settingindex].Toggle;
            else
            {
                switch (keybindtype)
                {
                    case "CTRL":
                        Settings.keybinds[keybindindex].CTRL = !Settings.keybinds[keybindindex].CTRL;
                        break;
                    case "ALT":
                        Settings.keybinds[keybindindex].ALT = !Settings.keybinds[keybindindex].ALT;
                        break;
                    case "SHIFT":
                        Settings.keybinds[keybindindex].SHIFT = !Settings.keybinds[keybindindex].SHIFT;
                        break;
                }
            }
        }
    }
}
