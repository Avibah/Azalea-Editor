using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Azalea_Editor.Properties;
using System.Windows.Forms;
using Azalea_Editor.GUI;
using System.Drawing;
using OpenTK.Input;
using System.ComponentModel;
using Azalea_Editor.Misc;
using System.IO;
using System.Net;
using System.Json;

namespace Azalea_Editor
{
    class MainWindow : GameWindow
    {
        public static MainWindow inst;

        public Point mouse = new Point(-1, -1);
        public Gui CurrentWindow { get; private set; }

        public TimeSpan currentTime;
        public TimeSpan totalTime;

        public bool ctrlHeld;
        public bool altHeld;
        public bool shiftHeld;

        public FreeTypeFont font;
        public MusicPlayer MusicPlayer = new MusicPlayer() { Volume = 0.2f };
        public SoundPlayer SoundPlayer = new SoundPlayer() { Volume = 0.2f };
        public UndoRedoManager UndoRedoManager = new UndoRedoManager();

        public string filename;
        public long id;
        public List<Note> Notes = new List<Note>();
        public List<TimingPoint> Timings = new List<TimingPoint>();
        public List<VelocityPoint> Velocities = new List<VelocityPoint>();
        public List<Note> SelectedNotes = new List<Note>();
        public List<Note> SelectedEnds = new List<Note>();

        public int NoteSoundIndex = -1;

        public int BeatDivisor = 4;
        public float Zoom = 1f;
        public float tempo = 1f;
        public float NoteStep
        {
            get => Zoom * 1024f;
            set => NoteStep = value;
        }

        public Dictionary<string, Color> colors = new Dictionary<string, Color>();

        public Note lastplayed;

        public MainWindow() : base(1280, 720, GraphicsMode.Default, Application.ProductName.Replace("_", " ") + " " + Application.ProductVersion)
        {
            VSync = VSyncMode.On;
            TargetUpdatePeriod = 1f / 60f;

            Icon = Resources.az4;

            inst = this;
            Settings.Setup();

            NoteSoundIndex = Settings.FindString("NoteSound");
            var fontindex = Settings.FindString("FontName");
            font = new FreeTypeFont($"misc/fonts/{Settings.strings[fontindex]}.ttf", 32);

            SwitchScreen(new MenuScreen());

            var sfxindex = Settings.FindSlider("SFXVolume");
            var musicindex = Settings.FindSlider("MusicVolume");

            SoundPlayer = new SoundPlayer { Volume = Settings.sliders[sfxindex].Value };
            MusicPlayer = new MusicPlayer { Volume = Settings.sliders[musicindex].Value };
        }

        public bool FileExists(string filetype, string fileName)
        {
            var files = Directory.GetFiles(filetype);
            
            foreach (var file in files)
            {
                if (Path.GetFileNameWithoutExtension(file) == fileName)
                    return true;
            }

            return false;
        }

        public void OrderNotes()
        {
            Notes = Notes.OrderBy(o => o.StartMs).ToList();
        }

        public void OrderTimings()
        {
            Timings = Timings.OrderBy(o => o.Ms).ToList();
        }

        public void OrderVelocities()
        {
            Velocities = Velocities.OrderBy(o => o.Ms).ToList();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (MusicPlayer.IsPlaying)
            {
                Note played = Notes.LastOrDefault(n => n.StartMs <= (long)currentTime.TotalMilliseconds);

                if (played != lastplayed)
                {
                    lastplayed = played;

                    if (lastplayed != null)
                        SoundPlayer.Play(Settings.strings[NoteSoundIndex].Value);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);

            font = new FreeTypeFont("misc/fonts/FreeSans.ttf", 32);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.PushMatrix();

            if (MusicPlayer.IsPlaying)
                currentTime = MusicPlayer.CurrentTime;

            CurrentWindow?.Render(mouse.X, mouse.Y, (float)e.Time);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.PopMatrix();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            ClientSize = new Size(Math.Max(1280, ClientSize.Width), Math.Max(720, ClientSize.Height));

            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            var m = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);
            GL.LoadMatrix(ref m);

            CurrentWindow?.OnResize(ClientSize);
            OnRenderFrame(new FrameEventArgs());
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (CurrentWindow is EditorScreen editor)
            {
                if (ctrlHeld)
                    Zoom = MathHelper.Clamp(Zoom + 0.1f * (e.DeltaPrecise > 0 ? 1 : -1), 0.1f, 5f);
                else if (shiftHeld)
                {
                    BeatDivisor = MathHelper.Clamp(BeatDivisor + 1 * (e.DeltaPrecise > 0 ? 1 : -1), 1, 32);
                    Settings.sliders[Settings.FindSlider("BeatDivisor")].Value = BeatDivisor - 1;
                }
                else
                {
                    if (editor.timingrect.Contains(e.Position))
                    {
                        var slider = Settings.sliders[editor.bpmIndex.settingindex];
                        slider.Value = MathHelper.Clamp(slider.Value + 1 * (e.DeltaPrecise > 0 ? -1 : 1), 0, slider.Max);
                    }
                    else if (editor.velocityrect.Contains(e.Position))
                    {
                        var slider = Settings.sliders[editor.velocityIndex.settingindex];
                        slider.Value = MathHelper.Clamp(slider.Value + 1 * (e.DeltaPrecise > 0 ? -1 : 1), 0, slider.Max);
                    }
                    else
                    {
                        MusicPlayer.Pause();

                        var reverse = Settings.toggles[editor.reverseScroll.settingindex].Toggle;

                        var ms = ClosestBeat((long)currentTime.TotalMilliseconds, true, reverse ? e.DeltaPrecise > 0 : e.DeltaPrecise < 0);
                        ms = ms >= 0 ? ms : (long)(currentTime.TotalMilliseconds + 100f / Zoom * (e.DeltaPrecise > 0 ? 1 : -1) * (reverse ? -1 : 1));
                        ms = (long)MathHelper.Clamp(ms, 0, totalTime.TotalMilliseconds);

                        currentTime = TimeSpan.FromMilliseconds(ms);
                    }
                }
            }
            else if (CurrentWindow is SettingsScreen settings)
            {
                if (e.Position.X < ClientSize.Width / 2f)
                {
                    var slider = Settings.sliders[settings.keybindIndex.settingindex];
                    slider.Value = MathHelper.Clamp(slider.Value + 1 * (e.DeltaPrecise > 0 ? -1 : 1), 0, slider.Max);
                }
                else
                {
                    var slider = Settings.sliders[settings.colorIndex.settingindex];
                    slider.Value = MathHelper.Clamp(slider.Value + 1 * (e.DeltaPrecise > 0 ? -1 : 1), 0, slider.Max);
                }
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            mouse = e.Position;

            CurrentWindow?.OnMouseMove(e.Position);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                CurrentWindow?.OnMouseClick(e.Position);
            else if (e.Button == MouseButton.Right)
                CurrentWindow?.OnRightClick(e.Position);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            CurrentWindow?.OnMouseUp();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            CurrentWindow?.OnMouseLeave();

            mouse = new Point(-1, -1);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            ctrlHeld = e.Control;
            altHeld = e.Alt;
            shiftHeld = e.Shift;

            if (!BoxFocused())
            {
                var name = Settings.CompareKeybind(e.Key, ctrlHeld, altHeld, shiftHeld);

                switch (name)
                {
                    case "Lane0":
                    case "Lane1":
                    case "Lane2":
                    case "Lane3":
                        var lane = int.Parse(name.Replace("Lane", ""));

                        if (!NoteExists(lane, (long)currentTime.TotalMilliseconds))
                        {
                            var note = new Note((long)currentTime.TotalMilliseconds, (long)currentTime.TotalMilliseconds, lane);

                            UndoRedoManager.AddFunction("Add Note", () =>
                            {
                                Notes.Remove(note);
                                OrderNotes();
                            }, () =>
                            {
                                Notes.Add(note);
                                OrderNotes();
                            });

                            if (Settings.toggles[Settings.FindToggle("AutoAdvance")].Toggle)
                                currentTime = TimeSpan.FromMilliseconds(ClosestBeat((long)currentTime.TotalMilliseconds, true, false));
                        }
                        break;
                    case "PlayPause":
                        if (MusicPlayer.IsPlaying)
                            MusicPlayer.Pause();
                        else
                            MusicPlayer.Play();
                        break;
                    case "Save":
                        SaveChart(true);
                        break;
                    case "SaveAs":
                        SaveChart(true, true);
                        break;
                    case "Undo":
                        UndoRedoManager.Undo();
                        break;
                    case "Redo":
                        UndoRedoManager.Redo();
                        break;
                    case "Copy":
                        var notescopy = SelectedNotes.ToList();
                        Clipboard.SetData("Notes", notescopy);
                        break;
                    case "Paste":
                        var notespaste = ((List<Note>)Clipboard.GetData("Notes")).ToList();
                        var minms = notespaste.Min(n => n.StartMs);

                        foreach (var note in notespaste)
                        {
                            note.StartMs = note.StartMs - minms + (long)currentTime.TotalMilliseconds;
                            note.EndMs = note.EndMs - minms + (long)currentTime.TotalMilliseconds;
                        }

                        UndoRedoManager.AddFunction("Paste Notes", () =>
                        {
                            SelectedNotes.Clear();
                            SelectedEnds.Clear();
                            foreach (var note in notespaste)
                                Notes.Remove(note);

                            OrderNotes();
                        }, () =>
                        {
                            SelectedNotes.Clear();
                            SelectedEnds.Clear();
                            SelectedNotes.AddRange(notespaste);
                            Notes.AddRange(notespaste);

                            OrderNotes();
                        });
                        break;
                }
            }
            
            CurrentWindow?.OnKeyDown(e.Key, e.Control);
        }

        protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if (BoxFocused())
                CurrentWindow?.OnKeyPress(e.KeyChar);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            ctrlHeld = e.Control;
            altHeld = e.Alt;
            shiftHeld = e.Shift;
        }

        public bool BoxFocused()
        {
            if (CurrentWindow != null)
            {
                foreach (var box in CurrentWindow?.boxes)
                {
                    if (box.focused)
                        return true;
                }
            }

            return false;
        }

        public void SwitchScreen(Gui screen)
        {
            if (screen is EditorScreen)
            {
                var index = Settings.FindSlider("Timeline");
                Settings.sliders[index].Value = (float)currentTime.TotalMilliseconds;
                Settings.sliders[index].Max = (float)totalTime.TotalMilliseconds;
                Settings.sliders[index].Step = (int)(totalTime.TotalMilliseconds / 500f);
            }

            if (!(CurrentWindow is EditorScreen) || SaveChart(false))
            {
                CurrentWindow?.OnClosing();
                Settings.Save();
                MusicPlayer.Reset();

                CurrentWindow = screen;
            }
        }

        public bool FromData(string data)
        {
            try
            {
                var split = data.Split('/');
                var info = split[0].Split(',');
                
                id = long.Parse(info[0]);

                if (split.Count() == 4)
                {
                    try
                    {
                        var timings = split[1].Split(',');
                        var velocities = split[2].Split(',');
                        var notes = split[3].Split(',');

                        if (split[1] != "")
                        {
                            foreach (var point in timings)
                            {
                                var newsplit = point.Split('|');
                                var ms = long.Parse(newsplit[0]);
                                var bpm = float.Parse(newsplit[1]);

                                Timings.Add(new TimingPoint(bpm, ms));
                            }
                        }

                        if (split[2] != "")
                        {
                            foreach (var velocity in velocities)
                            {
                                var newsplit = velocity.Split('|');
                                var ms = long.Parse(newsplit[0]);
                                var speed = float.Parse(newsplit[1]);

                                Velocities.Add(new VelocityPoint(speed, ms));
                            }
                        }

                        if (split[3] != "")
                        {
                            foreach (var note in notes)
                            {
                                var newsplit = note.Split('|');
                                var mssplit = newsplit[0].Split(':');
                                var startms = long.Parse(mssplit[0]);
                                var endms = mssplit.Count() == 2 ? long.Parse(mssplit[1]) : long.Parse(mssplit[0]);
                                var lane = long.Parse(newsplit[1]) - 1;

                                Notes.Add(new Note(startms, endms, lane));
                            }

                            OrderNotes();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Failed to parse data!\nCheck if the formatting is correct", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                MessageBox.Show("Failed to parse data!\nCheck if the formatting is correct", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public string ToData()
        {
            ScanMap();

            var info = id.ToString();

            var timings = new List<string>();
            foreach (var point in Timings)
                timings.Add($"{point.Ms}|{point.Bpm}");

            var velocities = new List<string>();
            foreach (var velocity in Velocities)
                velocities.Add($"{velocity.Ms}|{velocity.Velocity}");

            var notes = new List<string>();
            foreach (var note in Notes)
            {
                var ms = note.StartMs == note.EndMs ? note.StartMs.ToString() : $"{note.StartMs}:{note.EndMs}";
                notes.Add($"{ms}|{note.Lane + 1}");
            }

            var final = $"{info}/{string.Join(",", timings)}/{string.Join(",", velocities)}/{string.Join(",", notes)}";
            return final;
        }

        public void LoadProperties(string filepath)
        {
            var file = Path.ChangeExtension(filepath, ".json");

            if (File.Exists(file))
            {
                var result = (JsonObject)JsonValue.Parse(File.ReadAllText(file));

                if (result.TryGetValue("divisor", out var value))
                    BeatDivisor = value;
                if (result.TryGetValue("currentTime", out value))
                    currentTime = TimeSpan.FromMilliseconds(value);
            }
        }

        public void LoadChart(bool file, string filepath)
        {
            try
            {
                currentTime = TimeSpan.Zero;
                Settings.sliders[Settings.FindSlider("Tempo")].Value = 0.9f;
                Settings.sliders[Settings.FindSlider("BeatDivisor")].Value = 3f;
                BeatDivisor = 4;
                tempo = 1f;
                UndoRedoManager.Clear();
                Notes.Clear();
                Timings.Clear();
                Velocities.Clear();
                SelectedNotes.Clear();
                SelectedEnds.Clear();

                if (file)
                {
                    var data = File.ReadAllText(filepath);
                    if (!FromData(data))
                        return;
                    LoadProperties(filepath);
                    Settings.sliders[Settings.FindSlider("BeatDivisor")].Value = BeatDivisor - 1;
                }
                else
                {
                    if (!FromData(filepath))
                        return;
                }

                if (LoadAudio(id))
                {
                    MusicPlayer.Load($"cache/{id}.azl");
                    totalTime = MusicPlayer.TotalTime;
                    filename = file ? filepath : null;
                    SwitchScreen(new EditorScreen());
                }
            }
            catch
            {

            }
        }

        public bool LoadAudio(long ID)
        {
            try
            {
                if (!File.Exists($"cache/{ID}.azl"))
                {
                    using (var wc = new SecureWebClient())
                        wc.DownloadFile("https://assetdelivery.roblox.com/v1/asset/?id=" + ID, $"cache/{ID}.azl");
                }

                return true;
            }
            catch (Exception e)
            {
                var message = MessageBox.Show($"Failed to download asset with id '{ID}':\n\n{e.Message}\n\nWould you like to import a file with this id instead?", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (message == DialogResult.OK)
                {
                    using (var dialog = new OpenFileDialog
                    {
                        Title = "Select Audio File",
                        Filter = "Audio Files (*.mp3;*.ogg;*.wav;*.azl)|*.mp3;*.ogg;*.wav;*.azl"
                    })
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            File.Copy(dialog.FileName, $"cache/{ID}.azl", true);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void SaveProperties(string filepath)
        {
            var file = Path.ChangeExtension(filepath, ".json");

            var json = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>())
            {
                {"divisor", BeatDivisor},
                {"currentTime", currentTime.TotalMilliseconds}
            };

            File.WriteAllText(file, json.ToString());
        }

        public void ScanMap()
        {
            var outofbounds = false;

            foreach (var note in Notes)
            {
                if (note.Lane < 0 || note.Lane > 3)
                    outofbounds = true;

                var origin = new Note(note.StartMs, note.EndMs, note.Lane);
                note.StartMs = -1;
                note.EndMs = -1;
                note.Lane = -1;

                outofbounds = outofbounds || NoteExists(origin.Lane, origin.StartMs) || EndOverlap(origin.Lane, origin.StartMs, origin.EndMs);

                note.StartMs = origin.StartMs;
                note.EndMs = origin.EndMs;
                note.Lane = origin.Lane;
            }

            if (outofbounds)
                MessageBox.Show("NOTE: This map will not function ingame due to overlapping notes or notes outside the grid boundaries.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public bool SaveChart(bool forced, bool fileforced = false)
        {
            var data = ToData();

            if ((filename == null && (Notes.Count > 0 || Timings.Count > 0 || Velocities.Count > 0)) || (filename != null && data != File.ReadAllText(filename)))
            {
                var prompt = DialogResult.No;
                if (!forced)
                    prompt = MessageBox.Show("Would you like to save before closing?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (forced || prompt == DialogResult.Yes)
                {
                    if (filename == null || fileforced)
                    {
                        using (var dialog = new SaveFileDialog
                        {
                            Title = "Save Chart As",
                            Filter = "Text Documents (*.txt)|*.txt"
                        })
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                File.WriteAllText(dialog.FileName, data);
                                SaveProperties(dialog.FileName);
                            }
                        }
                    }
                    else
                    {
                        File.WriteAllText(filename, data);
                        SaveProperties(filename);
                    }
                }
                else if (prompt == DialogResult.Cancel)
                    return false;
            }

            return true;
        }

        public float PixelsToMs(float pixels)
        {
            return pixels * 1000f / NoteStep + (float)currentTime.TotalMilliseconds;
        }

        public float MsToPixels(float ms, bool offset = true)
        {
            return (ms - (offset ? (float)currentTime.TotalMilliseconds : 0)) / 1000f * NoteStep;
        }

        public bool TimingExists(long ms)
        {
            foreach (var timing in Timings)
            {
                if (timing.Ms == ms)
                    return true;
            }

            return false;
        }

        public bool VelocityExists(long ms)
        {
            foreach (var velocity in Velocities)
            {
                if (velocity.Ms == ms)
                    return true;
            }

            return false;
        }

        public bool NoteExists(long lane, long ms)
        {
            foreach (var note in Notes)
            {
                if (note.Lane == lane && (note.StartMs == ms || (note.StartMs < ms && note.EndMs >= ms)))
                    return true;
            }

            return false;
        }

        public bool EndOverlap(long lane, long startms, long endms)
        {
            foreach (var note in Notes)
            {
                if (note.Lane == lane && note.StartMs < endms && note.EndMs > startms)
                    return true;
            }

            return false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (CurrentWindow is EditorScreen)
                e.Cancel = !SaveChart(false);
            Settings.Save();
        }

        public long ClosestBeat(long ms, bool next, bool negative)
        {
            var closest = -1L;
            var nextstart = totalTime.TotalMilliseconds;

            var bpm = new TimingPoint(0, 0);
            var prevbpm = new TimingPoint(0, 0);
            for (int i = 0; i < Timings.Count; i++)
            {
                if (Timings[i].Ms <= ms)
                    bpm = Timings[i];
                if (i + 1 < Timings.Count)
                    nextstart = Timings[i + 1].Ms;
                if (i > 0)
                    prevbpm = Timings[i - 1];
                if (Timings.IndexOf(bpm) == Timings.Count - 1)
                    nextstart = totalTime.TotalMilliseconds;
                if (Timings.IndexOf(bpm) == 0)
                    prevbpm = new TimingPoint(0, 0);
            }

            if (bpm.Bpm > 0)
            {
                var step = 60000 / bpm.Bpm / BeatDivisor;
                var offset = bpm.Ms % step;
                closest = (long)Math.Round((long)Math.Round((ms - offset) / step) * step + offset);

                if (next)
                {
                    if (negative)
                    {
                        if (closest - step < bpm.Ms)
                            closest = ClosestBehind(closest, prevbpm);
                        else
                            closest = (long)(closest - step);
                    }
                    else
                        closest = (long)Math.Min(closest + step, nextstart);
                }
            }

            return closest;
        }

        public long ClosestBehind(long ms, TimingPoint bpm)
        {
            var closest = -1L;

            if (bpm.Bpm > 0)
            {
                var step = 60000 / bpm.Bpm / BeatDivisor;
                var offset = bpm.Ms % step;
                closest = (long)Math.Round((long)Math.Round((ms - offset) / step) * step + offset);

                if (closest >= ms)
                    closest = (long)(closest - step);
            }

            return closest;
        }
    }

    class SecureWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            if (request != null)
            {
                request.UserAgent = "RobloxProxy";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }
            return request;
        }
    }
}
