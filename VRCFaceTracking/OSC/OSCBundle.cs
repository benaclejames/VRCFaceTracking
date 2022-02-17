using System;
    using System.Collections.Generic;
using VRCFaceTracking.OSC;

namespace OSCTest
{
    public class OscBundle
    {
        public readonly byte[] Data;

        public OscBundle(OscMessage[] messages)
        {
            var dataConstruct = new List<byte>
                {35, 98, 117, 110, 100, 108, 101, 0}; // Include #bundle header and null terminator
            // Get the NTP time with picoseconds
            Int64 time = (Int64) (DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds * 1000;
            var timeBytes = BitConverter.GetBytes(time);
            Array.Reverse(timeBytes);
            dataConstruct.AddRange(timeBytes);

            // Now add bundle data
            foreach (var message in messages)
            {
                var length = BitConverter.GetBytes(message.Data.Count);
                Array.Reverse(length);
                dataConstruct.AddRange(length);

                dataConstruct.AddRange(message.Data);
            }

            Data = dataConstruct.ToArray();
        }
    }
}