using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.ComponentModel.Design;
using System.Reflection;
using System.ComponentModel;


namespace Fusion.Editor {

	public class EditorItem {

		/// <summary>
		/// Full path on disk
		/// </summary>
		public string FullPath { get; private set; }

		/// <summary>
		/// Base directory
		/// </summary>
		public string BaseDirectory { get; private set; }

		/// <summary>
		/// Key path
		/// </summary>
		public string KeyPath { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		public object Object { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullPath"></param>
		/// <param name="baseDirectory"></param>
		/// <param name="tyepList"></param>
		public EditorItem ( string fullPath, string baseDirectory, Type baseType )
		{
			this.FullPath	=	fullPath;
			BaseDirectory	=	baseDirectory;
			this.KeyPath	=	fullPath.Replace( baseDirectory, "" );
			
			string xmlText	=	File.ReadAllText( fullPath );
			Object			=	Misc.LoadObjectFromXml( baseType, xmlText, Misc.GetAllSubclassesOf(baseType) ); 
		}

	}
}
