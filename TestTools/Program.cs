using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

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

                            break;

                    }

                }

                Console.ReadLine();

            }

        }

    }
}
