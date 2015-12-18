using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SharpDX;
using Fusion.Engine.Graphics;
using System.Reflection;
using System.ComponentModel.Design;


namespace Fusion.Engine.Graphics {


	/// <summary>
	/// Material reference. 
	/// Keeps material name, base texture and reference to material.
	/// </summary>
	public sealed class MaterialRef : IEquatable<MaterialRef> {

		/// <summary>
		/// Material name.
		/// </summary>
		public	string	Name { get; set; }

		/// <summary>
		/// Base texture path.
		/// </summary>
		public	string	Texture { get; set; }



		/// <summary>
		/// Creates materail 
		/// </summary>
		public MaterialRef ()
		{
			Name			=	"";
			Texture		=	null;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public void Deserialize( BinaryReader reader )
		{
			Name =  reader.ReadString();

			Texture = null;
			if ( reader.ReadBoolean() == true ) {
				Texture =  reader.ReadString();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize( BinaryWriter writer )
		{
			writer.Write( Name );

			if ( Texture == null ) {
				writer.Write( false );
			} else {
				writer.Write( true );
				writer.Write( Texture );
			}
		}



		public bool Equals ( MaterialRef other )
		{
			if (other==null) return false;

			return ( Name		== other.Name		)
				&& ( Texture	== other.Texture	)
				;
		}


		public override bool Equals ( object obj )
		{
			if (obj==null) return false;
			if (obj as MaterialRef==null) return false;
			return Equals((MaterialRef)obj);
		}

		public override int GetHashCode ()
		{
			int hashCode = 0;
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ Texture.GetHashCode();
			//hashCode = (hashCode * 397) ^ Tag.GetHashCode(); ???
			return hashCode;
		}


		public static bool operator == (MaterialRef obj1, MaterialRef obj2)
		{
			if ((object)obj1 == null || ((object)obj2) == null)
				return Object.Equals(obj1, obj2);

			return obj1.Equals(obj2);
		}

		public static bool operator != (MaterialRef obj1, MaterialRef obj2)
		{
			if (obj1 == null || obj2 == null)
				return ! Object.Equals(obj1, obj2);

			return ! (obj1.Equals(obj2));
		}
	}
}
