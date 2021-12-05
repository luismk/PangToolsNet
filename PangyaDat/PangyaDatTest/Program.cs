using PangyaDat;
using System;
using System.IO;
namespace PangyaDatTest
{
    internal class Program
    {
        static void Main(string[] args)

        {
            Console.Title = "PangToolsNet - Pangya Dat";
            Console.WriteLine("Pangya.dat Files");
            Console.WriteLine("Wait insert to file...");
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                if (args.Length > 0 || System.IO.File.Exists(comando[0]))
                {
                    string filePath = "english.dat";
                    if (comando.Length > 0)
                    {
                        filePath = comando[0];
                    }
                    else if (args.Length > 0)
                    {
                        filePath = args[0];
                    }
                    var dat = new DATFile(filePath);
                    Console.OutputEncoding = dat.FileEncoding;
                    Console.Title = $"PangToolsNet - Pangya Dat - File: { Path.GetFileNameWithoutExtension(filePath).ToLower()}.dat - Lines: {dat.Entries.Count}";
                    foreach (var entry in dat.Entries)
                    {
                        Console.WriteLine($"Index: {entry.ID} | Text: {entry.Line} ");
                        Console.WriteLine(Environment.NewLine);
                    }
                }
                Console.ReadLine();
            }
        }
    }
}
