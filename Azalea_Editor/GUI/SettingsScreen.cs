using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Azalea_Editor.GUI
{
    class SettingsScreen : Gui
    {
        public readonly Button Return = new Button(1280f / 2f - 192f, 720f / 2f, 384f, 64f, 0, "Return to Menu", 32);
        public readonly Slider keybindIndex = new Slider(75f, 50f, 25f, 610f, "KeybindIndex");
        public readonly Slider colorIndex = new Slider(640f + 192f, 50f, 25f, 610f, "ColorIndex");

        public readonly TextBox fontName = new TextBox(1280f / 2f - 155f, 90f, 200f, 48f, false, "", 30);
        public readonly Button fontConfirm = new Button(1280f / 2f + 45f + 10f, 90f, 100f, 48f, 1, "Confirm", 24);

        public readonly TextBox clickSound = new TextBox(1280f / 2f - 155f, 90f + 48f + 50f, 200f, 48f, false, "", 30);
        public readonly Button clickConfirm = new Button(1280f / 2f + 45f + 10f, 90f + 48f + 50f, 100f, 48f, 2, "Confirm", 24);
        public readonly TextBox noteSound = new TextBox(1280f / 2f - 155f, 90f + (48f + 50f) * 2f, 200f, 48f, false, "", 30);
        public readonly Button noteConfirm = new Button(1280f / 2f + 45f + 10f, 90f + (48f + 50f) * 2f, 100f, 48f, 3, "Confirm", 24);

        public SettingsScreen() : base(0, 0, MainWindow.inst.ClientSize.Width, MainWindow.inst.ClientSize.Height)
        {
            foreach (var keybind in Settings.keybinds)
            {
                var textbox = new TextBox(100, 0, 100, 85, false, keybind.Key.ToString(), 20, keybind.Name);
                var ctrlbox = new CheckBox(210, 0, 25, 25, "CTRL", 20, "", true, keybind.Name, "CTRL");
                var altbox = new CheckBox(210, 0, 25, 25, "ALT", 20, "", true, keybind.Name, "ALT");
                var shiftbox = new CheckBox(210, 0, 25, 25, "SHIFT", 20, "", true, keybind.Name, "SHIFT");

                boxes.Add(textbox);
                checkboxes.Add(ctrlbox);
                checkboxes.Add(altbox);
                checkboxes.Add(shiftbox);
            }

            foreach (var color in Settings.colors)
            {
                var button = new Button(640 + 192 + 25, 0, 100, 85, 100 + Settings.FindColor(color.Name), " Pick\nColor", 30, color.Name);

                buttons.Add(button);
            }

            buttons.Add(Return);

            sliders.Add(keybindIndex);
            sliders.Add(colorIndex);

            boxes.Add(fontName);
            buttons.Add(fontConfirm);

            boxes.Add(clickSound);
            buttons.Add(clickConfirm);
            boxes.Add(noteSound);
            buttons.Add(noteConfirm);

            fontName.text = Settings.strings[Settings.FindString("FontName")].Value;
            clickSound.text = Settings.strings[Settings.FindString("ClickSound")].Value;
            noteSound.text = Settings.strings[Settings.FindString("NoteSound")].Value;

            AlignIndexes();
            OnResize(MainWindow.inst.ClientSize);
        }

        private void AlignIndexes()
        {
            Settings.sliders[keybindIndex.settingindex].Value = MathHelper.Clamp(Settings.sliders[keybindIndex.settingindex].Value, 0, Settings.keybinds.Count - 1);
            Settings.sliders[keybindIndex.settingindex].Max = Settings.keybinds.Count - 1;

            Settings.sliders[colorIndex.settingindex].Value = MathHelper.Clamp(Settings.sliders[colorIndex.settingindex].Value, 0, Settings.colors.Count - 1);
            Settings.sliders[colorIndex.settingindex].Max = Settings.colors.Count - 1;
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            var checkboxwidth = TextWidth("SHIFT", 20);
            var textheight = TextHeight(30);
            var heightdiff = rect.Height / 720f;

            foreach (var checkbox in checkboxes)
                checkbox.Visible = false;

            foreach (var box in boxes)
            {
                box.Visible = box.settingindex < 0;

                if (box.settingindex >= 0)
                {
                    box.Visible = box.settingindex >= Settings.sliders[keybindIndex.settingindex].Value && box.settingindex < Settings.sliders[keybindIndex.settingindex].Value + 6;

                    if (box.Visible)
                    {
                        box.rect.Y = (50f + (85f + 20f) * (box.settingindex - Settings.sliders[keybindIndex.settingindex].Value)) * heightdiff;

                        foreach (var checkbox in checkboxes)
                        {
                            if (checkbox.keybindindex == box.settingindex)
                            {
                                var offset = 0f;

                                switch (checkbox.keybindtype)
                                {
                                    case "ALT":
                                        offset = 5f + 25f;
                                        break;
                                    case "SHIFT":
                                        offset = 10f + 50f;
                                        break;
                                }

                                checkbox.rect.Y = box.rect.Y + offset * heightdiff;
                                checkbox.Visible = true;
                            }
                        }

                        RenderText(Settings.keybinds[box.settingindex].Name, box.rect.Right + 75f + checkboxwidth, box.rect.Y + box.rect.Width / 2f - textheight / 2f, 30);
                    }
                }
            }

            foreach (var button in buttons)
            {
                button.Visible = button.settingindex < 0;

                if (button.settingindex >= 0)
                {
                    button.Visible = button.settingindex >= Settings.sliders[colorIndex.settingindex].Value && button.settingindex < Settings.sliders[colorIndex.settingindex].Value + 6;

                    if (button.Visible)
                    {
                        button.rect.Y = (50f + (85f + 20f) * (button.settingindex - Settings.sliders[colorIndex.settingindex].Value)) * heightdiff;

                        var colorrect = new RectangleF(button.rect.Right + 10f, button.rect.Y, button.rect.Height, button.rect.Height);

                        GL.Color3(Settings.colors[button.settingindex].Color);
                        GLSpecial.Rect(colorrect);
                        GL.Color3(0.8f, 0.8f, 0.8f);
                        GLSpecial.Outline(colorrect);

                        RenderText(Settings.colors[button.settingindex].Name, colorrect.Right + 10f, colorrect.Y + colorrect.Height / 2f - textheight / 2f, 30);
                    }
                }
            }

            RenderText("Font Name", fontName.rect.X + 5f, fontName.rect.Y - 5f - textheight, 30);
            RenderText("Click Sound Name", clickSound.rect.X + 5f, clickSound.rect.Y - 5f - textheight, 30);
            RenderText("Note Sound Name", noteSound.rect.X + 5f, noteSound.rect.Y - 5f - textheight, 30);

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
                    MainWindow.inst.SwitchScreen(new MenuScreen());
                    break;
                case 1:
                    if (MainWindow.inst.FileExists("misc/fonts", fontName.text))
                    {
                        var indexf = Settings.FindString("FontName");
                        Settings.strings[indexf].Value = fontName.text;
                        MainWindow.inst.font = new FreeTypeFont($"misc/fonts/{fontName.text}.ttf", 32);
                    }
                    break;
                case 2:
                    if (MainWindow.inst.FileExists("misc/sounds", clickSound.text))
                    {
                        var indexc = Settings.FindString("ClickSound");
                        Settings.strings[indexc].Value = clickSound.text;
                    }
                    break;
                case 3:
                    if (MainWindow.inst.FileExists("misc/sounds", noteSound.text))
                    {
                        var indexn = Settings.FindString("NoteSound");
                        Settings.strings[indexn].Value = noteSound.text;
                    }
                    break;
            }

            if (id >= 100)
            {
                var index = id - 100;

                var colordialog = new ColorDialog();
                colordialog.Color = Settings.colors[index].Color;

                if (colordialog.ShowDialog() == DialogResult.OK)
                    Settings.colors[index].Color = colordialog.Color;
            }

            base.OnButtonClicked(id);
        }
    }
}
