using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fusion.Core.Development {
	class TextBoxLogListener : LogListener {

		TextBox textBox;

		public TextBoxLogListener ( TextBox textBox )
		{
			this.textBox	=	textBox;
		}



		public override void Log ( LogMessage message )
		{
			string prefix = "";
			
			textBox.Text += "[" + message.MessageType.ToString() + "] " + message.MessageText + "\r\n";
		}



		public override void Flush ()
		{
			base.Flush();
		}
	}
}
