using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using System.IO;
using System.Diagnostics;


namespace Fusion.Development {
	public static class LaunchBox {

		public static bool ShowDialog ( Game game, string config )
		{
			//var form = new Dashboard( game, config, true );
			var form = new LaunchBoxForm( game, config );

			var dr = form.ShowDialog();

			if (dr==DialogResult.OK) {
				return true;
			} else {
				return false;
			}
		} 


		public static void Show ( Game game, string config )
		{
			var openForms	=	Application.OpenForms.Cast<Form>();

			var dashboard	=	openForms.FirstOrDefault( f => f is Dashboard );

			if (dashboard==null) {
				var form = new Dashboard( game, config, false );
				form.Show();
			} else {
				dashboard.Focus();
			}
		} 
	}

}
