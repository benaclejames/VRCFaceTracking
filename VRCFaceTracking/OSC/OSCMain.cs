using System.Collections.Generic;
using System.Net.Sockets;
using OSCTest;

namespace VRCFaceTracking.OSC
{
    public static class OSCMain
    {
        public static void EnsureCompliance(this List<byte> inputArr)
        {
            int n = inputArr.Count + 3;
            int m = n % 4;
            int closestMult = n - m;
            int multDiff = closestMult - inputArr.Count;
            
            // Construct new array of zeros with the correct length
            byte[] newArr = new byte[multDiff];
            inputArr.AddRange(newArr);
        }
        
        private static readonly UdpClient _udpClient = new UdpClient();
        public static int Port = 9000;

        public static void SendOSCBundle(OscBundle messages)
        {
            _udpClient.Send(messages.Data, messages.Data.Length, "127.0.0.1", 9000);
        }
    }
}