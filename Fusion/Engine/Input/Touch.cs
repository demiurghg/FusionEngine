﻿using System;
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
		public event TouchManipulateHandler Manipulate;

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

		private void DeviceOnTouchGestureManipulate(Vector2 center, Vector2 delta, float scale)
		{
			if (Manipulate != null)
				Manipulate(center, delta, scale);
		}

		private void DeviceOnTouchGestureSecondaryTap(Vector2 point)
		{
			if (SecondaryTap != null)
				SecondaryTap(point);
		}

		private void DeviceOnTouchGestureDoubleTap(Vector2 point)
		{
			if (DoubleTap != null)
				DoubleTap(point);
		}

		private void DeviceOnTouchGestureTap(Vector2 point)
		{
			if (Tap != null)
				Tap(point);
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
