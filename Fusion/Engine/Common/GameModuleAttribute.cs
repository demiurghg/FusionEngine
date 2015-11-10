using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Drivers.Graphics;
using Fusion.Core;


namespace Fusion.Engine.Common {

	[AttributeUsage(AttributeTargets.Property)]
	public class GameModuleAttribute : Attribute {

		/// <summary>
		/// Gets nice name of declared service.
		/// </summary>
		public string NiceName { get; private set; }


		/// <summary>
		/// Gets short name of declared service.
		/// </summary>
		public string ShortName { get; private set; }


		/// <summary>
		/// Sets and gets init order
		/// </summary>
		public InitOrder InitOrder { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="niceName"></param>
		/// <param name="shortName"></param>
		/// <param name="initOrder">Initialization order. Negative value means that module will be initilized befor parent module.
		/// Positive value means that module will be initialized after parent module.</param>
		public GameModuleAttribute( string niceName, string shortName, InitOrder initOrder )
		{
			NiceName	=	niceName;
			ShortName	=	shortName;
			InitOrder	=	initOrder;
		}
	}
}
