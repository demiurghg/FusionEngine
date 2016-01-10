using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.IO;
using Fusion.Core.Content;

namespace Fusion.Engine.Graphics.Design {
	public class TextureLocationEditor : UITypeEditor {

		static string lastDir = null;
		static string baseDir = null;

		/// <summary>
		/// Freaky hack.
		/// </summary>
		/// <returns></returns>
		static string GetBuilderContentFolder ()
		{
			try {
				var builderType = AppDomain.CurrentDomain.GetAssemblies()
					.Where( a0 => a0.FullName.Contains("Fusion.Build") )
					.Select( a1 => a1.GetType("Fusion.Build.Builder") )
					.FirstOrDefault();

				return (string)builderType
								.GetProperty("FullInputDirectory").GetValue(null);
			} catch ( Exception e ) {
				Log.Warning( e.ToString() );
				return null;
			}
		}



		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
		{
			return UITypeEditorEditStyle.Modal;
		}


		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) 
		{
			if (lastDir==null) {
				lastDir = GetBuilderContentFolder();
				baseDir = lastDir;
			}

			using (OpenFileDialog ofd = new OpenFileDialog()) {
				ofd.Filter				=	"Image Files (*.tga;*.jpg;*.dds;*.png)|*.tga;*.jpg;*.dds;*.png";
				ofd.InitialDirectory	=	lastDir;

				if (ofd.ShowDialog() == DialogResult.OK) {
					lastDir = Path.GetDirectoryName(ofd.FileName);
					return ContentUtils.MakeRelativePath( baseDir + "/", ofd.FileName ); 
					//return GameContent.RemovePathBase( ofd.FileName, "Content" );
				}
			}
			return value;
		}
	}
}
