using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using OpenTK;

namespace Azalea_Editor.GUI
{
    class TextBox : Gui
    {
        public bool numeric;
        public bool focused;
        public string text;
        public int textsize;
        public float time;
        public int cursorpos;
        public RectangleF originrect;
        public bool Visible = true;
        public int settingindex = -1;

        public TextBox(float posx, float posy, float sizex, float sizey, bool Numeric, string Text, int TextSize, string keybindname = "") : base(posx, posy, sizex, sizey)
        {
            numeric = Numeric;
            text = Text;
            textsize = TextSize;
            originrect = new RectangleF(posx, posy, sizex, sizey);
            if (keybindname != "")
                settingindex = Settings.FindKeybind(keybindname);
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

                var width = TextWidth(text, textsize);
                var height = TextHeight(textsize);
                var x = rect.X + rect.Width / 2f - width / 2f;
                var y = rect.Y + rect.Height / 2f - height / 2f;
                GL.Color3(1f, 1f, 1f);
                RenderText(text, x, y, textsize);

                if (focused && time < 0.5f)
                {
                    var widthatcursor = TextWidth(text.Substring(0, cursorpos), textsize);
                    GL.LineWidth(2f);
                    GLSpecial.Line(x + widthatcursor, y - 2f, x + widthatcursor, y + height + 4f);
                }

                time += frametime;
                time %= 1f;
            }
        }

        public override void OnMouseClick(Point pos, bool right = false)
        {
            if (text.Length > 0)
            {
                var textwidth = TextWidth(text, textsize);
                var x = pos.X - rect.X - rect.Width / 2f + textwidth / 2f;
                var width = textwidth / text.Length;

                x = MathHelper.Clamp(x, 0, textwidth);
                x = (float)Math.Floor(x / width + 0.3f);

                cursorpos = (int)x;
            }
            
            focused = true;
            time = 0f;
        }

        public override void OnKeyPress(char key)
        {
            var str = key.ToString();

            if (numeric)
            {
                var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                if (int.TryParse(str, out _) || (str == separator && !text.Contains(str)) || (str == "-" && !text.Contains("-") && cursorpos == 0))
                {
                    text = text.Insert(cursorpos, str);
                    cursorpos++;
                }
            }
            else if (settingindex < 0)
            {
                text = text.Insert(cursorpos, str);
                cursorpos++;
            }

            time = 0f;
        }

        public override void OnKeyDown(Key key, bool control)
        {
            if (settingindex < 0)
            {
                switch (key)
                {
                    case Key.BackSpace:
                        if (!control)
                            text = cursorpos > 0 ? text.Remove(cursorpos - 1, 1) : text;
                        else
                            text = text.Substring(cursorpos, text.Length - cursorpos);
                        cursorpos = Math.Max(0, cursorpos - (control ? cursorpos : 1));
                        break;
                    case Key.Delete:
                        if (!control)
                            text = cursorpos < text.Length ? text.Remove(cursorpos, 1) : text;
                        else
                            text = text.Substring(0, cursorpos);
                        break;
                    case Key.Left:
                        cursorpos = Math.Max(0, cursorpos - (control ? cursorpos : 1));
                        break;
                    case Key.Right:
                        cursorpos = Math.Min(text.Length, control ? text.Length : cursorpos + 1);
                        break;
                    case Key.V when control:
                        try
                        {
                            text = text.Insert(cursorpos, Clipboard.GetText());
                            cursorpos += Clipboard.GetText().Length;
                        }
                        catch { }
                        break;
                    case Key.Enter:
                        text = text.Insert(cursorpos, "\n");
                        cursorpos++;
                        break;
                }
            }
            else
            {
                text = key.ToString();
                Settings.keybinds[settingindex].Key = key;
                cursorpos = text.Length;
            }

            time = 0f;
        }
    }
}
