using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using DrawPoint = System.Drawing.Point;

namespace Fusion.Input.Touch
{
    public class TouchForm : Form
    {
        #region ** fields

		public class TouchEventArgs : EventArgs {

			public TouchEventArgs ( int id, Point location )
			{
				PointerID	=	id;
				Location	=	location;
			}
			
			public int PointerID;
			public Point Location;
		}

		public event EventHandler<TouchEventArgs> PointerDown;
		public event EventHandler<TouchEventArgs> PointerUp;
		public event EventHandler<TouchEventArgs> PointerUpdate;
		public event EventHandler<TouchEventArgs> PointerLostCapture;

        #endregion

        #region ** initialization

        public TouchForm() : base()
        {
			Win32TouchFunctions.EnableMouseInPointer(false);
        }

        #endregion

        #region ** finalization

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
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

		        DrawPoint pt = PointToClient(pi.PtPixelLocation.ToPoint());

		        switch (m.Msg) {
			        case Win32TouchFunctions.WM_POINTERDOWN:
			        {
				        if ((pi.PointerFlags & Win32TouchFunctions.POINTER_FLAGS.PRIMARY) != 0) {
					        this.Capture = true;
				        }

						var pointerDown = PointerDown;
						if (pointerDown!=null) {
							pointerDown( new object(), new TouchEventArgs( pointerID, new Point( pt.X, pt.Y ) ) );
						}

			        }
				        break;

			        case Win32TouchFunctions.WM_POINTERUP:

				        if ((pi.PointerFlags & Win32TouchFunctions.POINTER_FLAGS.PRIMARY) != 0) {
					        this.Capture = false;
				        }

						var pointerUp = PointerUp;
						if (pointerUp!=null) {
							pointerUp( new object(), new TouchEventArgs( pointerID, new Point( pt.X, pt.Y ) ) );
						}

				        break;

			        case Win32TouchFunctions.WM_POINTERUPDATE:

						var pointerUpdate = PointerUpdate;
						if (pointerUpdate!=null) {
							pointerUpdate( new object(), new TouchEventArgs( pointerID, new Point( pt.X, pt.Y ) ) );
						}

				        break;

			        case Win32TouchFunctions.WM_POINTERCAPTURECHANGED:

				        this.Capture = false;

						var pointerLostCapture = PointerLostCapture;
						if (pointerLostCapture!=null) {
							pointerLostCapture( new object(), new TouchEventArgs( pointerID, new Point( pt.X, pt.Y ) ) );
						}

				        break;
		        }
		        m.Result = IntPtr.Zero;
	        }
	        catch (Exception e) {
		        Log.Warning(e.Message);
	        }
        }
    }
}
