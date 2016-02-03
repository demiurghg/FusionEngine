using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using System.IO;

namespace ShooterDemo {
	public class Snapshot {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entities"></param>
		/// <returns></returns>
		public static byte[] WriteSnapshot( GameEntityCollection entities )
		{
			var ents = entities.OrderBy( e => e.ID ).ToArray();

			using ( var ms = new MemoryStream() ) { 
				using ( var writer = new BinaryWriter(ms) ) {

					//	write number of entities :
					writer.Write( entities.Count );

					foreach ( var ent in ents ) {

						writer.Write( ent.TypeID );
						writer.Write( ent.ID );
						ent.Write( writer );
				
					}

					return ms.GetBuffer();
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="entities"></param>
		public static void ReadSnapshot ( byte[] snapshot, GameEntityCollection entities )
		{
			using ( var ms = new MemoryStream(snapshot) ) { 
				using ( var reader = new BinaryReader(ms) ) {

					//	read count :
					int count = reader.ReadInt32();

					//	read entities :
					for (int i=0; i<count; i++) {
						
						byte typeId	=	reader.ReadByte();
						int  id		=	reader.ReadInt32();

						var ent		=	entities[ id ];

						if (ent==null) {
							ent = GameEntity.Spawn( typeId );
							entities.Add( ent );
						}

						ent.Read( reader );
					}
				}
			}
		}
	}
}
