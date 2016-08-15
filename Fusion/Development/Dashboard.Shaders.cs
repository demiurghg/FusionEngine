using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using Fusion.Engine.Common;
using Fusion.Core.Mathematics;
using Fusion.Core.Extensions;
using Fusion.Engine.Graphics;


namespace Fusion.Development {

	public partial class Dashboard {


		void SetupShaderPage ()
		{
			var shaders = RequireShaderAttribute.GatherRequiredShaders().ToArray();

			listBoxShaders.Items.AddRange( shaders );

			listBoxShaders.SelectedIndexChanged += listBoxShaders_SelectedIndexChanged;

			listBoxShaders.SelectedIndex = 0;
		}



		void listBoxShaders_SelectedIndexChanged ( object sender, EventArgs e )
		{
			
		}									   


		private void buttonShaderCompile_Click ( object sender, EventArgs e )
		{
			var shaders = listBoxShaders.SelectedItems
					.Cast<string>()
					.Select( n => "/file:" + n )
					.ToArray();

			Game.Invoker.PushAndExecute(string.Format("contentBuild {0} /async", string.Join(" ", shaders) ));
		}

		private void buttonShaderShowReport_Click ( object sender, EventArgs e )
		{

		}

		private void buttonShaderEdit_Click ( object sender, EventArgs e )
		{

		}

	}
}
