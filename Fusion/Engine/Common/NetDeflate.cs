using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Shell;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using System.Net;
using System.Net.Sockets;
using Lidgren.Network;
using System.IO.Compression;
using System.IO;

namespace Fusion.Engine.Common {

	internal static class NetDeflate {

		
		/// <summary>
		/// Compresses byte array.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Compress(byte[] data)
		{
			using (var compressedStream = new MemoryStream()) {

				using (var zipStream = new DeflateStream(compressedStream, CompressionLevel.Fastest)) {

					zipStream.Write(data, 0, data.Length);
					zipStream.Close();
					return compressedStream.ToArray();
				}
			}
		}



		/// <summary>
		/// Decompresses byte array
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decompress(byte[] data)
		{
			using (var compressedStream = new MemoryStream(data)) {

				using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress)) {

					using (var resultStream = new MemoryStream()) {

						zipStream.CopyTo(resultStream);
						return resultStream.ToArray();
					}
				}
			}
		}
	}

}
