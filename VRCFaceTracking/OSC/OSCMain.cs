using System;
using System.Net.Sockets;

namespace VRCFaceTracking.OSC
{
    public static class OSCMain
    {
        public static byte[] EnsureCompliance(this byte[] inputArr)
        {
            var nullTerm = new byte[inputArr.Length + 1];
            Array.Copy(inputArr, nullTerm, inputArr.Length);

            int n = nullTerm.Length + 3;
            int m = n % 4;
            int closestMult = n - m;
            int multDiff = closestMult - nullTerm.Length;
            
            // Construct new array of zeros with the correct length + 1 for null terminator
            byte[] newArr = new byte[nullTerm.Length+multDiff];
            Array.Copy(nullTerm, newArr, nullTerm.Length);
            return newArr;
        }
        
        private static readonly UdpClient UdpClient = new UdpClient();
        private static int Port = 9000;

        public static void SendOscBundle(OscBundle messages) => UdpClient.SendAsync(messages.Data, messages.Data.Length, "127.0.0.1", Port);
    }
}