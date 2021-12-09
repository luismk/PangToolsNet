using PangyaGameGuardAPI;
using System;
using System.IO;

namespace GameGuard
{

    internal class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "PangToolsNet - GameGuard Files";
            Console.WriteLine("by LuisMK\n");
            HelpCommand();
            Console.WriteLine("Wait insert to file...");
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                if (args.Length > 0 || System.IO.File.Exists(comando[0]))
                {
                    string filePath = "PangyaUS.ini";
                    var gg = new Crypts();
                    if (comando.Length > 0)
                    {
                        filePath = comando[0];
                    }
                    else if (args.Length > 0)
                    {
                        filePath = args[0];
                    }
                    switch (comando[0])
                    {
                        case "decrypt":
                            {
                                string outfile = Path.GetFileNameWithoutExtension(filePath).ToLower() + "Dec.ini";
                                gg.DecryptINI(ref filePath, ref outfile);
                                gg.Log();
                            }
                            break;
                            case "encrypt":
                            {
                                string outfile = Path.GetFileNameWithoutExtension(filePath).ToLower() + "En.ini";
                                gg.EncryptINI(ref filePath, ref outfile);
                            }
                            break;
                            default:
                            { }
                            break;
                    }
                    Console.Title = $"PangToolsNet - Pangya GameGuard - File: { Path.GetFileNameWithoutExtension(filePath).ToLower()}.ini - Sign1: {gg.GGHeader.Sign1} - Sign2: {gg.GGHeader.Sign2}";
                }
                Console.ReadLine();
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
