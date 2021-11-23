using System;
using UpdateList;
namespace TestTools
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("UpdateList - Encrypt/Decrypt ");
            Console.WriteLine("Wait insert to file...");
            for (; ; )
            {
                var comando = Console.ReadLine().Split(new char[] { ' ' }, 2);
                if (args.Length > 0|| System.IO.File.Exists(comando[0]))
                {
                    string filePath = "updatelist";
                    if (comando.Length > 0)
                    {
                        filePath = comando[0];
                    }
                    else if (args.Length > 0)
                    {
                        filePath = args[0];
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        var result = new FileCrypt().DecryptEncryptFile(filePath, out byte[] decrypted, (FileCrypt.KeyEnum)i);

                        if (FileCrypt.Result.Sucess == result)
                        {
                            Console.WriteLine("Sucess ! ");
                            break;
                        }
                        else if(FileCrypt.Result.Test_New_Key == result) { Console.WriteLine("Testando nova chave..."); }
                    }
                }
                Console.ReadLine();
            }
        }
    }
}