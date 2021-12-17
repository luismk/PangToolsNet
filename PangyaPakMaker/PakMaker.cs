using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangyaPakMaker
{
    public class PakMaker
    {
        public List<PangyaPakFile> PakFiles= new List<PangyaPakFile>();
        public PangyaPakHeader PakHeader = new PangyaPakHeader();
        public string FilePath;
        public uint[] Pak_Key { get; set; }

        public PangyaPakEnum PakType { get; set; } = PangyaPakEnum.PakNoSelected;
        
        public PakResultEnum OpenPak(string path)
        {
            FilePath = path;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(FilePath))))
            {
                reader.BaseStream.Seek(-9L, SeekOrigin.End);

                PakHeader.ListOffSet = reader.ReadUInt32();
                PakHeader.Count = reader.ReadUInt32();
                PakHeader.Signature = reader.ReadByte();
                Console.WriteLine("OffSet -> {0}", PakHeader.ListOffSet);
                Console.WriteLine("Count -> {0}", PakHeader.Count);
                Console.WriteLine("Sign -> {0}", PakHeader.Signature);
                if (PakHeader.Signature != 0x12)
                {
                    return PakResultEnum.Error;
                    throw new NotSupportedException("The signature of this PAK file is invalid!");
                }

                reader.BaseStream.Seek(PakHeader.ListOffSet, SeekOrigin.Begin);

                for (uint i = 0; i < PakHeader.Count; i++)
                {
                    PangyaPakFile fileEntry = new PangyaPakFile
                    {
                        FileNameLength = reader.ReadByte(),
                        Compression = reader.ReadByte(),
                        Offset = reader.ReadUInt32(),
                        FileSize = reader.ReadUInt32(),
                        RealFileSize = reader.ReadUInt32()
                    };

                     FindPakLang(fileEntry.Offset, fileEntry.RealFileSize);
                    Pak_Key = PublicKeyTable.XTEA_ALL_KEY[(int)PakType];
                    if (PakType == PangyaPakEnum.PakUnknown)
                    {
                        Console.WriteLine("The Pak version Invalid !");
                    }

                    byte[] tempName = reader.ReadBytes(fileEntry.FileNameLength);

                    if (fileEntry.Compression < 4)
                    {
                        reader.BaseStream.Seek(1L, SeekOrigin.Current);
                        fileEntry.FileName = DecryptFileName(tempName, Pak_Key);
                        reader.BaseStream.Seek(1L, SeekOrigin.Current);
                    }
                    else
                    {
                       

                        fileEntry.Compression ^= 0x20;

                        fileEntry.FileName = DecryptFileName(tempName, Pak_Key);

                        uint[] decryptionData =
                        {
                            fileEntry.Offset,
                            fileEntry.RealFileSize
                        };

                        uint[] resultData = XTEA.Decipher(16, decryptionData, Pak_Key);

                        fileEntry.Offset = resultData[0];
                        fileEntry.RealFileSize = resultData[1];
                    }

                    PakFiles.Add(fileEntry);
                }
            }
            return PakResultEnum.Sucess;
        }

        public void CreatePak(int EncryptType)
        {
            List<string> ListOfFile = new List<string>();
            List<string> ListOfFolder = new List<string>();
            int CurPos = 0;


            ListFile(ListOfFile, ListOfFolder);
            Console.Write("Number of File found : {0:D} \n", ListOfFile.Count);

            // PART 1 - Put ALL data in the file ... And save Offset and all 
            uint NumFiles = (uint)(ListOfFile.Count + ListOfFolder.Count);
            byte Sign = 0x12;
            int FakeSize = 0;
            uint[] XTeaInfo = new uint[2];

            string SaveFileName = "projectg999";
            switch ((PangyaPakEnum)EncryptType)
            {
                case PangyaPakEnum.GB:
                    SaveFileName += "gb.pak";
                    break;
                case PangyaPakEnum.JP:
                    SaveFileName += "jp.pak";
                    break;
                case PangyaPakEnum.TH:
                    SaveFileName += "th.pak";
                    break;
                case PangyaPakEnum.EU:
                    SaveFileName += "eu.pak";
                    break;
                case PangyaPakEnum.ID:
                    SaveFileName += "id.pak";
                    break;
                case PangyaPakEnum.KR:
                    SaveFileName += "kr.pak";
                    break;
                case PangyaPakEnum.PakOld:
                    SaveFileName += "th.pak";
                    break;
            }

            var OutPak = new PangyaBinaryWriter(File.Open(SaveFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite));

            PakFiles = new List<PangyaPakFile>(ListOfFile.Count + 1);

            for (int i = 0; i < ListOfFile.Count; i++)
            {
                string Filename = ListOfFile[i];

                string RealFileName = "NEWPAK\\";
                RealFileName += Filename;

                var FileData = File.Open(RealFileName, FileMode.OpenOrCreate);

                if (FileData == null)
                {
                    Console.Write("File not found : {0} \n", RealFileName);
                    continue;
                }

                OutPak.Seek(0, SeekOrigin.End);
                PakFiles.Add(new PangyaPakFile()
                {
                    FileName = Filename,
                    FileNameLength = (byte)(ListOfFile[i].Length),
                    Compression = 3,
                    Offset = (uint)OutPak.BaseStream.Position,
                    RealFileSize = (uint)OutPak.BaseStream.Position,
                });

                byte[] DATAin = new byte[PakFiles[i].RealFileSize + 1];
                byte[] DATAout = new byte[1024 * 4096];

                //Now Read File
                OutPak.Seek(0, 0);
               
                
                FileData.Read(DATAin, (int)PakFiles[i].RealFileSize, 1);

                //Now need to Compress for know the new size
                uint newsize = (uint)LZ77.Compress(DATAin, PakFiles[i].RealFileSize, ref DATAout);
                PakFiles[i].FileSize = newsize;

                OutPak.Write(DATAout, 0, (int)newsize);

                Console.Write("File Compressed : {0} \n", RealFileName);
                DATAout = null;
            }

            uint OffsetList = (uint)OutPak.BaseStream.Position;

            byte FolderMode = 0x02;

            if (EncryptType != 6)
            {
                FolderMode ^= 0x20;
            }

            // PART 2 - DO THE TABLE-ROLL
            Console.Write("Create table ... ");
            //Folder !
            for (int i = 0; i < ListOfFolder.Count; i++)
            {
                string sFolderName = ListOfFolder[i];
                string FolderName = new string(new char[ListOfFolder[i].Length + 9 - 1]);
                uint[] FolderNameInt = Array.ConvertAll(FolderName.ToArray(), s => uint.Parse(s.ToString()));

                FolderName = sFolderName;

                uint size = (uint)(ListOfFolder[i].Length);
                uint Offset = 0;
                uint RealFileSize = 0;
                if (EncryptType != 6)
                {
                    if (size < 8)
                    {
                        XTEA.Encipher(16, FolderNameInt, PublicKeyTable.XTEA_ALL_KEY[EncryptType]);
                    }
                    else
                    {
                        for (int y = 0; y < ListOfFolder[i].Length; y = y + 8)
                        {
                            uint[] DataCut = new uint[8];

                            Buffer.BlockCopy(DataCut, 0, FolderNameInt, 0, 8);

                            XTEA.Encipher(16, DataCut, PublicKeyTable.XTEA_ALL_KEY[EncryptType]);

                            Buffer.BlockCopy(FolderNameInt, 0, DataCut, 0, 8);

                            size = (uint)y;
                        }
                    }

                    sFolderName = FolderName;

                    XTeaInfo[0] = Offset;
                    XTeaInfo[1] = RealFileSize;
                    XTEA.Encipher(16, XTeaInfo, PublicKeyTable.XTEA_ALL_KEY[EncryptType]);
                    Offset = XTeaInfo[0];
                    RealFileSize = XTeaInfo[1];
                }
                else
                {
                    sFolderName = XOR_data(FolderName.ToCharArray(), (int)size, 1);
                }

                size = (uint)sFolderName.Length;

                OutPak.Write(size);
                OutPak.Write(FolderMode);
                OutPak.Write(Offset);
                OutPak.Write(FakeSize);
                OutPak.Write(RealFileSize);
                OutPak.WriteStr(sFolderName, (int)size);

                if (EncryptType == 6)
                {
                    OutPak.Write(FakeSize);
                }
            }
        }
        private void ListFile(List<string> FileList, List<string> FolderList)
        {
            FileList.Clear();


            WIN32_FIND_DATA file;

            string OriginalDir = "NEWPAK";
            string Mask = "\\*";
            string ActualFolder = OriginalDir + Mask;

            int i = -1;
            do
            {

                if (i != -1 && FolderList.Count >0)
                {
                    ActualFolder = OriginalDir + "\\" + FolderList[i] + Mask;
                }
                
                var hSearch =FindFirstFile(ActualFolder, out file);

                if (hSearch != new IntPtr(-1))
                {
                    do
                    {
                        var result = string.Compare(file.cFileName, ".");
                        if ( result >=0 || string.Compare(file.cFileName, "..") > 0 || string.Compare(file.cFileName, "") > 0)
                        {
                            if ((file.dwFileAttributes & 16) != 0 && i == -1)
                            {
                                FolderList.Add(file.cFileName);
                            }
                            else if (file.dwFileAttributes != 16 && i == -1)
                            {
                                FileList.Add(file.cFileName);
                            }
                            else if ((file.dwFileAttributes & 16) == 204)
                            {
                                FolderList.Add(FolderList[i] + "\\" + file.cFileName);
                            }
                            else
                            {
                                FileList.Add(FolderList[i] + "\\" + file.cFileName);
                            }
                        }
                    } while (FindNextFile(hSearch, out file));

                    FindClose(hSearch);
                }
                i++;
            } while (i < FolderList.Count);
        }

        string DecryptFileName(byte[] fileNameBuffer, uint[] key)
        {
            for (int y = 0; y < fileNameBuffer.Length; y = y + 8)
            {
                uint[] DataCut = new uint[8];
                Buffer.BlockCopy(fileNameBuffer, y, DataCut, 0, 8);

                DataCut= XTEA.Decipher(16, DataCut,key);
                Buffer.BlockCopy(DataCut, 0, fileNameBuffer, y,8);

            }
            string Return = Encoding.UTF8.GetString(fileNameBuffer.ToArray().TakeWhile(x => x != 0x00).ToArray());
            
            return Return;
        }

        string EncryptFileName(byte[] fileNameBuffer, uint[] key)
        {
            for (int y = 0; y < fileNameBuffer.Length; y = y + 8)
            {
                uint[] DataCut = new uint[8];
                Buffer.BlockCopy(fileNameBuffer, y, DataCut, 0, 8);

                DataCut = XTEA.Encipher(16, DataCut, key);
                Buffer.BlockCopy(DataCut, 0, fileNameBuffer, y, 8);

            }
            string Return = Encoding.UTF8.GetString(fileNameBuffer);

            return Return;
        }

        public void Log()
        {
            Console.WriteLine("Pak Name = {0}, CountFile = {1} ", Path.GetFileNameWithoutExtension(FilePath)  +".pak", PakFiles.Count);
           
            foreach (var pak in PakFiles)
            {
                Console.WriteLine(pak.FileName);
                Console.WriteLine(pak.FileSize);
                Console.WriteLine(pak.RealFileSize);
                Console.WriteLine(pak.Compression);
            }
            SaveFile();
        }

        public void SaveFile()
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(FilePath))))
            {
                byte[] data = null;

                PakFiles.ForEach(fileEntry =>
                {
                    reader.BaseStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
                    data = reader.ReadBytes((int)fileEntry.FileSize);

                    switch (fileEntry.Compression)
                    {
                        case 1:
                        case 3:
                            data = LZ77.Decompress(data, fileEntry.FileSize, fileEntry.RealFileSize,
                                fileEntry.Compression);
                            break;
                            //create directory for decrypt pak
                        case 2:
                            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(FilePath) + "//" + fileEntry.FileName); 
                            break;
                        default:
                            Debug.WriteLine($"Unknown compression value '{fileEntry.Compression.ToString()}'");
                            break;
                    }

                    if (fileEntry.FileSize != 0)
                    {
                        File.WriteAllBytes(Path.GetFileNameWithoutExtension(FilePath) +"//"+ fileEntry.FileName, data);
                    }
                });
            }
        }
        public void FindPakLang(uint Data0, uint Data1)
        {

            var XTeaInfo = XTEA.Decipher(16, new uint[] { Data0, Data1 }, PublicKeyTable.XTEA_ALL_KEY[0]);

            if (XTeaInfo[0] == 0)
                this.PakType=  PangyaPakEnum.GB; // US KEY

            XTeaInfo = XTEA.Decipher(16, new uint[] { Data0, Data1 }, PublicKeyTable.XTEA_ALL_KEY[1]);
         if (XTeaInfo[0] == 0)
            { PakType = PangyaPakEnum.JP; }// JAPAN KEY

            XTeaInfo = XTEA.Decipher(16, new uint[] { Data0, Data1 }, PublicKeyTable.XTEA_ALL_KEY[2]);
            if (XTeaInfo[0] == 0)
                PakType= PangyaPakEnum.TH; // THAI KEY

            XTeaInfo = XTEA.Decipher(16, new uint[] { Data0, Data1 }, PublicKeyTable.XTEA_ALL_KEY[3]);
            if (XTeaInfo[0] == 0)
                PakType = PangyaPakEnum.EU; // Europe KEY

            XTeaInfo = XTEA.Decipher(16, new uint[] { Data0, Data1 }, PublicKeyTable.XTEA_ALL_KEY[4]);
            if (XTeaInfo[0] == 0)
                PakType = PangyaPakEnum.ID; // INDONESIA KEY

            XTeaInfo = XTEA.Decipher(16, new uint[] { Data0, Data1 }, PublicKeyTable.XTEA_ALL_KEY[5]);
            if (XTeaInfo[0] == 0)
                PakType = PangyaPakEnum.KR; // KOREAN KEY

            if(PakType== PangyaPakEnum.PakNoSelected)
            //i don't know the key :(
            PakType = PangyaPakEnum.PakUnknown;
        }


       string XOR_data(char[] Data, int DataSize, int Type)
        {
            if (Type == 1)
            {
                for (int i = 0; i < DataSize; i++)
                {
                    Data[i] ^= Convert.ToChar(0x71u);
                }
            }

            string Return = Data.ToString();
            Return = Return.Substring(0, DataSize);
            return Return;
        }


        [DllImport("kernel32", CharSet = CharSet.Auto)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        public static extern bool FindClose(IntPtr hFindFile);
    }

    public class PangyaPakHeader
    {
        public uint ListOffSet { get; set; }
        public uint Count { get; set; }
        public byte Signature { get; set; }
    }
    /// <summary>
    /// Main structure of file entries
    /// </summary>
    public class PangyaPakFile : IEquatable<PangyaPakFile>
    {
        /// <summary>
        /// Length of the file name
        /// </summary>
        public byte FileNameLength { get; set; }

        /// <summary>
        /// Compression flag determining if the file is compressed, or a directory
        /// </summary>
        public byte Compression { get; set; }

        /// <summary>
        /// Offset of the file data from the beginning of the archive
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// (Compressed) size of the file
        /// </summary>
        public uint FileSize { get; set; }

        /// <summary>
        /// Real size of the file
        /// </summary>
        public uint RealFileSize { get; set; }

        /// <summary>
        /// Full path and name of the file
        /// </summary>
        public string FileName { get; set; }

        public bool Equals(PangyaPakFile other)
        {
            return FileNameLength == other.FileNameLength && Compression == other.Compression &&
                   Offset == other.Offset && FileSize == other.FileSize && RealFileSize == other.RealFileSize &&
                   string.Equals(FileName, other.FileName);
        }
    }

  public  struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

}
