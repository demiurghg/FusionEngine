using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using Point = System.Drawing.Point;

namespace Fusion.Input.Touch
{
    public class TouchForm : Form
    {
        #region ** fields

        BackgroundTouch background;

		public event Action<TouchEventArgs> TouchTap;
		public event Action<TouchEventArgs> TouchDoubleTap;
		public event Action<TouchEventArgs> TouchSecondaryTap;
	    public event Action<TouchEventArgs> TouchManipulation;

        #endregion

        #region ** initialization

        public TouchForm() : base()
        {
			Win32TouchFunctions.EnableMouseInPointer(false);

			background = new BackgroundTouch(this);
        }

        #endregion

        #region ** finalization

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                background.Dispose();
                background = null;
            }
            base.Dispose(disposing);
        }

        #endregion


        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void WndProc(ref Message m)
        {
	        try {
		        switch (m.Msg) {
			        case Win32TouchFunctions.WM_POINTERDOWN:
			        case Win32TouchFunctions.WM_POINTERUP:
			        case Win32TouchFunctions.WM_POINTERUPDATE:
			        case Win32TouchFunctions.WM_POINTERCAPTURECHANGED:
				        break;

			        default:
				        base.WndProc(ref m);
				        return;
		        }
		        int pointerID = Win32TouchFunctions.GET_POINTER_ID(m.WParam);
		        Win32TouchFunctions.POINTER_INFO pi = new Win32TouchFunctions.POINTER_INFO();
		        if (!Win32TouchFunctions.GetPointerInfo(pointerID, ref pi)) {
			        Win32TouchFunctions.CheckLastError();
		        }
		        switch (m.Msg) {
			        case Win32TouchFunctions.WM_POINTERDOWN:
			        {
				        if ((pi.PointerFlags & Win32TouchFunctions.POINTER_FLAGS.PRIMARY) != 0) {
					        this.Capture = true;
				        }
				        Point pt = PointToClient(pi.PtPixelLocation.ToPoint());

				        background.AddPointer(pointerID);
				        background.ProcessPointerFrames(pointerID, pi.FrameID);

			        }
				        break;

			        case Win32TouchFunctions.WM_POINTERUP:

				        if ((pi.PointerFlags & Win32TouchFunctions.POINTER_FLAGS.PRIMARY) != 0) {
					        this.Capture = false;
				        }

				        if (background.ActivePointers.Contains(pointerID)) {
					        background.ProcessPointerFrames(pointerID, pi.FrameID);
					        background.RemovePointer(pointerID);
				        }
				        break;

			        case Win32TouchFunctions.WM_POINTERUPDATE:

				        if (background.ActivePointers.Contains(pointerID)) {
					        background.ProcessPointerFrames(pointerID, pi.FrameID);
				        }
				        break;

			        case Win32TouchFunctions.WM_POINTERCAPTURECHANGED:

				        this.Capture = false;

				        if (background.ActivePointers.Contains(pointerID)) {
					        background.StopProcessing();
				        }
				        break;
		        }
		        m.Result = IntPtr.Zero;
	        }
	        catch (Exception e) {
		        Log.Warning(e.Message);
	        }
        }


		public void NotifyTap(TouchEventArgs args)
	    {
		    if (TouchTap != null) {
				TouchTap(args);
		    }
	    }

		public void NotifyDoubleTap(TouchEventArgs args)
	    {
		    if (TouchDoubleTap != null) {
				TouchDoubleTap(args);
		    }
	    }

	    internal void NotifyTouchManipulation(TouchEventArgs args)
	    {
		    if (TouchManipulation != null) {
				TouchManipulation(args);
		    }
	    }

		internal void NotifyTouchSecondaryTap(TouchEventArgs args)
		{
			if (TouchSecondaryTap != null) {
				TouchSecondaryTap(args);
			}
		}

    }
}
