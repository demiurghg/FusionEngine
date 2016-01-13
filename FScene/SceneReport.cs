using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Core.Content;
using Fusion.Engine.Graphics;

namespace FScene {
	static class SceneReport {
		/// <summary>
		/// 
		/// </summary>
		static public string CreateHtmlReport ( Scene scene )
		{
			var sb = new StringBuilder();

			sb.AppendLine("<pre>");


			sb.AppendFormat("<b>Nodes count</b>     : {0}\r\n", scene.Nodes.Count );
			sb.AppendFormat("<b>Mesh count</b>      : {0}\r\n", scene.Meshes.Count );
			sb.AppendFormat("<b>Materials count</b> : {0}\r\n", scene.Materials.Count );
			sb.AppendLine();
			sb.AppendFormat("<b>Track count</b>     : {0}\r\n", scene.TrackCount );
			sb.AppendFormat("<b>Frame range</b>     : {0} - {1}\r\n", scene.FirstFrame, scene.LastFrame );
			sb.AppendFormat("<b>Time range</b>      : {0} - {1}\r\n", scene.StartTime, scene.EndTime );




			sb.AppendLine();
			sb.AppendLine("<b>Materials:</b>");

			foreach ( var mtrl in scene.Materials ) {

				var index = scene.Materials.IndexOf(mtrl);
				
				sb.AppendFormat("{0,4}:  {1,-30} texture:<i>{2}</i>\r\n", index, "\"" + mtrl.Name + "\"", mtrl.Texture );
			}



			sb.AppendLine();
			sb.AppendLine("<b>Meshes:</b>");

			foreach ( var mesh in scene.Meshes ) {
				
				var index	=	scene.Meshes.IndexOf(mesh);
				var verts	=	mesh.VertexCount;
				var tris	=	mesh.TriangleCount;
				var subsets	=	mesh.Subsets.Count;
				var refs	=	string.Join(" ", scene.Nodes.Where( n => n.MeshIndex==index ).Select( n1 => n1.Name ) );
				var skinned	=	mesh.Vertices.Any( v => v.SkinWeights != Vector4.Zero );
				
				sb.AppendFormat("{0,4}:  v:{1,4}  t:{2,4}  s:{3,2}  ref:[<i>{4}</i>] {5}\r\n", index, verts, tris, subsets, refs, skinned ? "skinned":"" );
			}


			sb.AppendLine();
			sb.AppendLine("<b>Nodes:</b>");

			foreach ( var node in scene.Nodes ) {
				
				var index	= scene.Nodes.IndexOf(node);
				var parent	= node.ParentIndex;
				var name	= node.Name;

				int depth	=	scene.CalculateNodeDepth(node);
				var padding	=	new string(' ', depth*2);
				var hasMesh =	node.MeshIndex >= 0 ? "mesh #" + node.MeshIndex.ToString() : "";
				
				sb.AppendFormat("{0,4}:  {1,-30}{2,4}  track:{3}  {4}\r\n", index, padding + name, parent, node.TrackIndex, hasMesh );
			}

			sb.AppendLine("</pre>");

			return sb.ToString();
		}
	}
}
