using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using SharpDX.Multimedia;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Core.Configuration;


namespace Fusion.Engine.Audio {
	public partial class SoundSystem : GameComponent {

        internal XAudio2 Device { get; private set; }
        internal MasteringVoice MasterVoice { get; private set; }

		internal static int OperationSetCounter {
			get { return operationSetCounter; }
		}
		static int operationSetCounter = 1;


		/// <summary>
		/// 
		/// </summary>
		public SoundSystem ( Game game ) : base(game)
		{
		}



		/// <summary>
		/// 
		/// </summary>
        public override void Initialize()
        {
            try
            {
                if (Device == null) {
                    Device = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.DefaultProcessor);
                    Device.StartEngine();
                }

				var DeviceFormat = Device.GetDeviceDetails(0).OutputFormat;

                // Just use the default device.
                const int deviceId = 0;

                if (MasterVoice == null) {
                    // Let windows autodetect number of channels and sample rate.
                    MasterVoice = new MasteringVoice(Device, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceId);
                    MasterVoice.SetVolume(_masterVolume, 0);
                }

                // The autodetected value of MasterVoice.ChannelMask corresponds to the speaker layout.
                var deviceDetails = Device.GetDeviceDetails(deviceId);
                Speakers = deviceDetails.OutputFormat.ChannelMask;

				var dev3d = Device3D;

				Log.Debug("Audio devices :");
				for ( int devId = 0; devId < Device.DeviceCount; devId++ ) {
					var device = Device.GetDeviceDetails( devId );

					Log.Debug( "[{1}] {0}", device.DisplayName, devId );
					Log.Debug( "    role : {0}", device.Role );
					Log.Debug( "    id   : {0}", device.DeviceID );
				}
            }
            catch
            {
                // Release the device and null it as
                // we have no audio support.
                if (Device != null)
                {
                    Device.Dispose();
                    Device = null;
                }

                MasterVoice = null;
            }


			soundWorld	=	new SoundWorld(Game);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
			if (disposing) {

				soundWorld.Clear();

				if (MasterVoice != null) {
					MasterVoice.DestroyVoice();
					MasterVoice.Dispose();
					MasterVoice = null;
				}

				if (Device != null) {
					Device.StopEngine();
					Device.Dispose();
					Device = null;
				}

				_device3DDirty = true;
				_speakers = Speakers.Stereo;
			}
        }



		/// <summary>
		/// Gets default sound world.
		/// </summary>
		public SoundWorld SoundWorld {
			get { return soundWorld; }
		}


		SoundWorld soundWorld;




		/// <summary>
		/// Updates sound.
		/// </summary>
		internal void Update ( GameTime gameTime )
		{
			SoundWorld.Update( gameTime, OperationSetCounter );

			Device.CommitChanges( OperationSetCounter );
			Interlocked.Increment( ref operationSetCounter );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	3D sound stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

        private float		speedOfSound	= 343.5f;
		private float		_distanceScale	= 1.0f;
		private X3DAudio	_device3D		= null;
        private bool		_device3DDirty	= true;
        private Speakers	_speakers		= Speakers.Stereo;
		private float		_masterVolume	= 1.0f;
        private float		_dopplerScale	= 1.0f;


		/// <summary>
		/// ???
		/// </summary>
        internal Speakers Speakers {
            get {
                return _speakers;
            }

            set	 {
                if (_speakers != value) {
                    _speakers = value;
                    _device3DDirty = true;
                }
            }
        }




		/// <summary>
		/// Device 3D
		/// </summary>
        internal X3DAudio Device3D
        {
            get {
                if (_device3DDirty) {
                    _device3DDirty = false;
                    _device3D = new X3DAudio(_speakers, speedOfSound);
                }//				   */

                return _device3D;
            }
        }


	}

}
