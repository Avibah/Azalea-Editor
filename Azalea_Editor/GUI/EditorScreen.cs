using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using Azalea_Editor.Misc;
using OpenTK;

namespace Azalea_Editor.GUI
{
    class EditorScreen : Gui
    {
        public readonly Button tabOptions = new Button(25f, 25f, 256f, 48f, 0, "Options", 24);
        public readonly Button tabTiming = new Button(256f + 25f + 10f, 25f, 256f, 48f, 1, "Timing", 24);
        public readonly Button copyData = new Button(25f, 720f - 64f - 25f, 256f, 48f, 2, "Copy Data", 24);
        public readonly Button Return = new Button(256f + 10f + 25f, 720f - 64f - 25f, 256f, 48f, 3, "Return to Menu", 24);
        public readonly Button PlayPause = new Button((256f + 10f) * 2 + 25f, 720f - 64 - 25f, 48f, 48f, 4, ">", 24);

        public readonly Button timingUpdate0 = new Button(0, 0, 0, 0, 100, "Update", 24);
        public readonly Button timingUpdate1 = new Button(0, 0, 0, 0, 101, "Update", 24);
        public readonly Button timingUpdate2 = new Button(0, 0, 0, 0, 102, "Update", 24);
        public readonly Button timingRemove0 = new Button(0, 0, 0, 0, 103, "X", 50);
        public readonly Button timingRemove1 = new Button(0, 0, 0, 0, 104, "X", 50);
        public readonly Button timingRemove2 = new Button(0, 0, 0, 0, 105, "X", 50);

        public readonly Button velocityUpdate0 = new Button(0, 0, 0, 0, 200, "Update", 24);
        public readonly Button velocityUpdate1 = new Button(0, 0, 0, 0, 201, "Update", 24);
        public readonly Button velocityUpdate2 = new Button(0, 0, 0, 0, 202, "Update", 24);
        public readonly Button velocityRemove0 = new Button(0, 0, 0, 0, 203, "X", 50);
        public readonly Button velocityRemove1 = new Button(0, 0, 0, 0, 204, "X", 50);
        public readonly Button velocityRemove2 = new Button(0, 0, 0, 0, 205, "X", 50);

        public readonly TextBox timingBpm = new TextBox(25f + 50f, 20f + 48f + 20f + 50f, 120f + 60f - 50f, 165f / 4f, true, "", 22);
        public readonly TextBox timingOffset = new TextBox(25f + 50f, 20f + 48f + 20f + 50f + (165f / 4f + 5f), 120f + 60f - 50f, 165f / 4f, true, "", 22);
        public readonly Button timingCurPos = new Button(25f, 20f + 48f + 20f + 50f + (165f / 4f + 5f) * 2f, 120f + 60f, 165f / 4f, 90, "Current Pos", 22);
        public readonly Button timingAdd = new Button(25f, 20f + 48f + 20f + 50f + (165f / 4f + 5f) * 3f, 120f + 60f, 165f / 4f, 91, "Add Point", 22);

        public readonly TextBox velocitySpeed = new TextBox(25f + 50f, 20f + 48f + 20f + 50f + 220f, 120f + 60f - 50f, 165f / 4f, true, "", 22);
        public readonly TextBox velocityOffset = new TextBox(25f + 50f, 20f + 48f + 20f + 50f + (165f / 4f + 5f) + 220f, 120f + 60f - 50f, 165f / 4f, true, "", 22);
        public readonly Button velocityCurPos = new Button(25f, 20f + 48f + 20f + 50f + (165f / 4f + 5f) * 2f + 220f, 120f + 60f, 165f / 4f, 92, "Current Pos", 22);
        public readonly Button velocityAdd = new Button(25f, 20f + 48f + 20f + 50f + (165f / 4f + 5f) * 3f + 220f, 120f + 60f, 165f / 4f, 93, "Add Point", 22);

        public readonly CheckBox rightClickDelete = new CheckBox(25f, 25f + 48f + 20f, 32f, 32f, "Right Click Delete", 20, "RightClickDelete");
        public readonly CheckBox autoAdvance = new CheckBox(25f, 25f + 48f + 20f + 32f + 10f, 32f, 32f, "Auto Advance", 20, "AutoAdvance");
        public readonly CheckBox reverseScroll = new CheckBox(25f, 25f + 48f + 20f + (32f + 10f) * 2f, 32f, 32f, "Reverse Scroll", 20, "ReverseScroll");

        public readonly Slider timeline = new Slider(1280f - 350f - 100f - 20f - 25f, 25f, 25f, 720f - 50f, "Timeline", true, true);
        public readonly Slider tempo = new Slider(25f, 720f - 64f - 25f - 25f, 256f, 25f, "Tempo");
        public readonly Slider beatDivisor = new Slider(256f + 10f + 25f, 720f - 64f - 25f - 25f, 256f, 25f, "BeatDivisor");
        public readonly Slider sfxVol = new Slider(25f, 720f - 64f - 65f - 25f, 256f, 25f, "SFXVolume");
        public readonly Slider musicVol = new Slider(256f + 10f + 25f, 720f - 64f - 65f - 25f, 256f, 25f, "MusicVolume");

        public readonly Slider bpmIndex = new Slider(256f + 10f + 256f + 200f, 25f + 48f + 20f + 50f, 25f, 170f, "BpmIndex");
        public readonly Slider velocityIndex = new Slider(256f + 10f + 256f + 200f, 25f + 48f + 70f + 170f + 50f, 25f, 170f, "VelocityIndex");

        public readonly Grid editorgrid = new Grid(1280f - 350f, 0f, 350f, 720f);
        public readonly Track editortrack = new Track(1280f - 350f - 100f, 0f, 100f, 720f);

        private string CurrentTab;

        public RectangleF timingrect;
        public RectangleF velocityrect;

        public EditorScreen() : base(0, 0, MainWindow.inst.ClientSize.Width, MainWindow.inst.ClientSize.Height)
        {
            var turrect = new RectangleF(tabOptions.rect.X + tabOptions.rect.Width * 3f / 4f, bpmIndex.rect.Y - 5f, bpmIndex.rect.X - tabOptions.rect.X - tabOptions.rect.Width * 3f / 4f, bpmIndex.rect.Height + 10f);
            var turect = new RectangleF(turrect.X + turrect.Width * 2f / 3f, turrect.Y, turrect.Width / 6f, turrect.Height / 3f);
            var trrect = new RectangleF(turrect.X + turrect.Width * 5f / 6f, turrect.Y, turrect.Width / 6f, turrect.Height / 3f);
            var vurrect = new RectangleF(tabOptions.rect.X + tabOptions.rect.Width * 3f / 4f, velocityIndex.rect.Y - 5f, velocityIndex.rect.X - tabOptions.rect.X - tabOptions.rect.Width * 3f / 4f, velocityIndex.rect.Height + 10f);
            var vurect = new RectangleF(vurrect.X + vurrect.Width * 2f / 3f, vurrect.Y, vurrect.Width / 6f, vurrect.Height / 3f);
            var vrrect = new RectangleF(vurrect.X + vurrect.Width * 5f / 6f, vurrect.Y, vurrect.Width / 6f, vurrect.Height / 3f);

            timingUpdate0.originrect = new RectangleF(turect.X, turect.Y, turect.Width, turect.Height);
            timingUpdate1.originrect = new RectangleF(turect.X, turect.Y + turect.Height, turect.Width, turect.Height);
            timingUpdate2.originrect = new RectangleF(turect.X, turect.Y + turect.Height * 2f, turect.Width, turect.Height);
            timingRemove0.originrect = new RectangleF(trrect.X, trrect.Y, trrect.Width, trrect.Height);
            timingRemove1.originrect = new RectangleF(trrect.X, trrect.Y + trrect.Height, trrect.Width, trrect.Height);
            timingRemove2.originrect = new RectangleF(trrect.X, trrect.Y + trrect.Height * 2f, trrect.Width, trrect.Height);

            velocityUpdate0.originrect = new RectangleF(vurect.X, vurect.Y, vurect.Width, vurect.Height);
            velocityUpdate1.originrect = new RectangleF(vurect.X, vurect.Y + vurect.Height, vurect.Width, vurect.Height);
            velocityUpdate2.originrect = new RectangleF(vurect.X, vurect.Y + vurect.Height * 2f, vurect.Width, vurect.Height);
            velocityRemove0.originrect = new RectangleF(vrrect.X, vrrect.Y, vrrect.Width, vrrect.Height);
            velocityRemove1.originrect = new RectangleF(vrrect.X, vrrect.Y + vrrect.Height, vrrect.Width, vrrect.Height);
            velocityRemove2.originrect = new RectangleF(vrrect.X, vrrect.Y + vrrect.Height * 2f, vrrect.Width, vrrect.Height);

            buttons.Add(tabOptions);
            buttons.Add(tabTiming);
            buttons.Add(copyData);
            buttons.Add(Return);
            buttons.Add(PlayPause);

            buttons.Add(timingUpdate0);
            buttons.Add(timingUpdate1);
            buttons.Add(timingUpdate2);
            buttons.Add(timingRemove0);
            buttons.Add(timingRemove1);
            buttons.Add(timingRemove2);

            buttons.Add(velocityUpdate0);
            buttons.Add(velocityUpdate1);
            buttons.Add(velocityUpdate2);
            buttons.Add(velocityRemove0);
            buttons.Add(velocityRemove1);
            buttons.Add(velocityRemove2);

            boxes.Add(timingBpm);
            boxes.Add(timingOffset);
            buttons.Add(timingCurPos);
            buttons.Add(timingAdd);

            boxes.Add(velocitySpeed);
            boxes.Add(velocityOffset);
            buttons.Add(velocityCurPos);
            buttons.Add(velocityAdd);

            checkboxes.Add(rightClickDelete);
            checkboxes.Add(autoAdvance);
            checkboxes.Add(reverseScroll);

            sliders.Add(timeline);
            sliders.Add(tempo);
            sliders.Add(beatDivisor);
            sliders.Add(sfxVol);
            sliders.Add(musicVol);

            sliders.Add(bpmIndex);
            sliders.Add(velocityIndex);

            grid = editorgrid;
            track = editortrack;

            SwitchTab("");
            AlignIndexes();
            OnResize(MainWindow.inst.ClientSize);
        }

        private void AlignIndexes()
        {
            Settings.sliders[bpmIndex.settingindex].Value = MathHelper.Clamp(Settings.sliders[bpmIndex.settingindex].Value, 0, MainWindow.inst.Timings.Count - 1);
            Settings.sliders[bpmIndex.settingindex].Max = MainWindow.inst.Timings.Count - 1;

            Settings.sliders[velocityIndex.settingindex].Value = MathHelper.Clamp(Settings.sliders[velocityIndex.settingindex].Value, 0, MainWindow.inst.Velocities.Count - 1);
            Settings.sliders[velocityIndex.settingindex].Max = MainWindow.inst.Velocities.Count - 1;
        }

        private void RenderTiming(int index, int offset)
        {
            var textheight = TextHeight(30);
            var point = MainWindow.inst.Timings[index];
            var y = timingrect.Y + timingrect.Height / 6f + timingrect.Height / 3f * offset - textheight / 2f;

            RenderText(point.Bpm.ToString(), timingrect.X + 5f, y, 30);
            RenderText(point.Ms.ToString(), timingrect.X + 5f + timingrect.Width / 3f, y, 30);
        }

        private void RenderVelocity(int index, int offset)
        {
            var textheight = TextHeight(30);
            var point = MainWindow.inst.Velocities[index];
            var y = velocityrect.Y + velocityrect.Height / 6f + velocityrect.Height / 3f * offset - textheight / 2f;

            RenderText(point.Velocity.ToString(), velocityrect.X + 5f, y, 30);
            RenderText(point.Ms.ToString(), velocityrect.X + 5f + velocityrect.Width / 3f, y, 30);
        }

        public override void Render(float mousex, float mousey, float frametime)
        {
            var heightdiff = rect.Height / 720f;

            PlayPause.text = MainWindow.inst.MusicPlayer.IsPlaying ? "| |" : ">";
            tabOptions.rect.Y = 25f * heightdiff + (CurrentTab == "Options" ? 10f : 0f);
            tabTiming.rect.Y = 25f * heightdiff + (CurrentTab == "Timing" ? 10f : 0f);

            var textheight = TextHeight(20);
            var timewidth = TextWidth("00:00", 20);
            var charwidth = TextWidth("0", 20);
            GL.Color3(1f, 1f, 1f);

            var sfxindex = Settings.FindSlider("SFXVolume");
            var sfxvalue = Settings.sliders[sfxindex].Value;
            RenderText($"SFX Volume - {(int)(sfxvalue * 100 + 0.5f)}", sfxVol.rect.X, sfxVol.rect.Y - textheight, 20);

            var musicindex = Settings.FindSlider("MusicVolume");
            var musicvalue = Settings.sliders[musicindex].Value;
            RenderText($"Music Volume - {(int)(musicvalue * 100 + 0.5f)}", musicVol.rect.X, musicVol.rect.Y - textheight, 20);

            var tempoindex = Settings.FindSlider("Tempo");
            var tempovalue = Settings.sliders[tempoindex].Value;
            RenderText($"Tempo - {(int)(tempovalue * 100 + 0.5f) + 10}%", tempo.rect.X, tempo.rect.Y - textheight, 20);

            RenderText($"Beat Divisor - {MainWindow.inst.BeatDivisor}", beatDivisor.rect.X, beatDivisor.rect.Y - textheight, 20);
            RenderText($"Zoom - {(int)(MainWindow.inst.Zoom * 100 + 0.5f)}%", beatDivisor.rect.Right + 10f, beatDivisor.rect.Y + beatDivisor.rect.Height / 2f - textheight / 2f, 20);
            RenderText($"{(long)MainWindow.inst.currentTime.TotalMilliseconds}", timeline.rect.X - (long)MainWindow.inst.currentTime.TotalMilliseconds.ToString().Length * charwidth - 10f, timeline.rect.Bottom - textheight, 20);
            RenderText($"{MainWindow.inst.currentTime.Minutes}:{MainWindow.inst.currentTime.Seconds:0#}", timeline.rect.X - timewidth, timeline.rect.Bottom - textheight * 2 - 10f, 20);
            RenderText($"{MainWindow.inst.totalTime.Minutes}:{MainWindow.inst.totalTime.Seconds:0#}", timeline.rect.X - timewidth, timeline.rect.Y, 20);

            bool[] timingsvisible = { false, false, false };
            bool[] velocitiesvisible = { false, false, false };

            if (CurrentTab == "Timing")
            {
                GL.LineWidth(3f);
                timingrect = new RectangleF(tabOptions.rect.X + tabOptions.rect.Width * 3f / 4f, bpmIndex.rect.Y - 5f * heightdiff, bpmIndex.rect.X - tabOptions.rect.X - tabOptions.rect.Width * 6f / 4f, bpmIndex.rect.Height + 10f * heightdiff);
                GLSpecial.Outline(timingrect);
                velocityrect = new RectangleF(tabOptions.rect.X + tabOptions.rect.Width * 3f / 4f, velocityIndex.rect.Y - 5f * heightdiff, velocityIndex.rect.X - tabOptions.rect.X - tabOptions.rect.Width * 3f / 4f, velocityIndex.rect.Height + 10f * heightdiff);
                GLSpecial.Outline(velocityrect);

                GLSpecial.Line(timingrect.X + timingrect.Width / 3f, timingrect.Y, timingrect.X + timingrect.Width / 3f, timingrect.Bottom);
                GLSpecial.Line(timingrect.X + timingrect.Width * 2f / 3f, timingrect.Y, timingrect.X + timingrect.Width * 2f / 3f, timingrect.Bottom);
                GLSpecial.Line(timingrect.X + timingrect.Width * 5f / 6f, timingrect.Y, timingrect.X + timingrect.Width * 5f / 6f, timingrect.Bottom);
                GLSpecial.Line(timingrect.X, timingrect.Y + timingrect.Height / 3f, timingrect.Right, timingrect.Y + timingrect.Height / 3f);
                GLSpecial.Line(timingrect.X, timingrect.Y + timingrect.Height * 2f / 3f, timingrect.Right, timingrect.Y + timingrect.Height * 2f / 3f);

                GLSpecial.Line(velocityrect.X + velocityrect.Width / 3f, velocityrect.Y, velocityrect.X + velocityrect.Width / 3f, velocityrect.Bottom);
                GLSpecial.Line(velocityrect.X + velocityrect.Width * 2f / 3f, velocityrect.Y, velocityrect.X + velocityrect.Width * 2f / 3f, velocityrect.Bottom);
                GLSpecial.Line(velocityrect.X + velocityrect.Width * 5f / 6f, velocityrect.Y, velocityrect.X + velocityrect.Width * 5f / 6f, velocityrect.Bottom);
                GLSpecial.Line(velocityrect.X, velocityrect.Y + velocityrect.Height / 3f, velocityrect.Right, velocityrect.Y + velocityrect.Height / 3f);
                GLSpecial.Line(velocityrect.X, velocityrect.Y + velocityrect.Height * 2f / 3f, velocityrect.Right, velocityrect.Y + velocityrect.Height * 2f / 3f);

                var boxtextheight = TextHeight(30);

                RenderText("BPM", timingrect.X + 5f, timingrect.Y - boxtextheight - 5f, 30);
                RenderText("Ms", timingrect.X + 5f + timingrect.Width / 3f, timingrect.Y - boxtextheight - 5f, 30);
                RenderText("Velocity", velocityrect.X + 5f, velocityrect.Y - boxtextheight - 5f, 30);
                RenderText("Ms", velocityrect.X + 5f + velocityrect.Width / 3f, velocityrect.Y - boxtextheight - 5f, 30);

                RenderText("BPM", tabOptions.rect.X, timingBpm.rect.Y + timingBpm.rect.Height / 2f - textheight / 2f, 20);
                RenderText("Ms", tabOptions.rect.X, timingOffset.rect.Y + timingOffset.rect.Height / 2f - textheight / 2f, 20);
                RenderText("Speed", tabOptions.rect.X, velocitySpeed.rect.Y + velocitySpeed.rect.Height / 2f - textheight / 2f, 20);
                RenderText("Ms", tabOptions.rect.X, velocityOffset.rect.Y + velocityOffset.rect.Height / 2f - textheight / 2f, 20);

                for (int i = 0; i < MainWindow.inst.Timings.Count; i++)
                {
                    if (i >= Settings.sliders[bpmIndex.settingindex].Value && i < Settings.sliders[bpmIndex.settingindex].Value + 3)
                    {
                        var offset = i - (int)Settings.sliders[bpmIndex.settingindex].Value;
                        RenderTiming(i, offset);
                        timingsvisible[offset] = true;
                    }
                        
                }

                for (int i = 0; i < MainWindow.inst.Velocities.Count; i++)
                {
                    if (i >= Settings.sliders[velocityIndex.settingindex].Value && i < Settings.sliders[velocityIndex.settingindex].Value + 3)
                    {
                        var offset = i - (int)Settings.sliders[velocityIndex.settingindex].Value;
                        RenderVelocity(i, offset);
                        velocitiesvisible[offset] = true;
                    }
                }
            }

            timingUpdate0.Visible = timingsvisible[0];
            timingUpdate1.Visible = timingsvisible[1];
            timingUpdate2.Visible = timingsvisible[2];
            timingRemove0.Visible = timingsvisible[0];
            timingRemove1.Visible = timingsvisible[1];
            timingRemove2.Visible = timingsvisible[2];

            velocityUpdate0.Visible = velocitiesvisible[0];
            velocityUpdate1.Visible = velocitiesvisible[1];
            velocityUpdate2.Visible = velocitiesvisible[2];
            velocityRemove0.Visible = velocitiesvisible[0];
            velocityRemove1.Visible = velocitiesvisible[1];
            velocityRemove2.Visible = velocitiesvisible[2];

            base.Render(mousex, mousey, frametime);
        }

        public override void OnResize(Size size)
        {
            rect = new RectangleF(0, 0, size.Width, size.Height);

            base.OnResize(size);
        }

        private void SwitchTab(string tab)
        {
            rightClickDelete.Visible = false;
            autoAdvance.Visible = false;
            reverseScroll.Visible = false;

            bpmIndex.Visible = false;
            velocityIndex.Visible = false;

            timingBpm.Visible = false;
            timingOffset.Visible = false;
            timingCurPos.Visible = false;
            timingAdd.Visible = false;

            velocitySpeed.Visible = false;
            velocityOffset.Visible = false;
            velocityCurPos.Visible = false;
            velocityAdd.Visible = false;

            if (tab != CurrentTab)
            {
                switch (tab)
                {
                    case "Options":
                        CurrentTab = tab;

                        rightClickDelete.Visible = true;
                        autoAdvance.Visible = true;
                        reverseScroll.Visible = true;
                        break;
                    case "Timing":
                        CurrentTab = tab;

                        bpmIndex.Visible = true;
                        velocityIndex.Visible = true;

                        timingBpm.Visible = true;
                        timingOffset.Visible = true;
                        timingCurPos.Visible = true;
                        timingAdd.Visible = true;

                        velocitySpeed.Visible = true;
                        velocityOffset.Visible = true;
                        velocityCurPos.Visible = true;
                        velocityAdd.Visible = true;
                        break;
                }
            }
            else
                CurrentTab = null;
        }

        public override void OnButtonClicked(int id)
        {
            switch (id)
            {
                case 0:
                    SwitchTab("Options");
                    break;
                case 1:
                    SwitchTab("Timing");
                    break;
                case 2:
                    try
                    {
                        Clipboard.SetText(MainWindow.inst.ToData());
                    }
                    catch { }
                    break;
                case 3:
                    MainWindow.inst.SwitchScreen(new MenuScreen());
                    break;
                case 4:
                    if (MainWindow.inst.MusicPlayer.IsPlaying)
                        MainWindow.inst.MusicPlayer.Pause();
                    else
                        MainWindow.inst.MusicPlayer.Play();
                    break;
                case 90:
                    timingOffset.text = ((long)MainWindow.inst.currentTime.TotalMilliseconds).ToString();
                    break;
                case 91:
                    if (float.TryParse(timingBpm.text, out var tbpma) && long.TryParse(timingOffset.text, out var tmsa) && !MainWindow.inst.TimingExists(tmsa))
                    {
                        MainWindow.inst.Timings.Add(new TimingPoint(tbpma, tmsa));
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 92:
                    velocityOffset.text = ((long)MainWindow.inst.currentTime.TotalMilliseconds).ToString();
                    break;
                case 93:
                    if (float.TryParse(velocitySpeed.text, out var vspa) && long.TryParse(velocityOffset.text, out var vmsa) && !MainWindow.inst.VelocityExists(vmsa))
                    {
                        MainWindow.inst.Velocities.Add(new VelocityPoint(vspa, vmsa));
                        MainWindow.inst.OrderVelocities();
                    }
                    break;
                case 100:
                    if (float.TryParse(timingBpm.text, out var tbpmu0) && long.TryParse(timingOffset.text, out var tmsu0) && !MainWindow.inst.TimingExists(tmsu0))
                    {
                        var point = MainWindow.inst.Timings[(int)Settings.sliders[bpmIndex.settingindex].Value];
                        point.Bpm = tbpmu0;
                        point.Ms = tmsu0;
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 101:
                    if (float.TryParse(timingBpm.text, out var tbpmu1) && long.TryParse(timingOffset.text, out var tmsu1) && !MainWindow.inst.TimingExists(tmsu1))
                    {
                        var point = MainWindow.inst.Timings[(int)Settings.sliders[bpmIndex.settingindex].Value + 1];
                        point.Bpm = tbpmu1;
                        point.Ms = tmsu1;
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 102:
                    if (float.TryParse(timingBpm.text, out var tbpmu2) && long.TryParse(timingOffset.text, out var tmsu2) && !MainWindow.inst.TimingExists(tmsu2))
                    {
                        var point = MainWindow.inst.Timings[(int)Settings.sliders[bpmIndex.settingindex].Value + 2];
                        point.Bpm = tbpmu2;
                        point.Ms = tmsu2;
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 103:
                    MainWindow.inst.Timings.RemoveAt((int)Settings.sliders[bpmIndex.settingindex].Value);
                    break;
                case 104:
                    MainWindow.inst.Timings.RemoveAt((int)Settings.sliders[bpmIndex.settingindex].Value + 1);
                    break;
                case 105:
                    MainWindow.inst.Timings.RemoveAt((int)Settings.sliders[bpmIndex.settingindex].Value + 2);
                    break;
                case 200:
                    if (float.TryParse(velocitySpeed.text, out var vspu0) && long.TryParse(velocityOffset.text, out var vmsu0) && !MainWindow.inst.VelocityExists(vmsu0))
                    {
                        var point = MainWindow.inst.Velocities[(int)Settings.sliders[velocityIndex.settingindex].Value];
                        point.Velocity = vspu0;
                        point.Ms = vmsu0;
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 201:
                    if (float.TryParse(velocitySpeed.text, out var vspu1) && long.TryParse(velocityOffset.text, out var vmsu1) && !MainWindow.inst.VelocityExists(vmsu1))
                    {
                        var point = MainWindow.inst.Velocities[(int)Settings.sliders[velocityIndex.settingindex].Value + 1];
                        point.Velocity = vspu1;
                        point.Ms = vmsu1;
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 202:
                    if (float.TryParse(velocitySpeed.text, out var vspu2) && long.TryParse(velocityOffset.text, out var vmsu2) && !MainWindow.inst.VelocityExists(vmsu2))
                    {
                        var point = MainWindow.inst.Velocities[(int)Settings.sliders[velocityIndex.settingindex].Value + 2];
                        point.Velocity = vspu2;
                        point.Ms = vmsu2;
                        MainWindow.inst.OrderTimings();
                    }
                    break;
                case 203:
                    MainWindow.inst.Velocities.RemoveAt((int)Settings.sliders[velocityIndex.settingindex].Value);
                    break;
                case 204:
                    MainWindow.inst.Velocities.RemoveAt((int)Settings.sliders[velocityIndex.settingindex].Value + 1);
                    break;
                case 205:
                    MainWindow.inst.Velocities.RemoveAt((int)Settings.sliders[velocityIndex.settingindex].Value + 2);
                    break;
            }

            base.OnButtonClicked(id);
        }
    }
}
