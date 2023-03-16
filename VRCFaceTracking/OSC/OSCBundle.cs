using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCFaceTracking.OSC
{
    public class OscBundle
    {
        public readonly byte[] Data;

        public OscBundle(OscMessage<object>[] messages)
        {
            int messageCount = messages.Length;
            byte[][] messageData = new byte[][messageCount];
            // Fill the second array with the message data from each message
            int i = 0;
            int combinedSize = 0;
            foreach (var message in messages)
            { 
                byte[] oscData = message.Serialize();
                messageData[i] = oscData;
                combinedSize += oscData.Length;
                i++;
            }
            
            Data = new byte[16+combinedSize]; // Include #bundle header and null terminator
            Array.Copy(new byte[] {35, 98, 117, 110, 100, 108, 101, 0}, Data, 8);
            
            // Get the NTP time with picoseconds
            Int64 time = (Int64) (DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds * 1000;
            var timeBytes = BitConverter.GetBytes(time);
            if (BitConverter.IsLittleEndian) 
                Array.Reverse(timeBytes);
            Array.Copy(timeBytes, 0, Data, 8, 8);

            // Now add bundle data
            int ix = 16;
            foreach (var message in messageData)
            {
                var length = BitConverter.GetBytes(message.Length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(length);
                Array.Copy(length, 0, Data, ix, 4);
                ix += 4;

                Array.Copy(message, 0, Data, ix, message.Length);
                ix += message.Length;
            }
        }
    }
}