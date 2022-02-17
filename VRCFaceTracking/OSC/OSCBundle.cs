using System;
using System.Collections.Generic;

namespace VRCFaceTracking.OSC
{
    public class OscBundle
    {
        public readonly byte[] Data;

        public OscBundle(IEnumerable<OscMessage> messages)
        {
            Data = new byte[16]; // Include #bundle header and null terminator
            Array.Copy(new byte[] {35, 98, 117, 110, 100, 108, 101, 0}, Data, 8);
            // Get the NTP time with picoseconds
            Int64 time = (Int64) (DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds * 1000;
            var timeBytes = BitConverter.GetBytes(time);
            Array.Reverse(timeBytes);
            Array.Copy(timeBytes, 0, Data, 8, 8);

            // Now add bundle data
            foreach (var message in messages)
            {
                var newElementArray = new byte[Data.Length + message.Data.Length + 4];
                Array.Copy(Data, newElementArray, Data.Length);
                var length = BitConverter.GetBytes(message.Data.Length);
                Array.Reverse(length);
                Array.Copy(length, 0, newElementArray, Data.Length, 4);

                Array.Copy(message.Data, 0, newElementArray, Data.Length+4, message.Data.Length);

                Data = newElementArray;
            }
        }
    }
}