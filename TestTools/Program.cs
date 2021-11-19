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
                if (args.Length > 0)
                {
                    string filePath = args[0];
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