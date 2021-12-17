using PangyaPakMaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaPakMakerTest
{
    internal class Program
    {
        static void Main( string[] args)
        {
            centerText("-----------------------------------------------");
            centerText("|                  Pangya Pak Maker            |");
            centerText("|             Extract/Create Version 1.0       |");
            centerText("------------------------------------------------");
            centerText("Created by LuisMK");
            centerText("------------------------------------------------");
            PakMaker pakmaker = new PakMaker();
            if (args.Length == 0)
            {
                Console.WriteLine("No input file, Create PAK...\n");
                if (Directory.CreateDirectory("NewPak").Exists)
                {
                    Console.WriteLine("Folder NEWPAK found \n");

                    Console.WriteLine("--------------\n");
                    Console.WriteLine("Write the number of what you want\n");
                    Console.WriteLine("0 - XTEA - US\n");
                    Console.WriteLine("1 - XTEA - JP\n");
                    Console.WriteLine("2 - XTEA - TH\n");
                    Console.WriteLine("3 - XTEA - EU\n");
                    Console.WriteLine("4 - XTEA - ID\n");
                    Console.WriteLine("5 - XTEA - KR\n");
                    Console.WriteLine("6 - XOR  - (old format)\n");
                    int typepak = int.Parse(Console.ReadLine());

                    pakmaker.CreatePak(typepak);
                }
            }
            else if (args.Length > 0)
            {
              var result =  pakmaker.OpenPak(args[0]);
                if (result == PakResultEnum.Sucess) { pakmaker.Log(); }
            }
        }


        private static void centerText(String text)
        {
            Console.Write(new string(' ', (Console.WindowWidth - text.Length) / 2));
            Console.WriteLine(text);
        }
    }
}
