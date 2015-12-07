using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Engine.Graphics;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Engine.Common;
using SharpDX;
using SharpDX.Win32;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;
using SharpDX.MediaFoundation;

namespace Fusion.Engine.Media
{
	public sealed partial class VideoPlayer : DisposableBase {

		private MediaState _state;
		private Video _currentVideo;
		private float _volume = 1.0f;
		private bool _isLooped = false;
		private bool _isMuted = false;


		/// <summary>
		/// Gets a value that indicates whether the player is playing video in a loop.
		/// </summary>
		public bool IsLooped
		{
			get { return _isLooped; }
			set {
				if (_isLooped == value) {
					return;
				}

				_isLooped = value;
				PlatformSetIsLooped();
			}
		}


		/// <summary>
		/// Gets or sets the muted setting for the video player.
		/// </summary>
		public bool IsMuted
		{
			get { return _isMuted; }
			set	{
				if (_isMuted == value) {
					return;
				}

				_isMuted = value;
				PlatformSetIsMuted();
			}
		}


		/// <summary>
		/// Gets the play position within the currently playing video.
		/// </summary>
		public TimeSpan PlayPosition
		{
			get	{
				if (_currentVideo == null || State == MediaState.Stopped)
					return TimeSpan.Zero;

				return PlatformGetPlayPosition();
			}
		}


		/// <summary>
		/// Gets the media playback state, MediaState.
		/// </summary>
		public MediaState State
		{
			get {
				// Give the platform code a chance to update 
				// the playback state before we return the result.
				PlatformGetState(ref _state);
				return _state;
			}
		}


		/// <summary>
		/// Gets the Video that is currently playing.
		/// </summary>
		public Video Video { get { return _currentVideo; } }


		/// <summary>
		/// Video player volume, from 0.0f (silence) to 1.0f (full volume relative to the current device volume).
		/// </summary>
		public float Volume
		{
			get { return _volume; }

			set{
				if (value < 0.0f || value > 1.0f)
					throw new ArgumentOutOfRangeException();

				_volume = value;

				if (_currentVideo != null) {
					PlatformSetVolume();
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public VideoPlayer()
		{
			_state = MediaState.Stopped;

			PlatformInitialize();
		}


		/// <summary>
		/// Retrieves a Texture2D containing the current frame of video being played.
		/// </summary>
		/// <returns>The current frame of video.</returns>
		/// <exception cref="InvalidOperationException">Thrown if no video is set on the player</exception>
		/// <exception cref="InvalidOperationException">Thrown if the platform was unable to get a texture in a reasonable amount of time. Often the platform specific media code is running
		/// in a different thread or process. Note: This may be a change from XNA behaviour</exception>
		public DynamicTexture GetTexture()
		{
			if (_currentVideo == null) {
				throw new InvalidOperationException("Operation is not valid due to the current state of the object");
			}

			//XNA never returns a null texture
			const int retries = 5;
			const int sleepTimeFactor = 50;
			DynamicTexture texture = null;

			for (int i = 0; i < retries; i++)
			{
				texture = PlatformGetTexture();
				if (texture != null)
				{
					break;
				}
				var sleepTime = i * sleepTimeFactor;

				Thread.Sleep(sleepTime); //Sleep for longer and longer times

			}
			if (texture == null)
			{
				Debug.WriteLine("Platform returned a null texture");
				//throw new InvalidOperationException("Platform returned a null texture");
			}

			return texture;
		}



		/// <summary>
		/// Pauses the currently playing video.
		/// </summary>
		public void Pause()
		{
			if (_currentVideo == null)
				return;

			PlatformPause();

			_state = MediaState.Paused;
		}



		/// <summary>
		/// Plays a Video.
		/// </summary>
		/// <param name="video">Video to play.</param>
		public void Play(Video video)
		{
			if (video == null)
				throw new ArgumentNullException("video is null.");

			if (_currentVideo == video) {
				var state = State;

				// No work to do if we're already
				// playing this video.
				if (state == MediaState.Playing)
					return;

				// If we try to Play the same video
				// from a paused state, just resume it instead.
				if (state == MediaState.Paused) {
					PlatformResume();
					return;
				}
			}
			else {
				if (_currentVideo != null && (State == MediaState.Playing || State == MediaState.Paused)) {
					SetNewVideo = true;
					Stop();
				}
			}

			_currentVideo = video;

			PlatformPlay();

			_state = MediaState.Playing;

			// XNA doesn't return until the video is playing
			const int retries = 5;
			const int sleepTimeFactor = 100;

			for (int i = 0; i < retries; i++) {
				if (State == MediaState.Playing) {
					break;
				}
				var sleepTime = i * sleepTimeFactor;
				Log.Verbose("State != MediaState.Playing ({0}) sleeping for {1} ms", i + 1, sleepTime);
#if WINRT
                Task.Delay(sleepTime).Wait();
#else
				Thread.Sleep(sleepTime); //Sleep for longer and longer times
#endif
			}
			if (State != MediaState.Playing) {
				//We timed out - attempt to stop to fix any bad state
				Stop();
				throw new InvalidOperationException("cannot start video");
			}
		}



		/// <summary>
		/// Resumes a paused video.
		/// </summary>
		public void Resume()
		{
			if (_currentVideo == null) {
				return;
			}

			var state = State;

			// No work to do if we're already playing
			if (state == MediaState.Playing) {
				return;
			}

			if (state == MediaState.Stopped) {
				PlatformPlay();
				return;
			}

			PlatformResume();

			_state = MediaState.Playing;
		}



		/// <summary>
		/// Stops playing a video.
		/// </summary>
		public void Stop()
		{
			if (_currentVideo == null)
				return;

			PlatformStop();

			_state = MediaState.Stopped;
		}



		/// <summary>
		/// 
		/// </summary>
		override protected void Dispose(bool disposing)
		{
			if (disposing) {
				PlatformDispose(disposing);
			}
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Media session stuff :
		 * 
		-----------------------------------------------------------------------------------------*/
		private static MediaSession _session;
		private static AudioStreamVolume _volumeController;
		private static PresentationClock _clock;

		// HACK: Need SharpDX to fix this.
		private static Guid AudioStreamVolumeGuid;

		private static Callback _callback;

		internal bool SetNewVideo = false;


		/// <summary>
		/// 
		/// </summary>
		private class Callback : IAsyncCallback
		{
			private VideoPlayer _player;

			public Callback(VideoPlayer player)
			{
				_player = player;
			}

			public void Dispose()
			{
			}

			public IDisposable Shadow { get; set; }
			public void Invoke(AsyncResult asyncResultRef)
			{
				var ev = _session.EndGetEvent(asyncResultRef);

				// Trigger an "on Video Ended" event here if needed
				if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ended)
				{
					Console.WriteLine("Video ended");
					if (!_player.SetNewVideo) _player.PlatformPlay();
					else {
						_player.SetNewVideo = false;
					}
				}

				if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
					_player.OnTopologyReady();

				_session.BeginGetEvent(this, null);

				SafeDispose( ref ev );
			}

			public AsyncCallbackFlags Flags { get; private set; }
			public WorkQueueId WorkQueueId { get; private set; }
		}




		/// <summary>
		/// 
		/// </summary>
		private void PlatformInitialize()
		{
			// The GUID is specified in a GuidAttribute attached to the class
			AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

			MediaAttributes attr = new MediaAttributes(0);
			//MediaManagerState.CheckStartup();
			
			MediaManager.Startup();

			MediaFactory.CreateMediaSession(attr, out _session);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		private void PlatformDispose(bool disposing)
		{
			PlatformStop();

			SafeDispose( ref _session );
			SafeDispose( ref _volumeController );
			SafeDispose( ref _clock );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private DynamicTexture PlatformGetTexture()
		{
			var sampleGrabber = _currentVideo.SampleGrabber;

			var texData = sampleGrabber.TextureData;

			if (texData == null)
				return null;

			// TODO: This could likely be optimized if we held on to the SharpDX Surface/Texture data,
			// and set it on an XNA one rather than constructing a new one every time this is called.
			//var retTex = new Texture2D(Game.Instance.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, ColorFormat.Bgra8, false);

			_currentVideo.VideoFrame.SetData(texData);

			return _currentVideo.VideoFrame;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void PlatformGetState(ref MediaState result)
		{
			if (_clock != null)
			{
				ClockState state;
				_clock.GetState(0, out state);

				switch (state)
				{
					case ClockState.Running:
						result = MediaState.Playing;
						return;

					case ClockState.Paused:
						result = MediaState.Paused;
						return;
				}
			}

			result = MediaState.Stopped;
		}



		/// <summary>
		/// 
		/// </summary>
		private void PlatformPause()
		{
			_session.Pause();
		}



		/// <summary>
		/// 
		/// </summary>
		private void PlatformPlay()
		{
			// Cleanup the last song first.
			if (State != MediaState.Stopped) {

				if (_session!=null) {	
					_session.Stop();
					_session.ClearTopologies();
					_session.Close();
				}
			
				SafeDispose(ref _clock);
				SafeDispose(ref _volumeController);
			}

			//create the callback if it hasn't been created yet
			if (_callback == null) {
				_callback = new Callback(this);
				_session.BeginGetEvent(_callback, null);
			}

			// Set the new song.
			_session.SetTopology(SessionSetTopologyFlags.Immediate, _currentVideo.Topology);

			// Get the clock.
			_clock = _session.Clock.QueryInterface<PresentationClock>();

			// Start playing.
			var varStart = new Variant();
			_session.Start(null, varStart);
		}



		/// <summary>
		/// 
		/// </summary>
		private void PlatformResume()
		{
			_session.Start(null, null);
		}



		/// <summary>
		/// 
		/// </summary>
		private void PlatformStop()
		{
			_session.ClearTopologies();
			_session.Stop();
			_session.Close();

			SafeDispose( ref _volumeController );
			SafeDispose( ref _clock );
		}



		/// <summary>
		/// 
		/// </summary>
		private void SetChannelVolumes()
		{
			if (_volumeController != null && !_volumeController.IsDisposed)
			{
				float volume = _volume;
				if (IsMuted)
					volume = 0.0f;

				for (int i = 0; i < _volumeController.ChannelCount; i++)
				{
					_volumeController.SetChannelVolume(i, volume);
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		private void PlatformSetVolume()
		{
			if (_volumeController == null)
				return;

			SetChannelVolumes();
		}



		/// <summary>
		/// 
		/// </summary>
		private void PlatformSetIsLooped()
		{
			
			throw new NotImplementedException();
		}

		
		
		/// <summary>
		/// 
		/// </summary>
		private void PlatformSetIsMuted()
		{
			if (_volumeController == null)
				return;

			SetChannelVolumes();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private TimeSpan PlatformGetPlayPosition()
		{
			return TimeSpan.FromTicks(_clock.Time);
		}

		
		
		/// <summary>
		/// 
		/// </summary>
		private void OnTopologyReady()
		{
			if (_session.IsDisposed) {
				return;
			}

			try {
				// Get the volume interface.
				IntPtr volumeObjectPtr;
				MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
				_volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

				SetChannelVolumes();
			}
			catch (Exception /*e*/) {
				_volumeController = null;
			}
		}
	}
}
