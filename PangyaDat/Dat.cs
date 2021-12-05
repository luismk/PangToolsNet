using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PangyaDat
{
    /// <summary>
    /// Main DAT file class
    /// </summary>
    public class DATFile
    {
        public enum IFFRegion
        {
            Default = -1,
            GB = 0,
            JP = 1,
            KR = 2,
            TH = 3
        }

        /// <summary>
        /// 
        /// </summary>
        public class FileDat
        {
            public FileDat(int value, string value2)
            {
                ID = value;
                Line = value2;
            }
            /// <summary>
            /// index line
            /// </summary>
            public int ID { get; set; }
            /// <summary>
            /// texto armazenarado
            /// </summary>
            public string Line { get; set; }
        }
        public List<FileDat> Entries = new List<FileDat>();

        public Encoding FileEncoding { get; set; }
        public IFFRegion Region { get; set; }

        public DATFile(string path)
        {
            LoadFile(path);
        }
        /// <summary>
        /// ler os arquivos .dat
        /// </summary>
        private void ReadDat(Stream data)
        {
            int id = 0;
            using (BinaryReader reader = new BinaryReader(data, FileEncoding))
            {
                List<char> stringChars = new List<char>();

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    if (reader.PeekChar() != 0x00)
                    {
                        stringChars.Add(reader.ReadChar());
                    }
                    else
                    {
                        char[] chars = stringChars.ToArray();
                        byte[] bytes = FileEncoding.GetBytes(chars);

                        Entries.Add(new FileDat(id, FileEncoding.GetString(bytes)));

                        reader.BaseStream.Seek(1L, SeekOrigin.Current);
                        stringChars = new List<char>();
                        id++;
                    }

                }
            }
        }

        /// <summary>
        /// obtem o tipo codificação/decodificação usada no arquivo
        /// </summary>
        /// <returns>retorna o encoding usado</returns>
        private Encoding GetEncoding(string filePath)
        {
            if (filePath == null)
            {
                throw new InvalidOperationException("No file path given to get encoding from, use SetEncoding() method!");
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();

            switch (fileName)
            {
                case "korea":
                    FileEncoding = Encoding.GetEncoding(51949);
                    Region = IFFRegion.KR;
                    break;
                case "japan":
                    FileEncoding = Encoding.GetEncoding(932);
                    Region = IFFRegion.JP;
                    break;
                case "english":
                    {
                        Region = IFFRegion.GB;
                        FileEncoding = Encoding.GetEncoding(874);
                    }
                    break;
                case "thailand":
                    FileEncoding = Encoding.GetEncoding(874);
                    Region = IFFRegion.TH;
                    break;
                case "indonesia":
                    FileEncoding = Encoding.GetEncoding(65001);
                    break;
                case "brasil":
                case "spanish":
                case "german":
                case "french":
                    FileEncoding = Encoding.GetEncoding(1252);
                    Region = IFFRegion.Default;
                    break;
                default:
                    FileEncoding = Encoding.GetEncoding(65001);
                    Region = IFFRegion.Default;
                    break;
            }
            return FileEncoding;
        }

        /// <summary>
        /// Returns the encoding used by the DATFile instance
        /// </summary>
        public Encoding GetEncoding()
        {
            return FileEncoding;
        }

        /// <summary>
        /// Método que realiza setagens e carregamento das informações do arquivo .dat
        /// </summary>
        /// <param name="filePath"> local onde está o arquivo </param>
        public DATFile LoadFile(string filePath)
        {
            GetEncoding(filePath);
            ReadDat(File.Open(filePath, FileMode.Open, FileAccess.Read));
            return this;
        }

        public void Save(string filePath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write), FileEncoding))
            {
                foreach (var entry in Entries)
                {

                    writer.Write(entry.Line.ToCharArray());
                    writer.Write((byte)0);
                }
            }
        }
    }
}
