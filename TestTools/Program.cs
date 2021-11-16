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
            string filePath = @"C:\\Users\\eric.silva\\desktop\\updatelist";
            string fileOutput = filePath + ".xml" ;

            var decrypted = new FileCrypt().DecryptEncryptFile(filePath, FileCrypt.KeyEnum.JP, FileCrypt.OperacaoEnum.Decrypt);
            System.IO.File.WriteAllBytes(fileOutput, decrypted);

            var encrypted = new FileCrypt().DecryptEncryptFile(fileOutput, FileCrypt.KeyEnum.JP, FileCrypt.OperacaoEnum.Encrypt);
            System.IO.File.WriteAllBytes(filePath + "_encrypt", encrypted);


        }
    }
}
