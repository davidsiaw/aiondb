// -----------------------------------------------------------------------
// <copyright file="BinaryXmlFile.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AionDBGenerator {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.IO;
	using System.Runtime.InteropServices;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class BinaryXmlFile {
		public BinaryXmlNode Root;
		public void Read(Stream input) {
			if (input.ReadU8() != 128) {
				throw new InvalidDataException("not a binary XML file");
			}
			BinaryXmlStringTable binaryXmlStringTable = new BinaryXmlStringTable();
			binaryXmlStringTable.Read(input, input.ReadPackedS32());
			this.Root = new BinaryXmlNode();
			this.Root.Read(input, binaryXmlStringTable);
		}
	}

	public class BinaryXmlNode {
		public string Name;
		public string Value;
		public Dictionary<string, string> Attributes;
		public List<BinaryXmlNode> Children;
		public void Read(Stream input, BinaryXmlStringTable table) {
			this.Name = table[input.ReadPackedS32()];
			this.Attributes = new Dictionary<string, string>();
			this.Children = new List<BinaryXmlNode>();
			this.Value = null;
			byte b = input.ReadU8();
			if ((b & 1) == 1) {
				this.Value = table[input.ReadPackedS32()];
			}
			if ((b & 2) == 2) {
				int num = input.ReadPackedS32();
				for (int i = 0; i < num; i++) {
					string key = table[input.ReadPackedS32()];
					string value = table[input.ReadPackedS32()];
					this.Attributes[key] = value;
				}
			}
			if ((b & 4) == 4) {
				int num2 = input.ReadPackedS32();
				for (int j = 0; j < num2; j++) {
					BinaryXmlNode binaryXmlNode = new BinaryXmlNode();
					binaryXmlNode.Read(input, table);
					this.Children.Add(binaryXmlNode);
				}
			}
		}
	}
	public class BinaryXmlStringTable {
		protected byte[] Data;
		public string this[int index] {
			get {
				if (index == 0) {
					return "";
				}
				return this.Data.ReadUnicodeZ(index * 2);
			}
		}
		public void Read(Stream input, int size) {
			this.Data = new byte[size];
			input.Read(this.Data, 0, size);
		}
	}
	public static class BinaryXmlFileHelpers {
		public static int ReadPackedS32(this Stream stream) {
			byte b = stream.ReadU8();
			int num = 0;
			int num2 = 0;
			while (b >= 128) {
				num |= (int)((int)(b & 127) << (num2 & 31));
				num2 += 7;
				b = stream.ReadU8();
			}
			return num | (int)((int)b << (num2 & 31));
		}
		public static string ReadTable(this byte[] data, int offset) {
			if (offset == 0) {
				return "";
			}
			return data.ReadUnicodeZ(2 * offset);
		}
	}
	public static class ByteHelpers {
		public static T ToStructure<T>(this byte[] data, int index) {
			int num = Marshal.SizeOf(typeof(T));
			if (index + num > data.Length) {
				throw new Exception("not enough data to fit the structure");
			}
			byte[] array = new byte[num];
			Array.Copy(data, index, array, 0, num);
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
			return result;
		}
		public static T ToStructure<T>(this byte[] data) {
			return data.ToStructure<T>(0);
		}
		public static string ReadASCIIZ(this byte[] data, int offset) {
			int num = offset;
			while (num < data.Length && data[num] != 0) {
				num++;
			}
			if (num == offset) {
				return "";
			}
			return Encoding.ASCII.GetString(data, offset, num - offset);
		}
		public static string ReadASCIIZ(this byte[] data, uint offset) {
			return data.ReadASCIIZ((int)offset);
		}
		public static string ReadUnicodeZ(this byte[] data, int offset) {
			int num = offset;
			while (num < data.Length && BitConverter.ToUInt16(data, num) != 0) {
				num += 2;
			}
			if (num == offset) {
				return "";
			}
			return Encoding.Unicode.GetString(data, offset, num - offset);
		}
		public static string ReadUnicodeZ(this byte[] data, uint offset) {
			return data.ReadUnicodeZ((int)offset);
		}
	}
	public static class NumberHelpers {
		public static short Swap(this short value) {
			ushort num = (ushort)((255 & (int)value >> 8) | (-256 & (int)value << 8));
			return (short)num;
		}
		public static ushort Swap(this ushort value) {
			return (ushort)((255 & (int)value >> 8) | (65280 & (int)value << 8));
		}
		public static int Swap(this int value) {
			return (int)((255 & (uint)value >> 24) | (65280 & (uint)value >> 8) | (16711680 & value << 8) | (-16777216 & value << 24));
		}
		public static uint Swap(this uint value) {
			return (uint)((255u & value >> 24) | (65280u & value >> 8) | (16711680u & (int)value << 8) | (4278190080u & (int)value << 24));
		}
		public static long Swap(this long value) {
			return (255L & (uint)value >> 56) | (65280L & (uint)value >> 40) | (16711680L & (uint)value >> 24) | (long)(unchecked((ulong)-16777216) & (ulong)((uint)value >> 8)) | (1095216660480L & (int)value << 8) | (280375465082880L & (int)value << 24) | (71776119061217280L & (int)value << 40) | (-72057594037927936L & (int)value << 56);
		}
		public static ulong Swap(this ulong value) {
			return (255uL & (uint)value >> 56) | (65280uL & (uint)value >> 40) | (16711680uL & (uint)value >> 24) | (unchecked((ulong)-16777216) & (uint)value >> 8) | (1095216660480uL & (ulong)value << 8) | (280375465082880uL & (ulong)value << 24) | (71776119061217280uL & (ulong)value << 40) | (18374686479671623680uL & (ulong)value << 56);
		}
		public static short LittleEndian(this short value) {
			if (!BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static ushort LittleEndian(this ushort value) {
			if (!BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static int LittleEndian(this int value) {
			if (!BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static uint LittleEndian(this uint value) {
			if (!BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static long LittleEndian(this long value) {
			if (!BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static ulong LittleEndian(this ulong value) {
			if (!BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static short BigEndian(this short value) {
			if (BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static ushort BigEndian(this ushort value) {
			if (BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static int BigEndian(this int value) {
			if (BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static uint BigEndian(this uint value) {
			if (BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static long BigEndian(this long value) {
			if (BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
		public static ulong BigEndian(this ulong value) {
			if (BitConverter.IsLittleEndian) {
				return value.Swap();
			}
			return value;
		}
	}
	public static class StreamHelpers {
		public static string ReadASCII(this Stream stream, uint size) {
			byte[] array = new byte[(int)((UIntPtr)size)];
			stream.Read(array, 0, array.Length);
			return Encoding.ASCII.GetString(array);
		}
		public static string ReadASCIIZ(this Stream stream) {
			int num = 0;
			byte[] array = new byte[64];
			while (true) {
				stream.Read(array, num, 1);
				if (array[num] == 0) {
					goto IL_42;
				}
				if (num >= array.Length) {
					if (array.Length >= 4096) {
						break;
					}
					Array.Resize<byte>(ref array, array.Length + 64);
				}
				num++;
			}
			throw new InvalidOperationException();
		IL_42:
			if (num == 0) {
				return "";
			}
			return Encoding.ASCII.GetString(array, 0, num);
		}
		public static void WriteASCII(this Stream stream, string value) {
			byte[] bytes = Encoding.ASCII.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}
		public static void WriteASCIIZ(this Stream stream, string value) {
			byte[] bytes = Encoding.ASCII.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
			stream.WriteByte(0);
		}
		public static uint ReadU32(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			return BitConverter.ToUInt32(array, 0);
		}
		public static uint ReadU32BE(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			return BitConverter.ToUInt32(array, 0).Swap();
		}
		public static void WriteU32(this Stream stream, uint value) {
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 4);
		}
		public static void WriteU32BE(this Stream stream, uint value) {
			byte[] bytes = BitConverter.GetBytes(value.Swap());
			stream.Write(bytes, 0, 4);
		}
		public static T ReadStructure<T>(this Stream stream) {
			int num = Marshal.SizeOf(typeof(T));
			byte[] array = new byte[num];
			if (stream.Read(array, 0, num) != num) {
				throw new Exception();
			}
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
			return result;
		}
		public static void WriteStructure<T>(this Stream stream, T structure) {
			int num = Marshal.SizeOf(typeof(T));
			byte[] array = new byte[num];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Marshal.StructureToPtr(structure, gCHandle.AddrOfPinnedObject(), false);
			gCHandle.Free();
			stream.Write(array, 0, array.Length);
		}
		public static ulong ReadU64(this Stream stream) {
			byte[] array = new byte[8];
			stream.Read(array, 0, 8);
			return BitConverter.ToUInt64(array, 0);
		}
		public static ulong ReadU64BE(this Stream stream) {
			byte[] array = new byte[8];
			stream.Read(array, 0, 8);
			return BitConverter.ToUInt64(array, 0).Swap();
		}
		public static int ReadS32(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			return BitConverter.ToInt32(array, 0);
		}
		public static int ReadS32LE(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			if (!BitConverter.IsLittleEndian) {
				return BitConverter.ToInt32(array, 0).Swap();
			}
			return BitConverter.ToInt32(array, 0);
		}
		public static int ReadS32BE(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			if (BitConverter.IsLittleEndian) {
				return BitConverter.ToInt32(array, 0).Swap();
			}
			return BitConverter.ToInt32(array, 0);
		}
		public static void WriteS32(this Stream stream, int value) {
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 4);
		}
		public static void WriteS32LE(this Stream stream, int value) {
			if (!BitConverter.IsLittleEndian) {
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 4);
		}
		public static void WriteS32BE(this Stream stream, int value) {
			if (BitConverter.IsLittleEndian) {
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 4);
		}
		public static long ReadS64(this Stream stream) {
			byte[] array = new byte[8];
			stream.Read(array, 0, 8);
			return BitConverter.ToInt64(array, 0);
		}
		public static long ReadS64BE(this Stream stream) {
			byte[] array = new byte[8];
			stream.Read(array, 0, 8);
			return BitConverter.ToInt64(array, 0).Swap();
		}
		public static char ReadS8(this Stream stream) {
			return (char)stream.ReadByte();
		}
		public static void WriteS8(this Stream stream, char value) {
			stream.WriteByte((byte)value);
		}
		public static bool ReadBoolean(this Stream stream) {
			return stream.ReadU8() > 0;
		}
		public static void WriteBoolean(this Stream stream, bool value) {
			stream.WriteU8((byte)(value ? 1 : 0));
		}
		public static byte ReadU8(this Stream stream) {
			return (byte)stream.ReadByte();
		}
		public static void WriteU8(this Stream stream, byte value) {
			stream.WriteByte(value);
		}
		public static int ReadAligned(this Stream stream, byte[] buffer, int offset, int size, int align) {
			if (size == 0) {
				return 0;
			}
			int result = stream.Read(buffer, offset, size);
			int num = size % align;
			if (num > 0) {
				stream.Seek((long)(align - num), SeekOrigin.Current);
			}
			return result;
		}
		public static void WriteAligned(this Stream stream, byte[] buffer, int offset, int size, int align) {
			if (size == 0) {
				return;
			}
			stream.Write(buffer, offset, size);
			int num = size % align;
			if (num > 0) {
				byte[] buffer2 = new byte[align - num];
				stream.Write(buffer2, 0, align - num);
			}
		}
		public static ushort ReadU16(this Stream stream) {
			byte[] array = new byte[2];
			stream.Read(array, 0, 2);
			return BitConverter.ToUInt16(array, 0);
		}
		public static ushort ReadU16LE(this Stream stream) {
			byte[] array = new byte[2];
			stream.Read(array, 0, 2);
			if (!BitConverter.IsLittleEndian) {
				return BitConverter.ToUInt16(array, 0).Swap();
			}
			return BitConverter.ToUInt16(array, 0);
		}
		public static ushort ReadU16BE(this Stream stream) {
			byte[] array = new byte[2];
			stream.Read(array, 0, 2);
			if (BitConverter.IsLittleEndian) {
				return BitConverter.ToUInt16(array, 0).Swap();
			}
			return BitConverter.ToUInt16(array, 0);
		}
		public static void WriteU16(this Stream stream, ushort value) {
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 2);
		}
		public static void WriteU16LE(this Stream stream, ushort value) {
			if (!BitConverter.IsLittleEndian) {
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 2);
		}
		public static void WriteU16BE(this Stream stream, ushort value) {
			if (BitConverter.IsLittleEndian) {
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 2);
		}
		public static short ReadS16(this Stream stream) {
			byte[] array = new byte[2];
			stream.Read(array, 0, 2);
			return BitConverter.ToInt16(array, 0);
		}
		public static short ReadS16LE(this Stream stream) {
			byte[] array = new byte[2];
			stream.Read(array, 0, 2);
			if (!BitConverter.IsLittleEndian) {
				return BitConverter.ToInt16(array, 0).Swap();
			}
			return BitConverter.ToInt16(array, 0);
		}
		public static short ReadS16BE(this Stream stream) {
			byte[] array = new byte[2];
			stream.Read(array, 0, 2);
			if (BitConverter.IsLittleEndian) {
				return BitConverter.ToInt16(array, 0).Swap();
			}
			return BitConverter.ToInt16(array, 0);
		}
		public static void WriteS16(this Stream stream, short value) {
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 2);
		}
		public static void WriteS16LE(this Stream stream, short value) {
			if (!BitConverter.IsLittleEndian) {
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 2);
		}
		public static void WriteS16BE(this Stream stream, short value) {
			if (BitConverter.IsLittleEndian) {
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 2);
		}
		public static float ReadF32(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			return BitConverter.ToSingle(array, 0);
		}
		public static void WriteF32(this Stream stream, float value) {
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 4);
		}
		public static float ReadF32BE(this Stream stream) {
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			uint value = BitConverter.ToUInt32(array, 0).Swap();
			array = BitConverter.GetBytes(value);
			return BitConverter.ToSingle(array, 0);
		}
		public static void WriteF32BE(this Stream stream, float value) {
			byte[] bytes = BitConverter.GetBytes(value);
			uint value2 = BitConverter.ToUInt32(bytes, 0).Swap();
			bytes = BitConverter.GetBytes(value2);
			stream.Write(bytes, 0, 4);
		}
		public static double ReadF64(this Stream stream) {
			byte[] array = new byte[8];
			stream.Read(array, 0, 8);
			return BitConverter.ToDouble(array, 0);
		}
		public static void WriteF64(this Stream stream, double value) {
			byte[] bytes = BitConverter.GetBytes(value);
			stream.Write(bytes, 0, 8);
		}
		public static double ReadF64BE(this Stream stream) {
			return BitConverter.Int64BitsToDouble((long)stream.ReadU64BE());
		}
		public static void WriteF64BE(this Stream stream, double value) {
			byte[] bytes = BitConverter.GetBytes(value);
			ulong value2 = BitConverter.ToUInt64(bytes, 0).Swap();
			bytes = BitConverter.GetBytes(value2);
			stream.Write(bytes, 0, 8);
		}
	}
}
