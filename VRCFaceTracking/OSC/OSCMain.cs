using System;
using System.Net;
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
        
        private static readonly Socket UdpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static int Port = 9000;

        static OSCMain() => UdpClient.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port));
        public static void Send(byte[] data) => UdpClient.Send(data, data.Length, SocketFlags.None);
    }
}