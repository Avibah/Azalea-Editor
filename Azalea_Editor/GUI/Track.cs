using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Azalea_Editor.Misc;

namespace Azalea_Editor.GUI
{
    class Track : Gui
    {
        public bool dragging;
        public bool rightdrag;
        public bool wasplaying;
        public float dragstartpx;
        public long dragstartms;
        public RectangleF originrect;

        private float noteheight = 48f;

        public Track(float posx, float posy, float sizex, float sizey) : base(posx, posy, sizex, sizey)
        {
            originrect = new RectangleF(posx, posy, sizex, sizey);
        }

        private long MsToPixels(long ms, bool offset = true)
        {
            return (long)(rect.Height - noteheight - MainWindow.inst.MsToPixels(ms, offset));
        }

        private float PixelsToMs(float pixels)
        {
            return MainWindow.inst.PixelsToMs(rect.Height - noteheight - pixels);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            GL.Color3(0.2f, 0.2f, 0.2f);
            GLSpecial.Rect(rect);

            //waveform start
            GL.Color3(1f, 0f, 0f);
            GL.PushMatrix();
            GL.BindVertexArray(MainWindow.inst.MusicPlayer.Waveform.VaoID);
            GL.EnableVertexAttribArray(0);

            var endY = -MainWindow.inst.totalTime.TotalMilliseconds / 1000f * MainWindow.inst.NoteStep;
            var waveY = MainWindow.inst.currentTime.TotalMilliseconds / 1000f * MainWindow.inst.NoteStep;
            var scale = endY / 100000.0;

            GL.Rotate(90, 0f, 0f, 1f);
            GL.Translate(waveY, -rect.X + rect.Width / 2f, 0);
            GL.Scale(scale, rect.Width, 1);
            GL.Translate(-0.5 + (rect.Height - noteheight) / scale, -1.5, 0);
            GL.LineWidth(2);
            MainWindow.inst.MusicPlayer.Waveform.Render(PrimitiveType.LineStrip);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);
            GL.PopMatrix();
            //waveform end

            GL.LineWidth(1f);
            GL.Color3(1f, 1f, 1f);
            GLSpecial.Outline(rect);
            GLSpecial.Line(rect.X, rect.Bottom - noteheight, rect.Right, rect.Bottom - noteheight);
            
            if (dragging)
            {
                var msdiff = PixelsToMs(dragstartpx) - PixelsToMs(mousey);
                var ms = MathHelper.Clamp(dragstartms + msdiff, 0, MainWindow.inst.totalTime.TotalMilliseconds);
                var closest = MainWindow.inst.ClosestBeat((long)ms, false, false);

                if (closest >= 0 && Math.Abs(MsToPixels(closest) - MsToPixels((long)ms)) < 12f)
                    ms = closest;

                MainWindow.inst.currentTime = TimeSpan.FromMilliseconds(ms);
            }
            else if (rightdrag)
            {
                var dragmsdiff = PixelsToMs(dragstartpx) - MainWindow.inst.currentTime.TotalMilliseconds;
                var dragms = dragstartms + dragmsdiff;
                var dragrect = new RectangleF(rect.X, mousey, rect.Width, MsToPixels((long)dragms) - mousey);

                GL.Color4(0f, 1f, 0f, 0.3f);
                GLSpecial.Rect(dragrect);

                MainWindow.inst.SelectedNotes = NotesInRect((long)dragms, (long)PixelsToMs(mousey));
                MainWindow.inst.SelectedEnds.Clear();
            }
        }

        public List<Note> NotesInRect(long startms, long endms)
        {
            var notes = new List<Note>();

            foreach (var note in MainWindow.inst.Notes)
            {
                if (note.StartMs <= Math.Max(endms, startms) && note.StartMs >= Math.Min(endms, startms))
                    notes.Add(note);
            }

            return notes;
        }

        public override void OnMouseClick(Point pos, bool right = false)
        {
            wasplaying = MainWindow.inst.MusicPlayer.IsPlaying;
            MainWindow.inst.MusicPlayer.Pause();
            rightdrag = right;
            dragging = !right;
            dragstartpx = pos.Y;
            dragstartms = (long)MainWindow.inst.currentTime.TotalMilliseconds;
        }
    }
}
