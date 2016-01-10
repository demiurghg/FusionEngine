using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Build;
using System.IO;
using System.Runtime.InteropServices;

namespace Fusion.Editor {
	public partial class EditorForm : Form {

		public readonly string	EditorName;
		public readonly Type	BaseType;
		public readonly string	FileExt;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="fileExt"></param>
		/// <param name="types"></param>
		public EditorForm ( string name, string fileExt, Type baseType )
		{
			EditorName	=	name;
			FileExt		=	fileExt;
			BaseType	=	baseType;

			InitializeComponent();

			this.Text	=	EditorName;

			RefreshTree();
		}
 


		/// <summary>
		/// 
		/// </summary>
		void RefreshTree ()
		{
			var baseDir	=	Builder.Options.FullInputDirectory;
			var files	=	Directory.GetFiles( baseDir, FileExt, SearchOption.AllDirectories ).ToArray();

			foreach ( var file in files ) {
				Log.Message(".. {0}", file);
			}

			var items = files.Select( f => new EditorItem( f, baseDir, BaseType ) ).ToArray(); 


			AddBranch( "Content", new[]{'\\','/'}, items, true );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		public void AddNode ( string name, object obj )
		{
			TreeNode node = null;

			if (mainTreeView.Nodes.ContainsKey( name )) {
				node = mainTreeView.Nodes[ name ];
			} else {
				mainTreeView.Nodes.Add( name, name );
				node = mainTreeView.Nodes[ name ];
			}

			node.Tag = obj;
		}



		public class PathComparer : IComparer<string> {

			[DllImport("shlwapi.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
			static extern int StrCmpLogicalW(String x, String y);

			public int Compare(string x, string y) {

				var pathX = x.Split( new[]{'/', '\\'}, StringSplitOptions.RemoveEmptyEntries );
				var pathY = y.Split( new[]{'/', '\\'}, StringSplitOptions.RemoveEmptyEntries );

				var min	  = Math.Min( pathX.Length, pathY.Length );

				
				for ( int i = 0; i<min; i++ ) {

					if ( i == min-1 ) {
						
						var lenD = pathY.Length - pathX.Length;
						var extD = StrCmpLogicalW( Path.GetExtension(pathX[i]), Path.GetExtension(pathY[i]) );
						
						if (lenD!=0) {
							return lenD;
						} else if (extD!=0) {
							return extD;
						} else {
							return StrCmpLogicalW( pathX[i], pathY[i] );
						}
					}

					int cmp = StrCmpLogicalW( pathX[i], pathY[i] );

					if (cmp!=0) {
						return cmp;
					}
				}

				return 0;
			}

		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rootName"></param>
		/// <param name="list"></param>
		public void AddBranch ( string rootName, char[] separator, IEnumerable<EditorItem> itemList, bool expand )
		{
			TreeNode rootNode = null;

			if (mainTreeView.Nodes.ContainsKey(rootName)) {
				rootNode = mainTreeView.Nodes[rootName];
			} else {
				mainTreeView.Nodes.Add(rootName,rootName);
				rootNode = mainTreeView.Nodes[rootName];
			}

			//
			//	Sort file names :
			//
			var itemList2 = itemList.OrderBy( item => item.KeyPath, new PathComparer() ).ToList();

			//
			//	Add all :
			//
			foreach ( var item in itemList2 ) {
				
				var path	= 	item.KeyPath;
				var tokens	=	item.KeyPath.Split( separator, StringSplitOptions.RemoveEmptyEntries );
				var node	=	rootNode;

				if (expand) {
					node.ExpandAll();
				}

				foreach ( var tok in tokens ) {
					
					if (node.Nodes.ContainsKey(tok)) {
						node = node.Nodes[tok];
					} else {
						node.Nodes.Add( tok, Path.GetFileNameWithoutExtension(tok) );
						node = node.Nodes[tok];
					}

					//node.ForeColor	=	System.Drawing.Color.Black;
				}

				node.Tag		=	item;
			}
		}



		private void mainTreeView_NodeMouseClick ( object sender, TreeNodeMouseClickEventArgs e )
		{
			var item	=	e.Node.Tag as EditorItem;

			if (item == null) {
				mainPropertyGrid.SelectedObject = null;
			} else {
				mainPropertyGrid.SelectedObject = item.Object;
			}
		}


	}
}
