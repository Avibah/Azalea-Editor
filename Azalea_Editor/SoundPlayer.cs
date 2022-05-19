using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using System.IO;

namespace Azalea_Editor
{
    class SoundPlayer
    {
		private Dictionary<string, string> files = new Dictionary<string, string>();
		public float Volume;

		public SoundPlayer()
		{
			var sounds = Directory.GetFiles("misc/sounds");

			foreach (var file in sounds)
				files.Add(Path.GetFileNameWithoutExtension(file), file);
		}

		public void Play(string filename)
		{
			if (files.TryGetValue(filename, out var value))
            {
				var s = Bass.BASS_StreamCreateFile(value, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_FX_FREESOURCE);//sound, 0, 0, BASSFlag.BASS_STREAM_AUTOFREE);

				s = BassFx.BASS_FX_TempoCreate(s, BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_FX_FREESOURCE | BASSFlag.BASS_MUSIC_AUTOFREE);

				Bass.BASS_ChannelSetAttribute(s, BASSAttribute.BASS_ATTRIB_VOL, Volume);

				Bass.BASS_ChannelPlay(s, false);
			}
		}
	}
}
