using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace Fusion.Development {
	internal partial class ExceptionDialog : Form {
		private ExceptionDialog ( Exception exception )
		{
			InitializeComponent();

			this.AcceptButton = buttonTerminate;

			ShowExceptionData( exception );

			if (Debugger.IsAttached) {
				this.buttonTerminate.Text = "Break";
			}

			if (exception.InnerException!=null) {
				showInnerException.Click += (s,e) => ShowExceptionData( exception.InnerException );
			} else {
				showInnerException.Enabled = false;
			}

			//	force visible cursor!
			Cursor.Show();
			Cursor.Clip	=	new Rectangle( int.MinValue, int.MinValue, int.MaxValue, int.MaxValue );
		}


		
		void ShowExceptionData ( Exception exception )
		{
			this.labelExceptionType.Text	=	exception.GetType().ToString();
			this.textBoxMessage.Text		=	exception.Message;
			this.textBoxStack.Text			=	exception.StackTrace.ToString();
		}



		public static void Show ( Exception exception )
		{
			var dlg = new ExceptionDialog(exception);
			dlg.ShowDialog();
		}

		private void buttonTerminate_Click ( object sender, EventArgs e )
		{
			if (Debugger.IsAttached) {
				Close();
			} else {
				Process.GetCurrentProcess().Kill();
			}
		}

		private void ExceptionDialog_FormClosed ( object sender, FormClosedEventArgs e )
		{
			if (Debugger.IsAttached) {
			} else {
				Process.GetCurrentProcess().Kill();
			}
		}
	}
}
