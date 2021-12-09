using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaGameGuardAPI
{
	public class Crypts
	{
		public readonly int HEADERSIZE = 16;//size GameGuardHeader
		public readonly byte[] HASHKEY = { 0x65, 0x63, 0x74, 0x47, 0x61, 0x6D, 0x65, 0x4D, 0x6F, 0x6E };
		public readonly int GGSIG1 = 0x32812622;
		public readonly int GGSIG2 = 0x32812621;
		public readonly byte[] RSAKEY = { 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x02, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0xFB, 0xE3, 0xFC, 0x09, 0xAF, 0xAE, 0x65, 0x8C, 0x96, 0x4C, 0xC5, 0x37, 0xD2, 0xA4, 0x77, 0xE7, 0x4C, 0x41, 0xC2, 0xCF, 0xF2, 0xFE, 0x2D, 0x9C, 0x80, 0x94, 0x0C, 0x88, 0x6D, 0xB3, 0x84, 0x9F, 0x8C, 0x22, 0xA0, 0xC9, 0xCD, 0xC0, 0xAB, 0x30, 0x65, 0x82, 0x42, 0x3C, 0xEE, 0x3C, 0xA8, 0xB7, 0x11, 0xD6, 0x22, 0xFA, 0xFB, 0x23, 0xF7, 0x72, 0xCD, 0xE7, 0xD0, 0x6F, 0x6A, 0x8E, 0x96, 0xE3 };
		public readonly bool DEBUG = true;
		public struct GameGuardHeader
		{
			public int Sign1 { get; set; }
			public int Filename_size { get; set; }
			public int Signature_size { get; set; }
			public int Sign2 { get; set; }
		}

		public Win32 Win32 { get; set; }
		public GameGuardHeader GGHeader { get; set; }
		public byte[] Data;
		public byte[] Signature { get; set; }

		
	//DataHdr: array[0..89] of byte = (
 // 	$4D, $75, $45, $6E, $67, $2E, $69, $6E, $69, $00, // name
 //   $35, $3C, $05, $11, $01, $07, $24, $B5, $6A, $19, $B2, $A8, $38, $F6, $BD, $E3, // sig
 //   $21, $7A, $03, $20, $5B, $97, $72, $71, $1F, $36, $48, $B5, $E1, $CB, $9C, $01, // na
 //   $AA, $21, $DE, $CA, $B4, $6E, $D0, $DD, $53, $0B, $11, $A8, $67, $EC, $CD, $E4, // tu
 //   $8D, $BA, $E2, $23, $9C, $74, $E7, $33, $BF, $F6, $9D, $3A, $66, $BC, $1B, $D6, // re

 //   $22, $26, $81, $32,  //keyF
 //   $0A, $00, $00, $00,  //FileName - 13(example PangyaUS.ini add 0 finish code)
 //   $40, $00, $00, $00,  //Signatures len - 64
 //   $21, $26, $81, $32   //KeyS
 // );
		public Crypts()
		{
			Win32 = new Win32();
			GGHeader = new GameGuardHeader();
			DEBUG = false;
			Data = new byte[0];
			Signature = new byte[0];
		}

		public void DecryptINI(ref string inputFile, ref string outputFile)
		{
			var inFile = new PangyaBinaryReader(File.Open(inputFile, FileMode.Open));
			if (File.Exists(inputFile) == false)
			{
				Console.Write("error");
				Console.Write("\n");
				Console.Read();
				return;
			}

			else
			{
				uint bfSize = inFile.GetPosition();
				inFile.Seek(0, 2);
				uint efSize = inFile.GetPosition();
				uint fSize = efSize - bfSize;

				inFile.Seek((int)fSize - HEADERSIZE, 0);

				GGHeader = (GameGuardHeader)inFile.Read(new GameGuardHeader());
				inFile.Seek((int)fSize - HEADERSIZE - GGHeader.Signature_size, 0);
				inFile.ReadBytes(out byte[] sig, GGHeader.Signature_size);
				inFile.Seek(0, 0);
				inFile.ReadBytes(out byte[] Data, (int)fSize - HEADERSIZE - GGHeader.Signature_size);
				
				inFile.Close();
				if (Win32.SetupCrypt())
				{
					if (Win32.VerifySignature(RSAKEY,Data, Convert.ToUInt32(fSize - HEADERSIZE - GGHeader.Signature_size), sig, (uint)GGHeader.Signature_size))
					{
						if (Win32.DecryptData(HASHKEY,ref Data, Convert.ToUInt32(fSize - HEADERSIZE - GGHeader.Filename_size - GGHeader.Signature_size) - 13))
						{
							var buffnew = new byte[Data.Length - GGHeader.Filename_size];
							Console.WriteLine("decrypted: {0}", inputFile);
							Buffer.BlockCopy(Data, 0, buffnew, 0, buffnew.Length);
							File.WriteAllBytes(Directory.GetCurrentDirectory() + "//" + outputFile, buffnew);
							Console.Write($"Data: \n{Encoding.ASCII.GetString(buffnew)}");
						}
						else
						{
							Console.Write("Failed to decrypt Data: {0:D}");
						}
					}
					else
					{
						Console.Write("Failed to verify signature: {0:x}");
					}
				}
				else
				{
					Console.Write("Failed to aquire context: {0:x}");
				}
				inFile.Close();
			}
			Win32.Clear(true, IntPtr.Zero);
		}
		/// <summary>
		/// not encrypt 100%, work not finish
		/// </summary>
		/// <param name="inputFile"></param>
		/// <param name="outputFile"></param>
		public void EncryptINI(ref string inputFile, ref string outputFile)
		{
			if (File.Exists(inputFile) == false)
			{
				Console.Write("error");
				Console.Write("\n");
				Console.Read();
				return;
			}
			else
			{
				using (var inFile = new PangyaBinaryReader(new MemoryStream(File.ReadAllBytes(inputFile))))
				{
					inFile.Seek(0, 0);
					uint bfSize = inFile.GetPosition();
					inFile.Seek(0, 2);
					uint efSize = inFile.GetPosition();
					uint fSize = (uint)(efSize - bfSize);

					inFile.Seek(0, 0);
					byte[] inData = new byte[fSize];
					inFile.ReadBytes(out inData, (int)fSize);
					if (Win32.SetupCrypt())
					{
						string hash = new string(new char[64]);

						inFile.Close();
						if (Win32.EncryptData(HASHKEY,ref inData, fSize, ref outputFile))
						{
							Console.Write("encrypted");

							using (var outFile = new PangyaBinaryWriter())
							{
								byte[] temp = inData;
								outFile.WriteBytes(temp, (int)fSize - 12);
								outFile.WriteStr("PangyaUS.ini", 13);
								byte[] hashBase = new byte[fSize + 1];
								Win32.ConcatCharStar(ref temp, ref hashBase, (int)fSize - 12, 0);
								temp = Encoding.UTF8.GetBytes("PangyaUS.ini\x0");
								Win32.ConcatCharStar(ref temp, ref hashBase, 13, (int)fSize - 12);
								Win32.CreateSignature(RSAKEY,ref hashBase, (uint)hashBase.Length - 1, ref hash);
								outFile.WriteStr(hash, 64);//signature?
								outFile.Write(GGSIG1);
								outFile.WriteUInt32(13);//PangyaUS.ini.
								outFile.Write(64);//signature size
								outFile.Write(GGSIG2);
								outFile.SaveWrite(outputFile);
							}
						}
						else
						{
							Console.Write("Failed to decrypt Data: {0:D}", 1);
						}
					}
					else
					{
						Console.Write("Failed to create signature: {0:D}", 2);
					}
				}
			}
			Win32.Clear(true, IntPtr.Zero);
		}

		public void Log()
		{
			if (DEBUG)
			{
				Console.Write("Sig1: {0}\n", GGHeader.Filename_size);
				Console.Write("Sig: {0}\n", Signature.HexDump());
				Console.Write($"Data Encrypted: {Data.HexDump()}");
			}
		}
	}
}
