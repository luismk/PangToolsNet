using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UpdateList
{
    public class FileCrypt
    {
        //Chaves
        private uint[] _xtea_US_key = { 66455465, 57629246, 17826484, 78315754 }; // 0x3F607A9, 0x36F5A3E, 0x11002B4, 0x4AB00EA
        private uint[] _xtea_JP_key = { 34234324, 32423423, 45336224, 83272673 }; // 0x20A5FD4, 0x1EEBDFF, 0x2B3C6A0, 0x4F6A3E1
        private uint[] _xtea_TH_key = { 84595515, 12254985, 72548314, 46875682 }; // 0x50AD33B, 0x0BAFF09, 0x452FFDA, 0x2CB4422
        private uint[] _xtea_EU_key = { 32081624, 92374137, 64139451, 46772272 }; // 0x1E986D8, 0x5818479, 0x3D2B0BB, 0x2C9B030
        private uint[] _xtea_ID_key = { 23334327, 21322395, 41884343, 93424468 }; // 0x1640DB7, 0x1455A9B, 0x27F1AB7, 0x5918B54
        private uint[] _xtea_KR_key = { 75871606, 85233154, 85204374, 42969558 }; // 0x485B576, 0x5148E02, 0x5141D96, 0x28FA9D6

        public enum Result
        {
            Sucess,
            Falied,
            Error,
            Test_New_Key
        }
        public enum KeyEnum
        {
            US,
            JP,
            TH,
            EU,
            ID,
            KR
        }

        public enum OperacaoEnum
        {
            Decrypt,
            Encrypt
        }

        /// <summary>
        /// Obtém Chave
        /// </summary>
        public uint[] GetKey(KeyEnum key)
        {
            switch (key)
            {
                case KeyEnum.US: return _xtea_US_key;
                case KeyEnum.JP: return _xtea_JP_key;
                case KeyEnum.TH: return _xtea_TH_key;
                case KeyEnum.EU: return _xtea_EU_key;
                case KeyEnum.ID: return _xtea_ID_key;
                case KeyEnum.KR: return _xtea_KR_key;
                default: throw new Exception("Chave inválida");
            }
        }

        /// <summary>
        /// Criptografa dados
        /// </summary>
        private uint[] xtea_encipher(int num_rounds, uint[] data, KeyEnum keyEnum)
        {
            var key = GetKey(keyEnum);

            var dataCopy = new uint[data.Length];

            //Cria uma cópia
            Buffer.BlockCopy(data, 0, dataCopy, 0, data.Length);

            uint i;
            uint data0 = data[0];
            uint data1 = data[1];
            uint delta = 0x61c88647;
            uint sum = 0;
            for (i = 0; i < num_rounds; i++)
            {
                data0 += (((data1 << 4) ^ (data1 >> 5)) + data1) ^ (sum + key[Convert.ToInt32(sum & 3)]);
                sum -= delta;
                data1 += (((data0 << 4) ^ (data0 >> 5)) + data0) ^ (sum + key[Convert.ToInt32((sum >> 11) & 3)]);
            }
            dataCopy[0] = data0;
            dataCopy[1] = data1;

            return dataCopy;
        }

        /// <summary>
        /// Decriptografa dados
        /// </summary>
        private uint[] xtea_decipher(int num_rounds, uint[] data, KeyEnum keyEnum)
        {
            var key = GetKey(keyEnum);

            var dataCopy = new uint[data.Length];

            //Cria uma cópia
            Buffer.BlockCopy(data, 0, dataCopy, 0, data.Length);

            uint i;
            uint data0 = data[0];
            uint data1 = data[1];
            uint delta = 0x61C88647, sum = 0xE3779B90;
            for (i = 0; i < num_rounds; i++)
            {
                data1 -= (((data0 << 4) ^ (data0 >> 5)) + data0) ^ (sum + key[Convert.ToInt32((sum >> 11) & 3)]);
                sum += delta;
                data0 -= (((data1 << 4) ^ (data1 >> 5)) + data1) ^ (sum + key[Convert.ToInt32(sum & 3)]);
            }

            dataCopy[0] = data0;
            dataCopy[1] = data1;

            return dataCopy;
        }

        // checar se está encriptado ou dwcriptografado
        public OperacaoEnum CheckCryptDecrypt(string filePath)
        {


            if (!File.Exists(filePath))
                throw new FileNotFoundException("Arquivo não encontrado");

            //Lê arquivo
            var data = File.ReadAllBytes(filePath);

            var dataResult = Encoding.UTF8.GetChars(data);
            if (dataResult[0] == '<' && dataResult[1] == '?')
            {
                Console.Write("Trying to Encrypt ... \n");
                if (dataResult[0x4B] == 'T' && dataResult[0x4C] == 'H')
                {
                    Console.Write("Encrypt Key found : Thai ... \n");
                    return OperacaoEnum.Encrypt;
                }
                else if (dataResult[0x4B] == 'J' && dataResult[0x4C] == 'P')
                {
                    Console.Write("Encrypt Key found : Japan ... \n");
                    return OperacaoEnum.Encrypt;
                }
                else if (dataResult[0x4B] == 'G' && dataResult[0x4C] == 'B')
                {
                    Console.Write("Encrypt Key found : English ... \n");
                    return OperacaoEnum.Encrypt;
                }
                else if (dataResult[0x4B] == 'E' && dataResult[0x4C] == 'U')
                {
                    Console.Write("Encrypt Key found : Europe ... \n");
                    return OperacaoEnum.Encrypt;
                }
                else if (dataResult[0x4B] == 'I' && dataResult[0x4C] == 'D')
                {
                    Console.Write("Encrypt Key found : Indonesia ... \n");
                    return OperacaoEnum.Encrypt;
                }
                else if (dataResult[0x4B] == 'K' && dataResult[0x4C] == 'R')
                {
                    Console.Write("Encrypt Key found : Korean ... \n");
                    return OperacaoEnum.Encrypt;
                }
                else
                {
                    //no have key :'(
                    Console.WriteLine("No Key found - Maybe Bad Decrypt :/ ... \n");
                }
            }

            return OperacaoEnum.Decrypt;
        }
        /// <summary>
        /// Decriptografa ou Criptografa Updatelist
        /// </summary>
        /// <param name="filePath">Caminho do arquivo</param>
        /// <param name="key">Chave do updatelist</param>
        /// <param name="operacao">Decriptografar ou Criptografar</param>
        public Result DecryptEncryptFile(string filePath, out byte[] decrypted, KeyEnum key)
        {

            var operacao = CheckCryptDecrypt(filePath);

            //Lê arquivo
            var data = File.ReadAllBytes(filePath);

            var dataResult = new byte[data.Length];
            //Resultado
            char[] updateresult = Encoding.UTF8.GetChars(data);
            //encrypt
            if (updateresult[0] == '<' && updateresult[1] == '?')
            {
                if (updateresult[0x4B] == 'T' && updateresult[0x4C] == 'H')
                {
                    key = KeyEnum.TH;
                }
                else if (updateresult[0x4B] == 'J' && updateresult[0x4C] == 'P')
                {
                    key = KeyEnum.JP;
                }
                else if (updateresult[0x4B] == 'G' && updateresult[0x4C] == 'B')
                {
                    key = KeyEnum.US;
                }
                else if (updateresult[0x4B] == 'E' && updateresult[0x4C] == 'U')
                {
                    key = KeyEnum.EU;
                }
                else if (updateresult[0x4B] == 'I' && updateresult[0x4C] == 'D')
                {
                    key = KeyEnum.ID;
                }
                else if (updateresult[0x4B] == 'K' && updateresult[0x4C] == 'R')
                {
                    key = KeyEnum.ID;
                }
                else
                {
                    //no have key :'(
                    Console.WriteLine("No Key found - Maybe Bad Decrypt :/ ... \n");
                }
            }


            for (int i = 0; i < data.Length; i = i + 8)
            {
                //Dados cortados
                var dataCut = new uint[8];

                Buffer.BlockCopy(data, srcOffset: i, dst: dataCut, dstOffset: 0, count: 8);

                if (operacao == OperacaoEnum.Encrypt)
                {
                    dataCut = xtea_encipher(16, dataCut, key);
                }
                else
                {
                    dataCut = xtea_decipher(16, dataCut, key);
                }

                Buffer.BlockCopy(dataCut, srcOffset: 0, dst: dataResult, dstOffset: i, count: 8);

                //If Decrypt fail ...
                if (i == 0 && dataResult[0] != '<' && dataResult[1] != '?' && operacao == OperacaoEnum.Decrypt)
                {
                    decrypted = dataResult;
                    return Result.Test_New_Key;
                }
            }

            decrypted = dataResult;
            var getcurrentdirectory = Directory.GetCurrentDirectory();
            if (operacao == OperacaoEnum.Encrypt)
            {
                File.WriteAllBytes(getcurrentdirectory+ "\\updatelist_encrypt", decrypted);
            }
            else if (operacao == OperacaoEnum.Decrypt)
            {
                File.WriteAllBytes(getcurrentdirectory+ "\\updatelist_decrypt.xml", decrypted);
            }
            return Result.Sucess;
        }
    }
}
