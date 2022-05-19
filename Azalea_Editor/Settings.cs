using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using System.Json;
using System.IO;
using System.Drawing;

namespace Azalea_Editor
{
    class Settings
    {
        public static List<ToggleSetting> toggles = new List<ToggleSetting>();
        public static List<SliderSetting> sliders = new List<SliderSetting>();
        public static List<FloatSetting> floats = new List<FloatSetting>();
        public static List<Keybind> keybinds = new List<Keybind>();
        public static List<ColorSetting> colors = new List<ColorSetting>();
        public static List<StringSetting> strings = new List<StringSetting>();

        public static void Setup()
        {
            toggles.Add(new ToggleSetting("RightClickDelete", false));
            toggles.Add(new ToggleSetting("AutoAdvance", false));
            toggles.Add(new ToggleSetting("ReverseScroll", false));

            sliders.Add(new SliderSetting("Timeline", 0f, 1f, 1f));
            sliders.Add(new SliderSetting("Tempo", 0.9f, 1.9f, 0.1f));
            sliders.Add(new SliderSetting("SFXVolume", 0.2f, 1f, 0.02f));
            sliders.Add(new SliderSetting("MusicVolume", 0.2f, 1f, 0.02f));
            sliders.Add(new SliderSetting("BpmIndex", 0f, 0f, 1f));
            sliders.Add(new SliderSetting("VelocityIndex", 0f, 0f, 1f));
            sliders.Add(new SliderSetting("KeybindIndex", 0f, 0f, 1f));
            sliders.Add(new SliderSetting("ColorIndex", 0f, 0f, 1f));
            sliders.Add(new SliderSetting("BeatDivisor", 3f, 31f, 1f));

            keybinds.Add(new Keybind("Lane0", Key.Number1, false, false, false));
            keybinds.Add(new Keybind("Lane1", Key.Number2, false, false, false));
            keybinds.Add(new Keybind("Lane2", Key.Number3, false, false, false));
            keybinds.Add(new Keybind("Lane3", Key.Number4, false, false, false));
            keybinds.Add(new Keybind("Delete", Key.Delete, false, false, false));
            keybinds.Add(new Keybind("Save", Key.S, true, false, false));
            keybinds.Add(new Keybind("SaveAs", Key.S, true, false, true));
            keybinds.Add(new Keybind("PlayPause", Key.Space, false, false, false));
            keybinds.Add(new Keybind("Undo", Key.Z, true, false, false));
            keybinds.Add(new Keybind("Redo", Key.Y, true, false, false));
            keybinds.Add(new Keybind("Copy", Key.C, true, false, false));
            keybinds.Add(new Keybind("Paste", Key.V, true, false, false));

            colors.Add(new ColorSetting("Lane0", Color.FromArgb(255, 255, 255, 255)));
            colors.Add(new ColorSetting("Lane0End", Color.FromArgb(255, 255, 0, 127)));
            colors.Add(new ColorSetting("Lane1", Color.FromArgb(255, 255, 0, 127)));
            colors.Add(new ColorSetting("Lane1End", Color.FromArgb(255, 255, 255, 255)));
            colors.Add(new ColorSetting("Lane2", Color.FromArgb(255, 255, 0, 127)));
            colors.Add(new ColorSetting("Lane2End", Color.FromArgb(255, 255, 255, 255)));
            colors.Add(new ColorSetting("Lane3", Color.FromArgb(255, 255, 255, 255)));
            colors.Add(new ColorSetting("Lane3End", Color.FromArgb(255, 255, 0, 127)));
            colors.Add(new ColorSetting("Divisor1", Color.FromArgb(255, 255, 150, 50)));
            colors.Add(new ColorSetting("Divisor2", Color.FromArgb(255, 196, 50, 50)));

            strings.Add(new StringSetting("FontName", "FreeSans"));
            strings.Add(new StringSetting("ClickSound", "click"));
            strings.Add(new StringSetting("NoteSound", "note"));

            Load();

            sliders[FindSlider("BpmIndex")].Value = 0f;
            sliders[FindSlider("VelocityIndex")].Value = 0f;
            sliders[FindSlider("KeybindIndex")].Value = 0f;
            sliders[FindSlider("ColorIndex")].Value = 0f;

            MainWindow.inst.SoundPlayer.Volume = sliders[FindSlider("SFXVolume")].Value;
            MainWindow.inst.MusicPlayer.Volume = sliders[FindSlider("MusicVolume")].Value;
        }

        public static void Save()
        {
            var togglejson = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>());
            foreach (var toggle in toggles)
                togglejson.Add(toggle.Name, toggle.Toggle);

            var sliderjson = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>());
            foreach (var slider in sliders)
                sliderjson.Add(slider.Name, new JsonArray(slider.Value, slider.Max, slider.Step));

            var floatjson = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>());
            foreach (var floatsetting in floats)
                floatjson.Add(floatsetting.Name, floatsetting.Value);

            var keybindjson = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>());
            foreach (var keybind in keybinds)
                keybindjson.Add(keybind.Name, new JsonArray(keybind.Key.ToString(), keybind.CTRL, keybind.ALT, keybind.SHIFT));

            var colorjson = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>());
            foreach (var color in colors)
                colorjson.Add(color.Name, new JsonArray(color.Color.R, color.Color.G, color.Color.B));

            var stringjson = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>());
            foreach (var stringsetting in strings)
                stringjson.Add(stringsetting.Name, stringsetting.Value);

            var final = new JsonObject(Array.Empty<KeyValuePair<string, JsonValue>>())
            {
                {"Toggles", togglejson },
                {"Sliders", sliderjson },
                {"Floats", floatjson },
                {"Keybinds", keybindjson },
                {"Colors", colorjson },
                {"Strings", stringjson },
            };

            try
            {
                File.WriteAllText("settings.json", FormatJson(final.ToString()));
            }
            catch { }

            MainWindow.inst.colors.Clear();
            foreach (var color in colors)
                MainWindow.inst.colors.Add(color.Name, color.Color);
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists("settings.json"))
                    return;

                var json = (JsonObject)JsonValue.Parse(File.ReadAllText("settings.json"));
                JsonValue value;

                if (json.TryGetValue("Toggles", out value))
                {
                    var togglejson = (JsonObject)value;

                    foreach (var key in togglejson)
                    {
                        var index = FindToggle(key.Key);

                        toggles[index].Toggle = key.Value;
                    }
                }

                if (json.TryGetValue("Sliders", out value))
                {
                    var sliderjson = (JsonObject)value;

                    foreach (var key in sliderjson)
                    {
                        var index = FindSlider(key.Key);

                        sliders[index].Value = key.Value[0];
                        sliders[index].Max = key.Value[1];
                        sliders[index].Step = key.Value[2];
                    }
                }

                if (json.TryGetValue("Floats", out value))
                {
                    var floatjson = (JsonObject)value;

                    foreach (var key in floatjson)
                    {
                        var index = FindFloat(key.Key);

                        floats[index].Value = key.Value;
                    }
                }

                if (json.TryGetValue("Keybinds", out value))
                {
                    var keybindjson = (JsonObject)value;

                    foreach (var key in keybindjson)
                    {
                        var index = FindKeybind(key.Key);

                        keybinds[index].Key = (Key)Enum.Parse(typeof(Key), key.Value[0], true);
                        keybinds[index].CTRL = key.Value[1];
                        keybinds[index].ALT = key.Value[2];
                        keybinds[index].SHIFT = key.Value[3];
                    }
                }

                if (json.TryGetValue("Colors", out value))
                {
                    var colorjson = (JsonObject)value;

                    foreach (var key in colorjson)
                    {
                        var index = FindColor(key.Key);

                        colors[index].Color = Color.FromArgb(255, key.Value[0], key.Value[1], key.Value[2]);
                    }
                }

                if (json.TryGetValue("Strings", out value))
                {
                    var stringjson = (JsonObject)value;

                    foreach (var key in stringjson)
                    {
                        var index = FindString(key.Key);

                        strings[index].Value = key.Value;
                    }
                }
            }
            catch { }

            MainWindow.inst.colors.Clear();
            foreach (var color in colors)
                MainWindow.inst.colors.Add(color.Name, color.Color);
        }
        
        public static int FindToggle(string name)
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static int FindSlider(string name)
        {
            for (int i = 0; i < sliders.Count; i++)
            {
                if (sliders[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static int FindFloat(string name)
        {
            for (int i = 0; i < floats.Count; i++)
            {
                if (floats[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static int FindKeybind(string name)
        {
            for (int i = 0; i < keybinds.Count; i++)
            {
                if (keybinds[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static int FindColor(string name)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static int FindString(string name)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static string CompareKeybind(Key key, bool ctrl, bool alt, bool shift)
        {
            foreach (var keybind in keybinds)
            {
                if (keybind.Key == key && keybind.CTRL == ctrl && keybind.ALT == alt && keybind.SHIFT == shift)
                    return keybind.Name;
            }

            return "";
        }



        public static string FormatJson(string json, string indent = "     ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in json ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }
    }

    [Serializable]
    class ToggleSetting
    {
        public string Name;
        public bool Toggle;

        public ToggleSetting(string name, bool toggle)
        {
            Name = name;
            Toggle = toggle;
        }
    }

    [Serializable]
    class SliderSetting
    {
        public string Name;
        public float Value;
        public float Max;
        public float Step;

        public SliderSetting(string name, float value, float max, float step)
        {
            Name = name;
            Value = value;
            Max = max;
            Step = step;
        }
    }

    [Serializable]
    class FloatSetting
    {
        public string Name;
        public float Value;

        public FloatSetting(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }

    [Serializable]
    class Keybind
    {
        public string Name;
        public Key Key;
        public bool CTRL;
        public bool ALT;
        public bool SHIFT;

        public Keybind(string name, Key key, bool ctrl, bool alt, bool shift)
        {
            Name = name;
            Key = key;
            CTRL = ctrl;
            ALT = alt;
            SHIFT = shift;
        }
    }

    [Serializable]
    class ColorSetting
    {
        public string Name;
        public Color Color;

        public ColorSetting(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }

    [Serializable]
    class StringSetting
    {
        public string Name;
        public string Value;

        public StringSetting(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
