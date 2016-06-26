using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;

namespace Fusion.Engine.Input
{
	public class Touch : GameModule		
	{
		InputDevice device;

		public event TouchTapEventHandler	Tap;
		public event TouchTapEventHandler	DoubleTap;
		public event TouchTapEventHandler	SecondaryTap;
		public event TouchTapEventHandler	Manipulate;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Game"></param>
		internal Touch ( Game game ) : base(game)
		{
			this.device = Game.InputDevice;

			device.TouchGestureTap			+= DeviceOnTouchGestureTap;
			device.TouchGestureDoubleTap	+= DeviceOnTouchGestureDoubleTap;
			device.TouchGestureSecondaryTap += DeviceOnTouchGestureSecondaryTap;
			device.TouchGestureManipulate	+= DeviceOnTouchGestureManipulate;
		}

		private void DeviceOnTouchGestureManipulate(TouchEventArgs args)
		{
			if (Manipulate != null)
				Manipulate(args);
		}

		private void DeviceOnTouchGestureSecondaryTap(TouchEventArgs args)
		{
			if (SecondaryTap != null)
				SecondaryTap(args);
		}

		private void DeviceOnTouchGestureDoubleTap(TouchEventArgs args)
		{
			if (DoubleTap != null)
				DoubleTap(args);
		}

		private void DeviceOnTouchGestureTap(TouchEventArgs args)
		{
			if (Tap != null)
				Tap(args);
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				device.TouchGestureTap			-= DeviceOnTouchGestureTap;
				device.TouchGestureDoubleTap	-= DeviceOnTouchGestureDoubleTap;
				device.TouchGestureSecondaryTap -= DeviceOnTouchGestureSecondaryTap;
				device.TouchGestureManipulate	-= DeviceOnTouchGestureManipulate;
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		public bool IsTouchSupported
		{
			get { return true; }
			set { }
		}

	}
}
