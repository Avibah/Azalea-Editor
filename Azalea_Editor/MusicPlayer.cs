using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.Misc;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Azalea_Editor
{
	class MusicPlayer
	{
		private int streamFileID;
		private int streamID;
		private string lastFile;
		public float originval;

		//private float[] samples;

		private SYNCPROC Sync;
		public ModelRaw Waveform;

		public MusicPlayer()
		{
			Init();
			Sync = new SYNCPROC(OnEnded);
		}

		/// <summary>
		///		This function makes sure your default device is being used, if not, reload Bass and the song back and continue as if nothing happened.
		/// </summary>
		private void CheckDevice()
		{
			if (!CheckDevice(streamID))
			{
				var pos = Bass.BASS_ChannelGetPosition(streamID, BASSMode.BASS_POS_BYTES);
				var secs = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(streamID, pos));

				var state = Bass.BASS_ChannelIsActive(streamID);
				var volume = 0.2f;

				Bass.BASS_ChannelGetAttribute(streamID, BASSAttribute.BASS_ATTRIB_VOL, ref volume);

				Reload();

				Load(lastFile);

				Volume = volume;
				CurrentTime = secs;

				switch (state)
				{
					case BASSActive.BASS_ACTIVE_PAUSED:
					case BASSActive.BASS_ACTIVE_STOPPED:
						Bass.BASS_ChannelPause(streamID);
						Bass.BASS_ChannelSetPosition(streamID, pos, BASSMode.BASS_POS_BYTES);
						break;
					case BASSActive.BASS_ACTIVE_STALLED:
					case BASSActive.BASS_ACTIVE_PLAYING:
						Bass.BASS_ChannelPlay(streamID, false);
						break;
				}
			}
		}

		public void Load(string file)
		{
			if (file == null || !File.Exists(file))
				return;

			Bass.BASS_StreamFree(streamID);
			Bass.BASS_StreamFree(streamFileID);

			if (lastFile != file)
			{
				lastFile = file;

				Waveform?.Dispose();

				var wf = new WaveForm(file);
				wf.FrameResolution = 0.005;

				if (wf.RenderStart(false, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_FX_FREESOURCE))
				{
					var samples = new float[wf.Wave.data.Length];

					var maxPeak = 0f;

					for (int i = 0; i < wf.Wave.data.Length; i++)
					{
						var sample = wf.Wave.data[i];

						var peakLeft = sample.left / (float)short.MaxValue;
						var peakRight = sample.right / (float)short.MaxValue;

						var peak = Math.Max(Math.Abs(peakLeft), Math.Abs(peakRight));

						maxPeak = Math.Max(peak, maxPeak);

						samples[i] = peak;
					}

					var vertexes = new Queue<float>();

					// averaging values
					for (int i = 0; i < samples.Length; i++)
					{
						if (maxPeak > 0.001)
						{
							samples[i] /= maxPeak;
						}
						samples[i] *= 0.9f;

						vertexes.Enqueue(i / (float)samples.Length * 100000);
						vertexes.Enqueue(samples[i]);
					}

					Waveform = ModelManager.LoadModel2ToVao(vertexes.ToArray());
					wf.Reset();
				}
			}

			var stream = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_FX_FREESOURCE);
			var tempo = Tempo;

			streamFileID = stream;
			streamID = BassFx.BASS_FX_TempoCreate(streamFileID, BASSFlag.BASS_STREAM_PRESCAN);

			Tempo = tempo;

			Bass.BASS_ChannelGetAttribute(streamID, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref originval);
			Bass.BASS_ChannelSetSync(streamID, BASSSync.BASS_SYNC_END, 0, Sync, IntPtr.Zero);

			Reset();
		}

		public static byte[] BitmapToByteArray(Bitmap bitmap)
		{

			BitmapData bmpdata = null;

			try
			{
				bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
				int numbytes = bmpdata.Stride * bitmap.Height;
				byte[] bytedata = new byte[numbytes];
				IntPtr ptr = bmpdata.Scan0;

				Marshal.Copy(ptr, bytedata, 0, numbytes);

				return bytedata;
			}
			finally
			{
				if (bmpdata != null)
					bitmap.UnlockBits(bmpdata);
			}

		}

		/*
		public float GetPeak(double time)
		{
			if (samples == null)
				return 0;

			var length = TotalTime.TotalMilliseconds;
			var p = time / length;
			var index = Math.Max(0, Math.Min(samples.Length - 1, p * samples.Length));

			var alpha = index % 1.0;

			var first = 0f;
			var next = 0f;

			if (samples.Length > 1)
			{
				var indexFirst = (int)index;
				var indexSecond = Math.Min(samples.Length - 1, indexFirst + 1);

				first = samples[indexFirst];
				next = samples[indexSecond];
			}

			return first + (next - first) * (float)alpha;
		}*/

		private void OnEnded(int handle, int channel, int data, IntPtr user)
		{
			Pause();
			CurrentTime = TotalTime;
			MainWindow.inst.currentTime = CurrentTime;
		}

		public void Play()
		{
			CurrentTime = MainWindow.inst.currentTime;
			if (CurrentTime != MainWindow.inst.currentTime)
				Reset();
			CheckDevice();

			Bass.BASS_ChannelPlay(streamID, false);
		}

		public void Pause()
		{
			if (IsPlaying)
            {
				var pos = Bass.BASS_ChannelGetPosition(streamID, BASSMode.BASS_POS_BYTES);

				Bass.BASS_ChannelPause(streamID);
				Bass.BASS_ChannelSetPosition(streamID, pos, BASSMode.BASS_POS_BYTES);
				MainWindow.inst.currentTime = CurrentTime;
			}
			CheckDevice();
		}

		public void Stop()
		{
			CheckDevice();

			Bass.BASS_ChannelStop(streamID);
			Bass.BASS_ChannelSetPosition(streamID, 0, BASSMode.BASS_POS_BYTES);
		}

		public float Tempo
		{
			set
			{
				CheckDevice();

				Bass.BASS_ChannelSetAttribute(streamID, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, originval * value);
			}
			get
			{
				CheckDevice();

				float val = 0;

				Bass.BASS_ChannelGetAttribute(streamID, BASSAttribute.BASS_ATTRIB_TEMPO_FREQ, ref val);

				return -(val + 95) / 100;
			}
		}

		public float Volume
		{
			set
			{
				CheckDevice();
				Bass.BASS_ChannelSetAttribute(streamID, BASSAttribute.BASS_ATTRIB_VOL, value);
			}
			get
			{
				CheckDevice();

				float val = 1;

				Bass.BASS_ChannelGetAttribute(streamID, BASSAttribute.BASS_ATTRIB_VOL, ref val);

				return val;
			}
		}

		public void Reset()
		{
			Stop();

			CurrentTime = TimeSpan.Zero;
		}

		public bool IsPlaying
		{
			get
			{
				CheckDevice();

				return Bass.BASS_ChannelIsActive(streamID) == BASSActive.BASS_ACTIVE_PLAYING;
			}
		}

		public bool IsPaused
		{
			get
			{
				CheckDevice();

				return Bass.BASS_ChannelIsActive(streamID) == BASSActive.BASS_ACTIVE_PAUSED;
			}
		}

		public TimeSpan TotalTime
		{
			get
			{
				CheckDevice();

				long len = Bass.BASS_ChannelGetLength(streamID, BASSMode.BASS_POS_BYTES);
				var time = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(streamID, len));
				time = TimeSpan.FromMilliseconds(time.TotalMilliseconds - 1);

				return time;
			}
		}

		public TimeSpan CurrentTime
		{
			get
			{
				CheckDevice();

				var pos = Bass.BASS_ChannelGetPosition(streamID, BASSMode.BASS_POS_BYTES);

				return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(streamID, pos) + 0.03 * MainWindow.inst.tempo);
			}
			set
			{
				CheckDevice();

				var pos = Bass.BASS_ChannelSeconds2Bytes(streamID, value.TotalSeconds - 0.03 * MainWindow.inst.tempo);

				Bass.BASS_ChannelSetPosition(streamID, pos, BASSMode.BASS_POS_BYTES);
			}
		}

		public decimal Progress
		{
			get
			{
				CheckDevice();

				var pos = Bass.BASS_ChannelGetPosition(streamID, BASSMode.BASS_POS_BYTES);
				var length = Bass.BASS_ChannelGetLength(streamID, BASSMode.BASS_POS_BYTES);

				return pos / (decimal)length;
			}
		}

		private static void Init()
		{
			Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 250);
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 5);
		}

		public static bool CheckDevice(int streamID)
		{
			var device = Bass.BASS_ChannelGetDevice(streamID);
			var info = Bass.BASS_GetDeviceInfo(device);

			if (info != null && (!info.IsDefault || !info.IsEnabled))
			{
				return false;
			}

			return true;
		}

		public static void Reload()
		{
			Dispose();
			Init();
		}

		public static void Dispose()
		{
			Bass.BASS_Free();
		}
	}

	class ModelRaw
	{
		public int VaoID { get; }
		public int[] BufferIDs { get; }

		public int VertexCount { get; protected set; }

		/*
        public ModelRaw(int vaoID, int valuesPerVertice, List<RawQuad> quads, params int[] bufferIDs)
        {
            VaoID = vaoID;
            BufferIDs = bufferIDs;

            foreach (RawQuad quad in quads)
                VertexCount += quad.vertices.Length / valuesPerVertice;
        }*/

		//buffer IDs must be in this order and step: 0;1;2;3; ...
		public ModelRaw(int vaoID, int vertexCount, params int[] bufferIDs)
		{
			VaoID = vaoID;
			BufferIDs = bufferIDs;

			VertexCount = vertexCount;
		}

		public bool HasLocalData() => true;

		public void Render(PrimitiveType pt) => GL.DrawArrays(pt, 0, VertexCount);

		public void Dispose()
		{
			ModelManager.DisposeOf(this);
		}
	}

	class ModelManager
	{
		private static readonly List<int> VaOs = new List<int>();
		private static readonly List<int> VbOs = new List<int>();

		public static ModelRaw LoadModel2ToVao(float[] vertexes)
		{
			int vaoId = CreateVao();

			int buff0 = StoreDataInAttribList(0, 2, vertexes);

			UnbindVao();

			return new ModelRaw(vaoId, vertexes.Length / 2, buff0);
		}

		private static void OverrideDataInAttributeList(int id, int attrib, int coordSize, float[] data)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, id);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
			GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		private static int CreateVao()
		{
			int vaoId = GL.GenVertexArray();

			VaOs.Add(vaoId);

			GL.BindVertexArray(vaoId);

			return vaoId;
		}

		private static void UnbindVao()
		{
			GL.BindVertexArray(0);
		}

		private static int StoreDataInAttribList(int attrib, int coordSize, float[] data)
		{
			int vboId = GL.GenBuffer();

			VbOs.Add(vboId);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vboId);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.DynamicDraw);
			GL.VertexAttribPointer(attrib, coordSize, VertexAttribPointerType.Float, false, 0, 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			return vboId;
		}

		public static void Cleanup()
		{
			foreach (int vao in VaOs)
				GL.DeleteVertexArray(vao);

			foreach (int vbo in VbOs)
				GL.DeleteBuffer(vbo);
		}

		public static void DisposeOf(ModelRaw model)
		{
			VaOs.Remove(model.VaoID);

			GL.DeleteVertexArray(model.VaoID);

			foreach (int vbo in model.BufferIDs)
			{
				VbOs.Remove(vbo);

				GL.DeleteBuffer(vbo);
			}
		}
	}
}