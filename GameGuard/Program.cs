using PangyaGameGuardAPI;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GameGuard
{

    internal class Program
    {
        static void Main(string[] args)
        {
            var test = new Crypts().RSAKEY;
            Console.Title = "PangToolsNet - GameGuard Files";
            Console.WriteLine("by LuisMK\n");
            HelpCommand();
            Console.WriteLine("Wait insert to file...");
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                if (File.Exists(comando[1]))
                {
                    string filePath = "PangyaUS.ini";
                    var gg = new Crypts();
                    if (comando.Length > 0)
                    {
                        filePath = comando[1];
                    }
                    switch (comando[0])
                    {
                        case "decrypt":
                            {
                                string outfile = Path.GetFileNameWithoutExtension(filePath) + "_Dec.ini";
                                gg.DecryptINI(ref filePath, ref outfile);
                                gg.Log();
                            }
                            break;
                            case "encrypt":
                            {
                                string outfile = Path.GetFileNameWithoutExtension(filePath) + "_En.ini";
                                gg.EncryptINI(ref filePath, ref outfile);
                            }
                            break;
                            default:
                            { }
                            break;
                    }
                    Console.Title = $"PangToolsNet - Pangya GameGuard - File: { Path.GetFileNameWithoutExtension(filePath)}.ini - Sign1: {gg.GGHeader.GameGuardTwo.Sign1} - Sign2: {gg.GGHeader.GameGuardTwo.Sign2}";
                }
                Console.ReadLine();
                Console.WriteLine();
            }
        }

        static void HelpCommand()
        {
            Console.WriteLine("PangyaUS.ini(example) file decryption tool\n");
            Console.WriteLine("use the command \ndecrypt or encrypt(does not work 100 % needs to work more)");
            Console.WriteLine("Example: decrypt C:\\PangYa_JP\\PangyaUS.ini");
        }
    }
}
