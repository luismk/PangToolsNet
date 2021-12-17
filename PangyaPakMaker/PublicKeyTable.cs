using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaPakMaker
{
    public class PublicKeyTable
    {
        public static ushort[] LZ77_MASKS = { 65313, 33615, 26463, 52, 62007, 33119, 18277, 563 };

        // 16 ROUND
        public static readonly uint[] XTEA_US_KEY = { 66455465, 57629246, 17826484, 78315754 }; // 0x3F607A9, 0x36F5A3E, 0x11002B4, 0x4AB00EA
        public static readonly uint[] XTEA_JP_KEY = { 34234324, 32423423, 45336224, 83272673 }; // 0x20A5FD4, 0x1EEBDFF, 0x2B3C6A0, 0x4F6A3E1
        public static readonly uint[] XTEA_TH_KEY = { 84595515, 12254985, 72548314, 46875682 }; // 0x50AD33B, 0x0BAFF09, 0x452FFDA, 0x2CB4422
        public static readonly uint[] XTEA_EU_KEY = { 32081624, 92374137, 64139451, 46772272 }; // 0x1E986D8, 0x5818479, 0x3D2B0BB, 0x2C9B030
        public static readonly uint[] XTEA_ID_KEY = { 23334327, 21322395, 41884343, 93424468 }; // 0x1640DB7, 0x1455A9B, 0x27F1AB7, 0x5918B54
        public static readonly uint[] XTEA_KR_KEY = { 75871606, 85233154, 85204374, 42969558 }; // 0x485B576, 0x5148E02, 0x5141D96, 0x28FA9D6

        public static readonly uint[][] XTEA_ALL_KEY = 
            { 
            new uint[] { 66455465, 57629246, 17826484, 78315754 },
            new uint[]  { 34234324, 32423423, 45336224, 83272673 },
            new uint[] { 84595515, 12254985, 72548314, 46875682 },
            new uint[] { 32081624, 92374137, 64139451, 46772272 },
            new uint[] { 23334327, 21322395, 41884343, 93424468 },
            new uint[] { 75871606, 85233154, 85204374, 42969558} 
        };
    }
}
