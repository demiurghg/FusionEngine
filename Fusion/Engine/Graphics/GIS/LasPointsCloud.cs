using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Microsoft.SqlServer.Server;

namespace Fusion.Engine.Graphics.GIS
{
	public class LasPointsCloud
	{
		public Game Game;

		public string FileName;

		public LasHeader Header;
		public VariableLengthRecordHeader[] VariableHeaders;
		public string UtmZone { get; set; }
		
		public PointDataRecord3[]	Points		{ protected set; get; }
		//public PointsGisLayer		GisPoints	{ protected set; get; }
		public CartesianPoints		CartPoints	{ protected set; get; }

		public LasPointsCloud(Game game, string fileName)
		{
			Game		= game;
			FileName	= fileName;

			using (var stream = File.OpenRead(fileName)) {
				BinaryReader reader = new BinaryReader(stream);

				Header = ReadHeader(reader);

				VariableHeaders = new VariableLengthRecordHeader[Header.NumberOfVariableLengthRecords];

				// Going to Variable Length Records
				reader.BaseStream.Seek(Header.HeaderSize, SeekOrigin.Begin);

				for (int i = 0; i < VariableHeaders.Length; i++) {
					VariableHeaders[i] = ReadVariableHeader(reader);
					VariableHeaders[i].Data = reader.ReadBytes(VariableHeaders[i].RecordLengthAfterHeader);
					VariableHeaders[i].DataString = Encoding.ASCII.GetString(VariableHeaders[i].Data).TrimEnd('\0');

					if (VariableHeaders[i].UserId.Equals("LASF_Projection")) {
						Match match = Regex.Match(VariableHeaders[i].DataString, @"UTM zone ([0-9]{2}[S,N])", RegexOptions.IgnoreCase);

						if (match.Success) {
							UtmZone = match.Groups[1].Value;
						}
					}
					//reader.BaseStream.Seek(VariableHeaders[i].RecordLengthAfterHeader, SeekOrigin.Current);
				}

				if (Header.PointDataFormatID != 3) {
					Log.Error("LasPointsCloud Error: PointDataFormatID is not equal to 3, only 3 is supported. File: " + fileName);
					reader.Dispose();
					return;
				}

				Points		= new PointDataRecord3[Header.NumberOfPointRecords];

				// Going to Points
				reader.BaseStream.Seek(Header.OffsetToPointData, SeekOrigin.Begin);
				for (int i = 0; i < Points.Length; i++) {
					Points[i] = ReadPoint3(reader);
				}
			}
		}


		public void BakePoints(double floorLevel, double xOffset = 35, double yOffset = 70)
		{
			CartPoints = new CartesianPoints(Game, Points.Length);

			for (int i = 0; i < Points.Length; i++) {
				double X = (double)Points[i].X * Header.XScaleFactor + Header.XOffset + xOffset;
				double Y = (double)Points[i].Y * Header.YScaleFactor + Header.YOffset + yOffset;
				double Z = ((double)Points[i].Z * Header.ZScaleFactor + floorLevel) * 0.001 + Header.ZOffset;

				double lon, lat;

				Gis.UtmToLatLon(X, Y, UtmZone, out lon, out lat);

				var cartPos = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(new DVector2(lon, lat)), GeoHelper.EarthRadius + Z);

				CartPoints.PointsCpu[i] = new Gis.CartPoint
				{
					X = cartPos.X,
					Y = cartPos.Y,
					Z = cartPos.Z,
					Color	= new Color(Points[i].Red / 255.0f, Points[i].Green / 255.0f, Points[i].Blue / 255.0f, 1.0f),
					Tex0	= new Vector4(0.0001f, 0, 0, 0)
				};

			}

			CartPoints.UpdatePointsBuffer();
		}


		public void BakePoints(double floorLevel, DVector3 pickCenter, double pickRadius = 1, double xOffset = 35, double yOffset = 70)
		{
			List<Gis.CartPoint> cartPoints = new List<Gis.CartPoint>();

			for (int i = 0; i < Points.Length; i++) {
				double X = (double)Points[i].X * Header.XScaleFactor + Header.XOffset + xOffset;
				double Y = (double)Points[i].Y * Header.YScaleFactor + Header.YOffset + yOffset;
				double Z = ((double)Points[i].Z * Header.ZScaleFactor + floorLevel) * 0.001 + Header.ZOffset;

				double lon, lat;

				Gis.UtmToLatLon(X, Y, UtmZone, out lon, out lat);

				var cartPos = GeoHelper.SphericalToCartesian(DMathUtil.DegreesToRadians(new DVector2(lon, lat)), GeoHelper.EarthRadius + Z);

				var dist = (pickCenter - cartPos).Length();
				if (dist > pickRadius) continue;

				float transparency = 2.0f - (float) (dist/pickRadius);

				if (pickRadius - dist < 0.05) transparency = (float)((pickRadius - dist)/0.05);

				cartPoints.Add(new Gis.CartPoint {
					X = cartPos.X,
					Y = cartPos.Y,
					Z = cartPos.Z,
					Color = new Color(Points[i].Red / 255.0f, Points[i].Green / 255.0f, Points[i].Blue / 255.0f, 1.0f),
					Tex0 = new Vector4(0.0001f, 0, 0, transparency)
				});

			}


			if (cartPoints.Count != 0) {
				CartPoints = new CartesianPoints(Game, cartPoints.Count);
				Array.Copy(cartPoints.ToArray(), CartPoints.PointsCpu, cartPoints.Count);
				CartPoints.UpdatePointsBuffer();
			}
		}


		public static LasHeader ReadHeader(BinaryReader reader)
		{
			LasHeader header = new LasHeader {
				FileSignature		= Encoding.ASCII.GetString(reader.ReadBytes(4)).TrimEnd('\0'),
				FileSourceID		= reader.ReadUInt16(),
				GlobalEncoding		= reader.ReadUInt16(),
				ProjectIDGUIDdata1	= reader.ReadUInt32(),
				ProjectIDGUIDdata2	= reader.ReadUInt16(),
				ProjectIDGUIDdata3	= reader.ReadUInt16(),
				ProjectIDGUIDdata4	= Encoding.ASCII.GetString(reader.ReadBytes(8)).TrimEnd('\0'),
				VersionMajor	= reader.ReadByte(),
				VersionMinor	= reader.ReadByte(),
				SystemIdentifier	= Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0'),
				GeneratingSoftware	= Encoding.ASCII.GetString(reader.ReadBytes(32)).TrimEnd('\0'),
				FileCreationDayOfYear	= reader.ReadUInt16(),
				FileCreationYear		= reader.ReadUInt16(),
				HeaderSize				= reader.ReadUInt16(),
				OffsetToPointData				= reader.ReadUInt32(),
				NumberOfVariableLengthRecords	= reader.ReadUInt32(),
				PointDataFormatID		= reader.ReadByte(),
				PointDataRecordLength	= reader.ReadUInt16(),
				NumberOfPointRecords	= reader.ReadUInt32(),
				NumberOfPointsByReturn	= ReadUInt32s(5, reader),
				XScaleFactor = reader.ReadDouble(),
				YScaleFactor = reader.ReadDouble(),
				ZScaleFactor = reader.ReadDouble(),
				XOffset = reader.ReadDouble(),
				YOffset = reader.ReadDouble(),
				ZOffset = reader.ReadDouble(),
				MaxX = reader.ReadDouble(),
				MinX = reader.ReadDouble(),
				MaxY = reader.ReadDouble(),
				MinY = reader.ReadDouble(),
				MaxZ = reader.ReadDouble(),
				MinZ = reader.ReadDouble()
			};

			return header;
		}



		public static VariableLengthRecordHeader ReadVariableHeader(BinaryReader r)
		{
			var header = new VariableLengthRecordHeader
			{
				Reserved				= r.ReadUInt16(),
				UserId					= Encoding.ASCII.GetString(r.ReadBytes(16)).TrimEnd('\0'),
				RecordId				= r.ReadUInt16(),
				RecordLengthAfterHeader = r.ReadUInt16(),
				Description				= Encoding.ASCII.GetString(r.ReadBytes(32)).TrimEnd('\0')
			};
			return header;
		}


		public static PointDataRecord3 ReadPoint3(BinaryReader r)
		{
			var point = new PointDataRecord3();
			point.X = r.ReadInt32();
			point.Y = r.ReadInt32();
			point.Z = r.ReadInt32();
			point.Intensity = r.ReadUInt16();

			byte aBunchOfStuff = r.ReadByte();

			point.ReturnNumber		= (byte)(aBunchOfStuff & 0x0007);
			point.NumberOfReturns	= (byte)(aBunchOfStuff & 0x0038);
			point.ScanDirectionFlag = (byte)(aBunchOfStuff & 0x0040);
			point.EdgeOfFlightLine	= (byte)(aBunchOfStuff & 0x0080);

			point.Classification	= r.ReadByte();
			point.ScanAngleRank		= r.ReadByte();
			point.UserData		= r.ReadByte();
			point.PointSourceID = r.ReadUInt16();
			point.GPSTime		= r.ReadDouble();
			point.Red	= (byte)(r.ReadUInt16() >> 8);
			point.Green = (byte)(r.ReadUInt16() >> 8);
			point.Blue	= (byte)(r.ReadUInt16() >> 8);
			
			return point;
		}


		public struct PointDataRecord3
		{
			public int X; // long 4 bytes *
			public int Y; // long 4 bytes *
			public int Z; // long 4 bytes *
			public ushort Intensity; // unsigned short 2 bytes
			public byte ReturnNumber; // 3 bits (bits 0, 1, 2) 3 bits *
			public byte NumberOfReturns; // (given pulse) 3 bits (bits 3, 4, 5) 3 bits *
			public byte ScanDirectionFlag; // 1 bit (bit 6) 1 bit *
			public byte EdgeOfFlightLine; // 1 bit (bit 7) 1 bit *
			public byte Classification; // unsigned char 1 byte *
			public byte ScanAngleRank; // (-90 to +90) – Left side unsigned char 1 byte *
			public byte UserData; // unsigned char 1 byte
			public ushort PointSourceID; // unsigned short 2 bytes *
			public double GPSTime; // double 8 bytes *
			public byte Red; // unsigned short 2 bytes *
			public byte Green; // unsigned short 2 bytes *
			public byte Blue; // unsigned short 2 bytes * 
		}


		public struct VariableLengthRecordHeader
		{
			public ushort Reserved;
			public string UserId;
			public ushort RecordId;
			public ushort RecordLengthAfterHeader;
			public string Description;
			public byte[] Data;
			public string DataString;
		}


		static uint[] ReadUInt32s(int count, BinaryReader r)
		{
			var ret = new uint[count];


			for (int i = 0; i < count; i++) {
				ret[i] = r.ReadUInt32();
			}


			return ret;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LasHeader
		{
			public string FileSignature;		// (“LASF”) char[4] 4 bytes *
			public ushort FileSourceID;			// unsigned short 2 bytes *
			public ushort GlobalEncoding;		// unsigned short 2 bytes *
			public uint ProjectIDGUIDdata1;		// unsigned long 4 bytes
			public ushort ProjectIDGUIDdata2;	// unsigned short 2 byte
			public ushort ProjectIDGUIDdata3;	// unsigned short 2 byte
			public string ProjectIDGUIDdata4;	// unsigned char[8] 8 bytes
			public byte VersionMajor;			// unsigned char 1 byte *
			public byte VersionMinor;			// unsigned char 1 byte *
			public string SystemIdentifier;		// char[32] 32 bytes *
			public string GeneratingSoftware;	// char[32] 32 bytes *
			public ushort FileCreationDayOfYear;	// unsigned short 2 bytes *
			public ushort FileCreationYear;			// unsigned short 2 bytes *
			public ushort HeaderSize;				// unsigned short 2 bytes *
			public uint OffsetToPointData;			// unsigned long 4 bytes *
			public uint NumberOfVariableLengthRecords;	// unsigned long 4 bytes *
			public byte PointDataFormatID;				// (0-99 for spec) unsigned char 1 byte *
			public ushort PointDataRecordLength;		// unsigned short 2 bytes *
			public uint NumberOfPointRecords;			// unsigned long 4 bytes *
			public uint[] NumberOfPointsByReturn;		// unsigned long[5] 20 bytes *
			public double XScaleFactor;					// Double 8 bytes *
			public double YScaleFactor;					// Double 8 bytes *
			public double ZScaleFactor;					// Double 8 bytes *
			public double XOffset;						// Double 8 bytes *
			public double YOffset;						// Double 8 bytes *
			public double ZOffset;						// Double 8 bytes *
			public double MaxX;							// Double 8 bytes *
			public double MinX;							// Double 8 bytes *
			public double MaxY;							// Double 8 bytes *
			public double MinY;							// Double 8 bytes *
			public double MaxZ;							// Double 8 bytes *
			public double MinZ;							// Double 8 bytes *
			//public ulong StartOfWaveformDataPacketRecord;	// Unsigned long long 8 bytes
		}
	}
}
