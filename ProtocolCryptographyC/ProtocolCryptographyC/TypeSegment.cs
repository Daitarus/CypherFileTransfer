using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolCryptographyC
{
    public enum TypeSegment : byte
    {
        UNKNOW = 0,
        ASK_GET_PKEY = 1,
        PKEY = 2,
        AUTHORIZATION = 3,
        ASK_GET_FILE = 4,
        FILE = 5
    }
}
