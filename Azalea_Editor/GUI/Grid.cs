using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Azalea_Editor.Misc;
using OpenTK;

namespace Azalea_Editor.GUI
{
    class Grid : Gui
    {
        public bool dragging;
        public long dragstart;
        public RectangleF originrect;

        public Note hoverstart;
        public Note hoverend;

        public Note draggedstart;
        public Note draggedend;
        public Note draggedstartorigin;
        public Note draggedendorigin;

        public Note selectedstart;
        public Note selectedend;

        private float noteheight = 48f;
        private float notewidth;

        public Grid(float posx, float posy, float sizex, float sizey) : base(posx, posy, sizex, sizey)
        {
            originrect = new RectangleF(posx, posy, sizex, sizey);
        }

        private long MsToPixels(long ms)
        {
            return (long)(rect.Height - noteheight - MainWindow.inst.MsToPixels(ms));
        }

        private float PixelsToMs(float pixels)
        {
            return MainWindow.inst.PixelsToMs(rect.Height - noteheight - pixels);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            //base rendering
            notewidth = rect.Width / 4f;
            var widthdiff = rect.Width / originrect.Width;

            GL.LineWidth(1f);
            GL.Color3(1f, 1f, 1f);
            GLSpecial.Outline(rect);
            GLSpecial.Line(rect.X, rect.Bottom - noteheight, rect.Right, rect.Bottom - noteheight);
            GL.Color3(0.2f, 0.2f, 0.2f);
            GLSpecial.Rect(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height - noteheight));

            GL.Color3(0.5f, 0.5f, 0.5f);
            for (int i = 1; i < 4; i++)
                GLSpecial.Line(rect.X + notewidth * i, rect.Bottom, rect.X + notewidth * i, rect.Y);

            GL.Color3(1f, 1f, 1f);
            for (int i = 0; i < 4; i++)
            {
                var x = rect.X + notewidth / 2f + notewidth * i;
                var y = rect.Bottom - noteheight / 2f;
                var keybind = Settings.FindKeybind($"Lane{i}");
                var keyname = Settings.keybinds[keybind].Key.ToString().Replace("Number", "").Replace("Keypad", "");
                var textwidth = TextWidth(keyname, 32);
                var textheight = TextHeight(32);

                RenderText(keyname, x - textwidth / 2f, y - textheight / 2f, 32);
            }

            //drag start
            if (draggedstart != null)
            {
                var originms = draggedstart.StartMs;
                var enddiff = draggedstart.EndMs - draggedstart.StartMs;
                var ms = MainWindow.inst.ClosestBeat((long)PixelsToMs(mousey + noteheight / 2f), false, false);
                ms = ms >= 0 ? ms : (long)PixelsToMs(mousey + noteheight / 2f);
                ms = (long)MathHelper.Clamp(ms, 0, MainWindow.inst.totalTime.TotalMilliseconds);
                var lane = (long)((mousex - rect.X) / notewidth);

                var maxlane = 0L;
                var minlane = 3L;
                foreach (var note in MainWindow.inst.SelectedNotes)
                {
                    note.DragStartMsDifference = note.StartMs - draggedstart.StartMs;
                    note.DragEndMsDifference = note.EndMs - note.StartMs;
                    note.DragLaneDifference = note.Lane - draggedstart.Lane;

                    maxlane = Math.Max(maxlane, note.Lane);
                    minlane = Math.Min(minlane, note.Lane);
                }

                lane = (long)MathHelper.Clamp(lane, Math.Max(draggedstart.Lane - minlane, 0), Math.Min(3 - (maxlane - draggedstart.Lane), 3));

                draggedstart.StartMs = -1;
                draggedstart.EndMs = -1;
                if (SelectedInBoundaries(lane, ms, enddiff))
                {
                    draggedstart.StartMs = ms;
                    draggedstart.EndMs = ms + enddiff;
                    draggedstart.Lane = lane;
                }
                else
                {
                    draggedstart.StartMs = originms;
                    draggedstart.EndMs = originms + enddiff;
                }

                foreach (var note in MainWindow.inst.SelectedNotes)
                {
                    note.StartMs = draggedstart.StartMs + note.DragStartMsDifference;
                    note.EndMs = note.StartMs + note.DragEndMsDifference;
                    note.Lane = draggedstart.Lane + note.DragLaneDifference;
                }
            }

            //drag end
            if (draggedend != null)
            {
                var originms = draggedend.StartMs;
                var enddiff = draggedend.EndMs - draggedend.StartMs;
                var ms = MainWindow.inst.ClosestBeat((long)PixelsToMs(mousey + noteheight / 2f), false, false);
                ms = ms >= 0 ? ms : (long)PixelsToMs(mousey + noteheight / 2f);
                ms = (long)MathHelper.Clamp(ms, draggedend.StartMs, MainWindow.inst.totalTime.TotalMilliseconds);

                draggedend.StartMs = -1;
                draggedend.EndMs = -1;
                if (!MainWindow.inst.NoteExists(draggedend.Lane, ms) && !MainWindow.inst.EndOverlap(draggedend.Lane, originms, ms))
                    draggedend.EndMs = ms;
                else
                    draggedend.EndMs = originms + enddiff;
                draggedend.StartMs = originms;
            }

            var lastms = PixelsToMs(0);

            //timing points
            GL.LineWidth(1f);
            for (int i = 0; i < MainWindow.inst.Timings.Count; i++)
            {
                var point = MainWindow.inst.Timings[i];
                var nextms = MainWindow.inst.totalTime.TotalMilliseconds;

                if (i + 1 < MainWindow.inst.Timings.Count)
                    nextms = MainWindow.inst.Timings[i + 1].Ms;

                if (point.Ms <= lastms && nextms >= MainWindow.inst.currentTime.TotalMilliseconds)
                {
                    var linems = (float)point.Ms;
                    var linediff = 60000 / point.Bpm;
                    var smalldiff = linediff / MainWindow.inst.BeatDivisor;
                    var y = MsToPixels((long)linems);

                    GL.Color3(1f, 1f, 1f);
                    RenderText($"{point.Bpm} BPM", rect.X - 100f * widthdiff + 5f, y + 5f, 20);

                    while (linems < nextms)
                    {
                        y = MsToPixels((long)linems);

                        if (y <= rect.Bottom - noteheight)
                        {
                            GL.Color3(1f, 1f, 1f);
                            GLSpecial.Line(rect.X - 100f * widthdiff, y, rect.Right, y);
                        }
                        
                        for (int v = 1; v < MainWindow.inst.BeatDivisor; v++)
                        {
                            if (linems + smalldiff * v >= MainWindow.inst.currentTime.TotalMilliseconds && linems + smalldiff * v < nextms)
                            {
                                y = MsToPixels((long)(linems + smalldiff * v));
                                
                                GL.Color3((v == MainWindow.inst.BeatDivisor / 2 && MainWindow.inst.BeatDivisor % 2 == 0) ? MainWindow.inst.colors["Divisor1"] : MainWindow.inst.colors["Divisor2"]);
                                GLSpecial.Line(rect.X, y, rect.Right, y);
                            }
                        }

                        linems += linediff;
                    }
                }
            }

            //velocities
            foreach (var velocity in MainWindow.inst.Velocities)
            {
                if (velocity.Ms > MainWindow.inst.currentTime.TotalMilliseconds && velocity.Ms < lastms)
                {
                    var y = MsToPixels(velocity.Ms);

                    GL.Color3(0f, 1f, 1f);
                    GL.LineWidth(2f);
                    GLSpecial.Line(rect.X - 100f * widthdiff, y, rect.Right, y);
                    RenderText($"{velocity.Velocity}x", rect.X - 100f * widthdiff + 5f, y + 2f + TextHeight(20), 20);
                }
            }

            Note hoveringstart = null;
            Note hoveringend = null;

            //notes
            foreach (var note in MainWindow.inst.Notes)
            {
                var color1 = MainWindow.inst.colors[$"Lane{note.Lane}"];
                var color2 = MainWindow.inst.colors[$"Lane{note.Lane}End"];

                //line
                if (note.StartMs != note.EndMs && note.StartMs <= lastms && note.EndMs >= MainWindow.inst.currentTime.TotalMilliseconds)
                {
                    var x = rect.X + notewidth * note.Lane + notewidth / 2f;
                    var y1 = Math.Min(MsToPixels(note.StartMs), rect.Bottom - noteheight);
                    var y2 = Math.Max(MsToPixels(note.EndMs) - noteheight + noteheight / 2f, 0);

                    GL.Color3(color1);
                    GL.LineWidth(12f);
                    GLSpecial.Line(x, y1, x, y2);
                }

                //start
                if (note.StartMs >= MainWindow.inst.currentTime.TotalMilliseconds && note.StartMs <= lastms)
                {
                    var y = MsToPixels(note.StartMs) - noteheight;
                    var startrect = new RectangleF(rect.X + notewidth * note.Lane, y, notewidth, noteheight);

                    GL.Color3(color1);
                    GLSpecial.Rect(startrect);

                    var outlinecolor = note == selectedstart ? Color.FromArgb(255, 0, 100, 255) : Color.FromArgb(255, 0, 255, 100);

                    if (startrect.Contains(mousex, mousey) || MainWindow.inst.SelectedNotes.Contains(note))
                    {
                        if (startrect.Contains(mousex, mousey))
                            hoveringstart = note;
                        GL.Color3(outlinecolor);
                        GL.LineWidth(2f);
                        GLSpecial.Outline(startrect);
                    }
                }

                //end
                if (note.EndMs >= MainWindow.inst.currentTime.TotalMilliseconds && note.EndMs <= lastms)
                {
                    var x = rect.X + notewidth * note.Lane + notewidth / 2f;
                    var y = MsToPixels(note.EndMs) - noteheight / 2f;
                    var r = noteheight * 5f / 12f;
                    var endrect = new RectangleF(x - r, y - r, 2 * r, 2 * r);

                    GL.Color3(color2);
                    GLSpecial.Circle(x, y, r, 4);
                    GL.Color3(0.5f, 0.5f, 0.5f);
                    GL.LineWidth(2f);
                    GLSpecial.Circle(x, y, r + 2f, 4, true);

                    var outlinecolor = note == selectedend ? Color.FromArgb(255, 0, 100, 255) : Color.FromArgb(255, 0, 255, 100);

                    if (MainWindow.inst.SelectedEnds.Contains(note) || endrect.Contains(mousex, mousey))
                    {
                        if (endrect.Contains(mousex, mousey))
                            hoveringend = note;
                        GL.Color3(outlinecolor);
                        GL.LineWidth(2f);
                        GLSpecial.Circle(x, y, r + 3f, 8, true);
                    }
                }
            }

            hoverstart = hoveringstart;
            hoverend = hoveringend;

            //preview
            if (hoverstart == null && hoverend == null && rect.Contains(mousex, mousey) && draggedstart == null && draggedend == null)
            {
                long ms = MainWindow.inst.ClosestBeat((long)PixelsToMs(mousey + noteheight / 2f), false, false);
                ms = ms >= 0 ? ms : (long)PixelsToMs(mousey + noteheight / 2f);
                ms = (long)MathHelper.Clamp(ms, 0, MainWindow.inst.totalTime.TotalMilliseconds);

                long startms = 0;
                if (dragging)
                {
                    startms = MainWindow.inst.ClosestBeat(dragstart, false, false);
                    startms = startms >= 0 ? startms : dragstart;
                    startms = (long)MathHelper.Clamp(startms, 0, MainWindow.inst.totalTime.TotalMilliseconds);
                }

                var lane = (long)((mousex - rect.X) / notewidth);
                var x = lane * notewidth + rect.X;
                var y = MsToPixels(dragging ? startms : ms) - noteheight;
                var previewrect = new RectangleF(x + notewidth / 12f, y + noteheight / 12f, notewidth * 5f / 6f, noteheight * 5f / 6f);

                if (!MainWindow.inst.NoteExists(lane, dragging ? dragstart : ms) && (!dragging || !MainWindow.inst.NoteExists(lane, ms)) && !MainWindow.inst.EndOverlap(lane, dragging ? dragstart : ms, ms))
                {
                    if (dragging)
                    {
                        var y2 = Math.Min(MsToPixels(ms) - noteheight / 2f, y + noteheight / 2f);
                        var r = previewrect.Height / 2f - 3f;

                        GL.Color3(0.5f, 0.5f, 0.5f);
                        GL.LineWidth(12f);
                        GLSpecial.Line(x + notewidth / 2f, Math.Min(rect.Bottom - noteheight, previewrect.Bottom), x + notewidth / 2f, y2);
                        GLSpecial.Circle(x + notewidth / 2f, y2, r, 4);
                        GL.Color3(0.7f, 0.7f, 0.7f);
                        GL.LineWidth(2f);
                        GLSpecial.Circle(x + notewidth / 2f, y2, r + 3f, 4, true);
                    }

                    if (previewrect.Bottom <= rect.Bottom - noteheight)
                    {
                        GL.Color3(0.5f, 0.5f, 0.5f);
                        GLSpecial.Rect(previewrect);
                        GL.Color3(0.7f, 0.7f, 0.7f);
                        GL.LineWidth(2f);
                        GLSpecial.Outline(previewrect);
                    }
                }
            }
        }

        public bool SelectedInBoundaries(long lane, long ms, long enddiff)
        {
            foreach (var note in MainWindow.inst.SelectedNotes)
            {
                note.StartMs = -1;
                note.EndMs = -1;
            }

            if (MainWindow.inst.NoteExists(lane, ms) || MainWindow.inst.NoteExists(lane, ms + enddiff) || MainWindow.inst.EndOverlap(lane, ms, ms + enddiff))
                return false;

            foreach (var note in MainWindow.inst.SelectedNotes)
            {
                //ms check
                if (ms + note.DragStartMsDifference < 0 || ms + note.DragStartMsDifference + note.DragEndMsDifference > MainWindow.inst.totalTime.TotalMilliseconds)
                    return false;
                //overlap check
                if (MainWindow.inst.NoteExists(lane + note.DragLaneDifference, ms + note.DragStartMsDifference) || MainWindow.inst.NoteExists(lane + note.DragLaneDifference, ms + note.DragStartMsDifference + note.DragEndMsDifference) || MainWindow.inst.EndOverlap(lane + note.DragLaneDifference, ms + note.DragStartMsDifference, ms + note.DragStartMsDifference + note.DragEndMsDifference))
                    return false;
            }

            return true;
        }

        public override void OnMouseClick(Point pos, bool right = false)
        {
            draggedstart = null;
            draggedend = null;

            if (hoverend != null)
            {
                MainWindow.inst.SelectedNotes.Clear();
                MainWindow.inst.SelectedEnds.Clear();
                MainWindow.inst.SelectedEnds.Add(hoverend);
                draggedend = hoverend;
            }
            else if (hoverstart != null)
            {
                if (!MainWindow.inst.SelectedNotes.Contains(hoverstart))
                {
                    MainWindow.inst.SelectedNotes.Clear();
                    MainWindow.inst.SelectedEnds.Clear();
                    MainWindow.inst.SelectedNotes.Add(hoverstart);
                }

                draggedstart = hoverstart;
            }
            else
            {
                MainWindow.inst.SelectedNotes.Clear();
                MainWindow.inst.SelectedEnds.Clear();
                dragging = true;
                dragstart = (long)PixelsToMs(pos.Y + noteheight / 2f);
            }

            selectedstart = draggedstart;
            selectedend = draggedend;
            draggedstartorigin = draggedstart != null ? new Note(draggedstart.StartMs, draggedstart.EndMs, draggedstart.Lane) : null;
            draggedendorigin = draggedend != null ? new Note(draggedend.StartMs, draggedend.EndMs, draggedend.Lane) : null;
        }

        private void URRemove(Note note)
        {
            var selectedcheck = MainWindow.inst.SelectedNotes.Count > 0 && MainWindow.inst.SelectedNotes.Contains(note);

            if (selectedcheck)
            {
                var selected = MainWindow.inst.SelectedNotes.ToList();

                MainWindow.inst.UndoRedoManager.AddFunction("Remove Note", () =>
                {
                    foreach (var selectednote in selected)
                        MainWindow.inst.Notes.Add(selectednote);

                    MainWindow.inst.OrderNotes();
                }, () =>
                {
                    foreach (var selectednote in selected)
                        MainWindow.inst.Notes.Remove(selectednote);

                    MainWindow.inst.OrderNotes();
                });
            }
            else
            {
                MainWindow.inst.UndoRedoManager.AddFunction("Remove Note", () =>
                {
                    MainWindow.inst.Notes.Add(note);
                    MainWindow.inst.OrderNotes();
                }, () =>
                {
                    MainWindow.inst.Notes.Remove(note);
                    MainWindow.inst.OrderNotes();
                });
            }
        }

        private void URRemoveEnd(Note note)
        {
            var end = note.EndMs;

            MainWindow.inst.UndoRedoManager.AddFunction("Remove End", () =>
            {
                note.EndMs = end;
            }, () =>
            {
                note.EndMs = note.StartMs;
            });
        }

        public void DeleteNote(bool delete)
        {
            var setting = Settings.FindToggle("RightClickDelete");

            if (delete || Settings.toggles[setting].Toggle)
            {
                if (delete)
                {
                    if (selectedend != null)
                        URRemoveEnd(selectedend);
                    if (selectedstart != null)
                        URRemove(selectedstart);
                }
                else
                {
                    if (hoverend != null)
                        URRemoveEnd(hoverend);
                    if (hoverstart != null)
                        URRemove(hoverstart);
                }

                foreach (var note in MainWindow.inst.SelectedNotes)
                    MainWindow.inst.Notes.Remove(note);

                MainWindow.inst.OrderNotes();
            }
        }

        private void PlaceNote(long lane, long startms, long endms)
        {
            var note = new Note(startms, endms, lane);

            MainWindow.inst.UndoRedoManager.AddFunction("Add Note", () =>
            {
                MainWindow.inst.Notes.Remove(note);
                MainWindow.inst.OrderNotes();
            }, () =>
            {
                MainWindow.inst.Notes.Add(note);
                MainWindow.inst.OrderNotes();
            });

            if (Settings.toggles[Settings.FindToggle("AutoAdvance")].Toggle)
                MainWindow.inst.currentTime = TimeSpan.FromMilliseconds(MainWindow.inst.ClosestBeat((long)MainWindow.inst.currentTime.TotalMilliseconds, true, false));
        }

        private void ReleaseDrag()
        {
            if (draggedstart != draggedstartorigin && draggedstart != null && draggedstartorigin != null)
            {
                var startcopy = new Note(draggedstart.StartMs, draggedstart.EndMs, draggedstart.Lane);
                var origincopy = new Note(draggedstartorigin.StartMs, draggedstartorigin.EndMs, draggedstartorigin.Lane);
                var selected = MainWindow.inst.SelectedNotes.ToList();

                MainWindow.inst.UndoRedoManager.AddFunction("Move Note(s)", () =>
                {
                    foreach (var note in selected)
                    {
                        var msdiff = note.StartMs - startcopy.StartMs;
                        var enddiff = note.EndMs - note.StartMs;
                        var lanediff = note.Lane - startcopy.Lane;

                        note.StartMs = origincopy.StartMs + msdiff;
                        note.EndMs = note.StartMs + enddiff;
                        note.Lane = origincopy.Lane + lanediff;
                    }

                    MainWindow.inst.OrderNotes();
                }, () =>
                {
                    foreach (var note in selected)
                    {
                        var msdiff = note.StartMs - origincopy.StartMs;
                        var enddiff = note.EndMs - note.StartMs;
                        var lanediff = note.Lane - origincopy.Lane;

                        note.StartMs = startcopy.StartMs + msdiff;
                        note.EndMs = note.StartMs + enddiff;
                        note.Lane = startcopy.Lane + lanediff;
                    }

                    MainWindow.inst.OrderNotes();
                }, false);
            }

            if (draggedend != draggedendorigin && draggedend != null && draggedendorigin != null)
            {
                var endcopy = draggedend.EndMs;
                var origincopy = draggedendorigin.EndMs;
                var selected = MainWindow.inst.SelectedEnds.ToList();

                MainWindow.inst.UndoRedoManager.AddFunction("Move End", () =>
                {
                    foreach (var note in selected)
                        note.EndMs = origincopy;
                }, () =>
                {
                    foreach (var note in selected)
                        note.EndMs = endcopy;
                }, false);
            }

            dragging = false;
            draggedstart = null;
            draggedend = null;
            draggedstartorigin = null;
            draggedendorigin = null;
        }

        public override void OnMouseLeave()
        {
            ReleaseDrag();
        }

        public override void OnMouseUp()
        {
            if (dragging)
            {
                long startms = MainWindow.inst.ClosestBeat(dragstart, false, false);
                startms = startms >= 0 ? startms : dragstart;
                startms = (long)MathHelper.Clamp(startms, 0, MainWindow.inst.totalTime.TotalMilliseconds);
                long ms = MainWindow.inst.ClosestBeat((long)PixelsToMs(MainWindow.inst.mouse.Y + noteheight / 2f), false, false);
                ms = ms >= 0 ? ms : (long)PixelsToMs(MainWindow.inst.mouse.Y + noteheight / 2f);
                ms = (long)MathHelper.Clamp(ms, startms, MainWindow.inst.totalTime.TotalMilliseconds);
                var lane = (long)((MainWindow.inst.mouse.X - rect.X) / notewidth);

                if (!MainWindow.inst.NoteExists(lane, startms) && !MainWindow.inst.NoteExists(lane, ms) && !MainWindow.inst.EndOverlap(lane, startms, ms))
                    PlaceNote(lane, startms, ms);
            }

            ReleaseDrag();
        }
    }
}
