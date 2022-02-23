using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCFaceTracking.OSC
{
    public class OscBundle
    {
        public readonly byte[] Data;

        public OscBundle(IEnumerable<OscMessage> messages)
        {
            int size = messages.Sum(param => param.Data.Length + 4);
            Data = new byte[16+size]; // Include #bundle header and null terminator
            Array.Copy(new byte[] {35, 98, 117, 110, 100, 108, 101, 0}, Data, 8);
            // Get the NTP time with picoseconds
            Int64 time = (Int64) (DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds * 1000;
            var timeBytes = BitConverter.GetBytes(time);
            Array.Reverse(timeBytes);
            Array.Copy(timeBytes, 0, Data, 8, 8);

            // Now add bundle data
            int ix = 16;
            foreach (var message in messages)
            {
                var length = BitConverter.GetBytes(message.Data.Length);
                Array.Reverse(length);
                Array.Copy(length, 0, Data, ix, 4);
                ix += 4;

                Array.Copy(message.Data, 0, Data, ix, message.Data.Length);
                ix += message.Data.Length;
            }
        }
    }
}