using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Audio;
using Fusion.Engine.Common;


namespace Fusion.Engine.Audio {

	public partial class SoundSystem : GameModule {

		/// <summary>
		/// Mastering voice value.
		/// </summary>
		[Config]
        public float MasterVolume
        { 
            get {
                return _masterVolume;
            }
            set {
                if (_masterVolume != value) {
                    _masterVolume = value;
                }
				if (MasterVoice!=null) {
					MasterVoice.SetVolume(_masterVolume, SoundSystem.OperationSetCounter);
				}
            }
        }

		/// <summary>
		/// Overall distance scale. Default = 1.
		/// </summary>
		[Config]
        public float DistanceScale {
            get {
                return _distanceScale;
            }
            set	{
                if (value <= 0f){
					throw new ArgumentOutOfRangeException ("value of DistanceScale");
                }
                _distanceScale = value;
            }
        }

		/// <summary>
		/// Overall doppler scale. Default = 1;
		/// </summary>
		[Config]
        public float DopplerScale {
            get {
                return _dopplerScale;
            }
            set	 {
                // As per documenation it does not look like the value can be less than 0
                //   although the documentation does not say it throws an error we will anyway
                //   just so it is like the DistanceScale
                if (value < 0f) {
                    throw new ArgumentOutOfRangeException ("value of DopplerScale");
                }
                _dopplerScale = value;
            }
        }



		/// <summary>
		/// Global speed of sound. Default = 343.5f;
		/// </summary>
		[Config]
        public float SpeedOfSound {
            get {
                return speedOfSound;
            }
            set {
                speedOfSound = value;
		        _device3DDirty = true;
            }
        }
	}
}
