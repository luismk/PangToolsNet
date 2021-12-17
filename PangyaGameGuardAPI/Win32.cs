using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangyaGameGuardAPI
{

    /// <summary>
    /// WinAPI Imports
    /// </summary>
    public class WinCrypt
    {
        #region Import func dll
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptAcquireContext(out IntPtr hProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDestroyKey(IntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptCreateHash(IntPtr hProv, uint Algid, IntPtr hKey, uint dwFlags, out IntPtr hHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptHashData(IntPtr hHash, [In, Out] byte[] pbData, int dwDataLen, uint dwSize);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDestroyHash(IntPtr hHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDeriveKey(IntPtr hProv, uint Algid, IntPtr hHash, uint dwFlags, out IntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGenKey(IntPtr hProv, uint Algid, uint dwFlags, out IntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptEncrypt(IntPtr hKey, IntPtr hHash, int final, uint dwFlags, [In, Out] byte[] pbData, ref int pdwDataLen, int dwBufLen);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptExportKey(IntPtr hKey, IntPtr hExpKey, uint dwBlobType, uint dwFlags, [Out] byte[] pbData, ref int pdwDataLen);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetKeyParam(IntPtr hKey, uint dwParam, [Out] byte[] pbData, ref int pdwDataLen, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptDecrypt(IntPtr hKey, IntPtr hHash, bool final, uint dwFlags, [In, Out] byte[] pbData, ref uint pdwDataLen);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptImportKey(

         IntPtr hProv,

         Byte[] pbData,

         Int32 dwDataLen,

         IntPtr hPubKey,

         Int32 dwFlags,

         ref IntPtr phKey

     );


        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetHashParam(
            IntPtr hHash, uint dwParam, byte[] pbData, ref Int32 out_NumHashBytes, uint dwFlags

        );
       

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptVerifySignature(

            IntPtr hHash,

            Byte[] pbSignature,

            Int32 dwSigLen,

            IntPtr hPubKey,

            String sDescription,

            Int32 dwFlags

        );



        public const uint PROV_RSA_FULL = 1;
        public const uint NTE_BAD_KEYSET = 0x80090016;
        public const uint CRYPT_NEWKEYSET = 0x00000008;
        public const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
        public const uint CRYPT_MACHINE_KEYSET = 0x00000020;
        public const uint CALG_RC4 = 0x00006801;
        public const uint CRYPT_EXPORTABLE = 1;
        public const uint HP_ALGID = 0x0001;  // Hash algorithm
        public const int HP_HASHVAL = 0x0002; // Hash value
        public const int HP_HASHSIZE = 0x0004;// Hash value size
        public const uint ALG_CLASS_HASH = (4 << 13);
        public const uint ALG_SID_MD5 = 3;
        public const uint CALG_MD5 = (ALG_CLASS_HASH | ALG_SID_MD5);
        public const uint ALG_CLASS_DATA_ENCRYPT = (3 << 13);
        public const uint ALG_TYPE_BLOCK = (3 << 9);
        public const uint ALG_SID_3DES = 3;
        public const uint CALG_3DES = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_3DES);


        public IntPtr hKey;
        public IntPtr hProv;

        #endregion

        public WinCrypt()
        {
            hProv = IntPtr.Zero;
            hKey = IntPtr.Zero;
        }
        public void Clear(bool destroid, IntPtr hHash)
        {
            if (destroid)
            {
                CryptDestroyKey(hKey);
                CryptReleaseContext(hProv, 0);
            }
            else
            {
                CryptDestroyKey(hKey);
                CryptDestroyHash(hHash);
            }
        }
        public bool VerifySignature(byte[] RSAKEY,byte[] buff, uint buffLen, byte[] preSig, uint preSigLen)
        {
            bool ret = false;

            if (!CryptCreateHash(hProv, CALG_MD5, IntPtr.Zero, 0, out IntPtr hHash))
            {
                Console.Write("Failed to create hash: {0:D}");
            }
            if (!CryptImportKey(hProv, RSAKEY, RSAKEY.Length, IntPtr.Zero, 0, ref hKey))
            {
                Console.Write("Failed to import key: {0:x}");
            }
            if (!CryptHashData(hHash, buff, (int)buffLen, 0))
            {
                Console.Write("Failed to hash data: {0:D}");
            }

            if (CryptVerifySignature(hHash, preSig, (int)preSigLen, hKey, null, 0))
            {
                ret = true;
            }
            CryptDestroyHash(hHash);
            CryptDestroyKey(hKey);
            return ret;
        }

        public bool DecryptData(byte[] HASHKEY, ref byte[] buff, uint len)
        {
            int ret = 0;

            if (!CryptCreateHash(hProv, CALG_MD5, IntPtr.Zero, 0, out IntPtr hHash))
            {
                Console.Write("Failed to create hash: {0:D}");
            }

            if (!CryptHashData(hHash, HASHKEY, HASHKEY.Length, 0))
            {
                Console.Write("Failed to hash data: {0:D}");
            }

            if (!CryptDeriveKey(hProv, CALG_RC4, hHash, 0, out hKey))
            {
                Console.Write("Failed to derive key: {0:D}");
            }

            if (CryptDecrypt(hKey, IntPtr.Zero, true, 0, buff, ref len))
            {
                ret = 1;
            }
            CryptDestroyHash(hHash);
            CryptDestroyKey(hKey);
            return ret != 0;
        }


        public bool EncryptData(byte[] HASHKEY, ref byte[] buff, uint len)
        {

            int ret = 0;

            if (!CryptCreateHash(hProv, CALG_MD5, IntPtr.Zero, 0, out IntPtr hHash))
            {
                Console.Write("Failed to create hash: {0:D}");
            }

            if (!CryptHashData(hHash, HASHKEY, HASHKEY.Length, 0))
            {
                Console.Write("Failed to hash data: {0:D}");
            }

            if (!CryptDeriveKey(hProv, CALG_RC4, hHash, 0, out hKey))
            {
                Console.Write("Failed to derive key: {0:D}");
            }
            if (CryptDecrypt(hKey, IntPtr.Zero, true, 0, buff, ref len))
            {
                ret = 1;
            }
            Clear(false, hHash);
            return ret != 0;
        }

        public bool SetupCrypt(string Provider = "Microsoft Base Cryptographic Provider v1.0" )
        {
            bool ret = false;
            if (CryptAcquireContext(out hProv, null, Provider, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT))
            {
                ret = true;
            }
            return ret;
        }
       
        public bool CreateSignature(byte[] RSAKEY, ref byte[] data, uint dSize, ref byte[] hash)
        {
            bool ret = false;
            if (!CryptCreateHash(hProv, CALG_MD5, IntPtr.Zero, 0, out IntPtr hHash))
            {
                Console.Write("Failed to create hash: {0:D}");
            }
            if (!CryptImportKey(hProv, RSAKEY, RSAKEY.Length, IntPtr.Zero, 0, ref hKey))
            {
                Console.Write("Failed to import key: {0:x}");
            }
            if (!CryptHashData(hHash, data,(int)dSize - 1, 0))
            {
                Console.Write("Failed to import key: {0:x}");
            }
            int temp = 0x40;
            if (CryptGetHashParam(hHash, 0x0004, data, ref temp, 0))
            {

            }
            byte[] pBuffer = new byte[temp];
            if (CryptGetHashParam(hHash, HP_HASHVAL, pBuffer, ref temp, 0))
            {
                ret = true;
            }
            CryptDestroyHash(hHash);
            CryptDestroyKey(hKey);
            hash = data;
            return ret;
        }

        public void ConcatCharStar(byte[] srcBuff, ref byte[] dstBuff, int len, int offset =0)
        {
            for (int i = offset; i < len; i++)
            {
                dstBuff[i] = srcBuff[i - offset];
            }
        }
    }
}
