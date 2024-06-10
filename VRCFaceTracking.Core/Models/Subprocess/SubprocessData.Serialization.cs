using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Models;
public partial class SubprocessData
{
    public byte[] Encode()
    {
        List<byte> dataEncoded = new List<byte>();

        // Serialiser for UnifiedExpressions

        return dataEncoded.ToArray();
    }

    public void Decode(byte[] data)
    {
        // Deserialiser for UnifiedExpressions
        MemoryMarshal.Cast<byte[], SubprocessData_Protocol>
    }
}
