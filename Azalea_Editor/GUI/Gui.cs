using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Input;

namespace Azalea_Editor.GUI
{
    class Gui
    {
        public RectangleF rect;

        public List<Button> buttons = new List<Button>();
        public List<CheckBox> checkboxes = new List<CheckBox>();
        public List<Slider> sliders = new List<Slider>();
        public List<TextBox> boxes = new List<TextBox>();

        public Track track;
        public Grid grid;

        protected Gui(float posx, float posy, float sizex, float sizey)
        {
            rect = new RectangleF(posx, posy, sizex, sizey);
        }

        public virtual void Render(float mousex, float mousey, float frametime)
        {
            foreach (var button in buttons)
                button.Render(mousex, mousey, frametime);
            foreach (var checkbox in checkboxes)
                checkbox.Render(mousex, mousey, frametime);
            foreach (var slider in sliders)
                slider.Render(mousex, mousey, frametime);
            foreach (var box in boxes)
                box.Render(mousex, mousey, frametime);

            track?.Render(mousex, mousey, frametime);
            grid?.Render(mousex, mousey, frametime);
        }

        public virtual void OnResize(Size size)
        {
            var widthdiff = size.Width / 1280f;
            var heightdiff = size.Height / 720f;

            foreach (var button in buttons)
                button.rect = ResizeRect(button.originrect, widthdiff, heightdiff);
            foreach (var checkbox in checkboxes)
                checkbox.rect = ResizeRect(checkbox.originrect, widthdiff, heightdiff);
            foreach (var slider in sliders)
                slider.rect = ResizeRect(slider.originrect, widthdiff, heightdiff);
            foreach (var box in boxes)
                box.rect = ResizeRect(box.originrect, widthdiff, heightdiff);

            if (track != null)
                track.rect = ResizeRect(track.originrect, widthdiff, heightdiff);
            if (grid != null)
                grid.rect = ResizeRect(grid.originrect, widthdiff, heightdiff);
        }

        public virtual void OnMouseMove(Point pos)
        {

        }

        public virtual void OnMouseClick(Point pos, bool right = false)
        {
            foreach (var button in buttons)
            {
                if (button.rect.Contains(pos) && button.Visible)
                {
                    button.OnMouseClick(pos);
                    OnButtonClicked(button.ID);
                }
            }

            foreach (var checkbox in checkboxes)
            {
                if (checkbox.rect.Contains(pos) && checkbox.Visible)
                    checkbox.OnMouseClick(pos);
            }

            foreach (var slider in sliders)
            {
                var horizontal = slider.rect.Width > slider.rect.Height;
                var hitbox = new RectangleF(slider.rect.X - (horizontal ? 12f : 0f), slider.rect.Y - (horizontal ? 0f : 12f), slider.rect.Width + (horizontal ? 24f : 0f), slider.rect.Height + (horizontal ? 0f : 24f));
                
                if (hitbox.Contains(pos) && slider.Visible)
                {
                    var index = Settings.FindString("ClickSound");

                    MainWindow.inst.SoundPlayer.Play(Settings.strings[index].Value);
                    slider.dragging = true;
                }
            }

            foreach (var box in boxes)
            {
                if (box.rect.Contains(pos) && box.Visible)
                    box.OnMouseClick(pos);
                else
                    box.focused = false;
            }

            if (track != null)
                track.wasplaying = false;
            if (track != null && track.rect.Contains(pos))
                track.OnMouseClick(pos);
            if (grid != null && grid.rect.Contains(pos))
                grid.OnMouseClick(pos);
            else
                MainWindow.inst.SelectedNotes.Clear();
        }

        public virtual void OnRightClick(Point pos)
        {
            if (track != null && track.rect.Contains(pos))
                track.OnMouseClick(pos, true);
            if (grid != null && grid.rect.Contains(pos))
                grid.DeleteNote(false);
        }

        public virtual void OnButtonClicked(int id)
        {

        }

        public virtual void OnMouseLeave()
        {
            foreach (var slider in sliders)
                slider.dragging = false;

            if (track != null)
            {
                track.dragging = false;
                track.rightdrag = false;
                if (track.wasplaying)
                    MainWindow.inst.MusicPlayer.Play();
                track.wasplaying = false;
            }
            grid?.OnMouseLeave();
        }

        public virtual void OnMouseUp()
        {
            foreach (var slider in sliders)
                slider.dragging = false;

            if (track != null)
            {
                track.dragging = false;
                track.rightdrag = false;
                if (track.wasplaying)
                    MainWindow.inst.MusicPlayer.Play();
                track.wasplaying = false;
            }
            grid?.OnMouseUp();
        }

        public virtual void OnKeyPress(char key)
        {
            foreach (var box in boxes)
            {
                if (box.focused && box.Visible)
                    box.OnKeyPress(key);
            }
        }

        public virtual void OnKeyDown(Key key, bool control)
        {
            foreach (var box in boxes)
            {
                if (box.focused && box.Visible)
                    box.OnKeyDown(key, control);
            }

            if (key == Key.Delete && grid != null)
                grid.DeleteNote(true);
        }

        public void RenderText(string text, float posx, float posy, int size)
        {
            MainWindow.inst.font.Print(text, posx, posy - (text.Split('\n').Count() - 1) * 5f, size / 32f);
        }

        public int TextWidth(string text, int size)
        {
            string[] lines = text.Split('\n');
            int max = 0;

            foreach (var line in lines)
                max = Math.Max(max, (int)(MainWindow.inst.font.Extent(line) * size / 32f));

            return max;
        }

        public int TextHeight(int size)
        {
            return (int)(MainWindow.inst.font.BaseLine * size / 32f);
        }

        public RectangleF ResizeRect(RectangleF originrect, float width, float height)
        {
            return new RectangleF(originrect.X * width, originrect.Y * height, originrect.Width * width, originrect.Height * height);
        }

        public virtual void OnClosing()
        {

        }
    }
}
