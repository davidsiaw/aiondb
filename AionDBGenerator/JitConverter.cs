using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.ComponentModel;
using Tao.OpenGl;
using Tao.DevIl;
using System.Drawing.Imaging;

namespace JitOpener {
	class JitConverter {

		public static Bitmap DDSDataToBMP(byte[] DDSData) {
			// Create a DevIL image "name" (which is actually a number)
			int img_name;
			Il.ilGenImages(1, out img_name);
			Il.ilBindImage(img_name);

			// Load the DDS file into the bound DevIL image
			bool a = Il.ilLoadL(Il.IL_DDS, DDSData, DDSData.Length);
			int err = Il.ilGetError();
			// Set a few size variables that will simplify later code

			int ImgWidth = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
			int ImgHeight = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
			Rectangle rect = new Rectangle(0, 0, ImgWidth, ImgHeight);

			// Convert the DevIL image to a pixel byte array to copy into Bitmap
			Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);

			// Create a Bitmap to copy the image into, and prepare it to get data
			Bitmap bmp = new Bitmap(ImgWidth, ImgHeight);
			BitmapData bmd =
			  bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

			// Copy the pixel byte array from the DevIL image to the Bitmap
			Il.ilCopyPixels(0, 0, 0,
			  Il.ilGetInteger(Il.IL_IMAGE_WIDTH),
			  Il.ilGetInteger(Il.IL_IMAGE_HEIGHT),
			  1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE,
			  bmd.Scan0);

			// Clean up and return Bitmap
			Il.ilDeleteImages(1, ref img_name);
			bmp.UnlockBits(bmd);
			return bmp;
		}

		public static unsafe Image GetImage(string jitfile) {


			using (FileStream fs = new FileStream(jitfile, FileMode.Open)) {

				JITHEADER jitheader;


				byte[] jithdr = new byte[0xc];
				fs.Read(jithdr, 0, jithdr.Length);

				fixed (byte* ptr = &jithdr[0]) {
					jitheader = (JITHEADER)Marshal.PtrToStructure((IntPtr)ptr, typeof(JITHEADER));
				}


				DDS_HEADER header = new DDS_HEADER();
				header.magicNumber = new byte[] { 68, 68, 83, 32 };
				header.ddspf.dwSize = 32;
				header.dwSize = 124;
				header.dwHeight = jitheader.dwHeight;
				header.dwWidth = jitheader.dwWidth;
				header.ddspf.dwFlags = DWFLAGS.DDPF_FOURCC;

				switch (jitheader.magicNumber) {
					case JITSIGN.JT31:
						header.ddspf.dwFourCC = DWFOURCC.DXT1;
						break;
					case JITSIGN.JT32:
						header.ddspf.dwFourCC = DWFOURCC.DXT2;
						break;
					case JITSIGN.JT33:
						header.ddspf.dwFourCC = DWFOURCC.DXT3;
						break;
					case JITSIGN.JT34:
						header.ddspf.dwFourCC = DWFOURCC.DXT4;
						break;
					case JITSIGN.JT35:
						header.ddspf.dwFourCC = DWFOURCC.DXT5;
						break;
				}


				header.dwMipMapCount = 0;
				header.dwHeaderFlags =
					DWHEADERFLAGS.DDSD_CAPS |
					DWHEADERFLAGS.DDSD_HEIGHT |
					DWHEADERFLAGS.DDSD_WIDTH;
				header.dwSurfaceFlags = DWSURFACEFLAGS.DDSCAPS_TEXTURE;
				header.dwReserved1 = new uint[11];
				header.dwReserved2 = new uint[3];


				byte[] stuff = new byte[fs.Length - 0xc];
				fs.Read(stuff, 0, stuff.Length);

				using (MemoryStream ms = new MemoryStream()) {

					byte[] hdr = new byte[0x80];
					fixed (byte* ptr = &hdr[0]) {
						Marshal.StructureToPtr(header, (IntPtr)ptr, false);
					}
					ms.Write(hdr, 0, hdr.Length);
					ms.Write(stuff, 0, stuff.Length);
					return DDSDataToBMP(ms.GetBuffer());
				}




			}
		}

		unsafe public static void ConvertJITToDDS(string jitfile) {



			using (FileStream fs = new FileStream(jitfile, FileMode.Open)) {

				JITHEADER jitheader;


				byte[] jithdr = new byte[0xc];
				fs.Read(jithdr, 0, jithdr.Length);

				fixed (byte* ptr = &jithdr[0]) {
					jitheader = (JITHEADER)Marshal.PtrToStructure((IntPtr)ptr, typeof(JITHEADER));
				}

				byte[] stuff = new byte[fs.Length - 0xc];
				fs.Read(stuff, 0, stuff.Length);


				using (FileStream dest = new FileStream(jitfile + ".dds", FileMode.Create)) {

					DDS_HEADER header = new DDS_HEADER();
					header.magicNumber = new byte[] { 68, 68, 83, 32 };
					header.ddspf.dwSize = 32;
					header.dwSize = 124;
					header.dwHeight = jitheader.dwHeight;
					header.dwWidth = jitheader.dwWidth;
					header.ddspf.dwFlags = DWFLAGS.DDPF_FOURCC;

					switch (jitheader.magicNumber) {
						case JITSIGN.JT31:
							header.ddspf.dwFourCC = DWFOURCC.DXT1;
							break;
						case JITSIGN.JT32:
							header.ddspf.dwFourCC = DWFOURCC.DXT2;
							break;
						case JITSIGN.JT33:
							header.ddspf.dwFourCC = DWFOURCC.DXT3;
							break;
						case JITSIGN.JT34:
							header.ddspf.dwFourCC = DWFOURCC.DXT4;
							break;
						case JITSIGN.JT35:
							header.ddspf.dwFourCC = DWFOURCC.DXT5;
							break;
					}

					header.dwMipMapCount = 0;
					//header.dwPitchOrLinearSize = 1048576;
					header.dwHeaderFlags =
						DWHEADERFLAGS.DDSD_CAPS |
						DWHEADERFLAGS.DDSD_HEIGHT |
						DWHEADERFLAGS.DDSD_WIDTH;
					header.dwSurfaceFlags = DWSURFACEFLAGS.DDSCAPS_TEXTURE;
					header.dwReserved1 = new uint[11];
					header.dwReserved2 = new uint[3];

					byte[] hdr = new byte[0x80];
					fixed (byte* ptr = &hdr[0]) {
						Marshal.StructureToPtr(header, (IntPtr)ptr, false);
					}

					dest.Write(hdr, 0, hdr.Length);
					dest.Write(stuff, 0, stuff.Length);


				}

			}
		}

		struct JITHEADER {
			public JITSIGN magicNumber;
			public uint dwWidth;
			public uint dwHeight ;

		}

		enum JITSIGN {
			JT31 = 0x3133544A,
			JT32 = 0x3233544A,
			JT33 = 0x3333544A,
			JT34 = 0x3433544A,
			JT35 = 0x3533544A,
		}

		enum DWFLAGS {
			DDPF_FOURCC = 4
		}
		
		
//        DDSD_CAPS	Required in every .dds file.	0x1
//DDSD_HEIGHT	Required in every .dds file.	0x2
//DDSD_WIDTH	Required in every .dds file.	0x4
//DDSD_PITCH	Required when pitch is provided for an uncompressed texture.	0x8
//DDSD_PIXELFORMAT	Required in every .dds file.	0x1000
//DDSD_MIPMAPCOUNT	Required in a mipmapped texture.	0x20000
//DDSD_LINEARSIZE	Required when pitch is provided for a compressed texture.	0x80000
//DDSD_DEPTH	Required in a depth texture.	0x800000

//        DDSCAPS_COMPLEX	Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).	0x8
//DDSCAPS_MIPMAP	Optional; should be used for a mipmap.	0x400000
//DDSCAPS_TEXTURE	Required	0x1000

		[Flags]
		enum DWSURFACEFLAGS {
			DDSCAPS_COMPLEX = 0x8,
			DDSCAPS_MIPMAP = 0x400000,
			DDSCAPS_TEXTURE = 0x1000,
		}

		[Flags]
		enum DWHEADERFLAGS {
			DDSD_CAPS = 0x1,
			DDSD_HEIGHT = 0x2,
			DDSD_WIDTH = 0x4,
			DDSD_PITCH = 0x8,
			DDSD_PIXELFORMAT = 0x1000,
			DDSD_MIPMAPCOUNT = 0x20000,
			DDSD_LINEARSIZE = 0x80000,
			DDSD_DEPTH = 0x800000,
		}

		enum DWFOURCC {
			DXT1 = 0x31545844,
			DXT2 = 0x32545844,
			DXT3 = 0x33545844,
			DXT4 = 0x34545844,
			DXT5 = 0x35545844
		}


		[StructLayout(LayoutKind.Sequential)]
		struct DDS_PIXELFORMAT {
			public uint dwSize;
			public DWFLAGS dwFlags;
			public DWFOURCC dwFourCC;
			public uint dwRGBBitCount;
			public uint dwRBitMask;
			public uint dwGBitMask;
			public uint dwBBitMask;
			public uint dwABitMask;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct DDS_HEADER {
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public byte[] magicNumber;
			public uint dwSize;
			public DWHEADERFLAGS dwHeaderFlags;
			public uint dwHeight;
			public uint dwWidth;
			public uint dwPitchOrLinearSize;
			public uint dwDepth;
			public uint dwMipMapCount;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 11, ArraySubType = UnmanagedType.U4)]
			public uint[] dwReserved1;
			public DDS_PIXELFORMAT ddspf;
			public DWSURFACEFLAGS dwSurfaceFlags;
			public uint dwCubemapFlags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
			public uint[] dwReserved2;
		}


	}
}
