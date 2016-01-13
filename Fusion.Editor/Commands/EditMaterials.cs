using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Shell;
using Fusion.Core;
using Fusion.Engine.Graphics;

namespace Fusion.Editor.Commands {

	[Command("editMaterials", CommandAffinity.Default)]
	public class EditMaterials : NoRollbackCommand {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="invoker"></param>
		public EditMaterials ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Execute ()
		{
			Editor.Open(Invoker.Game, "Material Editor", "*.material", typeof(BaseIllum) );
		}
		

	}
}
