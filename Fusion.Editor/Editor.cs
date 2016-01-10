using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Engine.Common;

namespace Fusion.Editor {
	public static class Editor {

		/// <summary>
		/// Opens editor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="extension"></param>
		/// <param name="typelist"></param>
		public static void Open ( Game game, string name, string extension, Type baseType )
		{
			var forms = Application.OpenForms
						.Cast<Form>()
						.Where( f0 => f0 is EditorForm )
						.Select( f1 => f1 as EditorForm )
						.ToArray();

			var form = forms.FirstOrDefault( f => f.EditorName == name );

			if (form==null || form.IsDisposed) {
				form = new EditorForm( name, extension, baseType );
				form.Show();
				form.BringToFront();
			} else {
				form.BringToFront();
			}
		}


		public static void CloseAll ()
		{
			var forms = Application.OpenForms
						.Cast<Form>()
						.Where( f0 => f0 is EditorForm )
						.Select( f1 => f1 as EditorForm )
						.ToArray();

			foreach ( var form in forms ) {
				form.Close();
			}
		}
	}
}
