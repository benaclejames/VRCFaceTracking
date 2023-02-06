using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace VRCFaceTracking.OSC
{
    public static class OSCUtils
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
            byte[] newArr = new byte[nullTerm.Length + multDiff];
            Array.Copy(nullTerm, newArr, nullTerm.Length);
            return newArr;
        }
    }

    public class OscMain
    {
        private static readonly Socket SenderClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly Socket ReceiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static Thread _receiveThread;

        public (bool senderSuccess, bool receiverSuccess) Bind(string address, int outPort, int inPort)
        {
            (bool senderSuccess, bool receiverSuccess) = (false, false);
            try
            {
                SenderClient.Connect(new IPEndPoint(IPAddress.Parse(address), outPort));
                senderSuccess = true;
                ReceiverClient.Bind(new IPEndPoint(IPAddress.Parse(address), inPort));
                receiverSuccess = true;
                ReceiverClient.ReceiveTimeout = 1000;
                
                _receiveThread = new Thread(() =>
                {
                    while (!MainStandalone.MasterCancellationTokenSource.IsCancellationRequested)
                        Recv();
                });
                _receiveThread.Start();
            }
            catch (Exception)
            {
                return (senderSuccess, receiverSuccess);
            }
            return (true, true);
        }

        private void Recv()
        {
            byte[] buffer = new byte[2048];
            try
            {
                ReceiverClient.Receive(buffer, buffer.Length, SocketFlags.None);
            }
            catch (SocketException)
            {
                // Ignore as this is most likely a timeout exception
                return;
            }
            var newMsg = new OscMessage(buffer);
            switch (newMsg.Address)
            {
                case "/avatar/change":
                    ConfigParser.ParseNewAvatar((string)newMsg.Value);
                    break;
                case "/avatar/parameters/EyeTrackingActive":
                    if (UnifiedLibManager.EyeStatus != ModuleState.Uninitialized)
                    {
                        if (!(bool)newMsg.Value)
                            UnifiedLibManager.EyeStatus = ModuleState.Idle;
                        else UnifiedLibManager.EyeStatus = ModuleState.Active;
                    }
                    break;
                case "/avatar/parameters/LipTrackingActive":
                    if (UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized)
                    {
                        if (!(bool)newMsg.Value)
                            UnifiedLibManager.ExpressionStatus = ModuleState.Idle;
                        else UnifiedLibManager.ExpressionStatus = ModuleState.Active;
                    }
                    break;
                case "/avatar/parameters/ExpressionTrackingActive":
                    if (UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized)
                    {
                        if (!(bool)newMsg.Value)
                            UnifiedLibManager.ExpressionStatus = ModuleState.Idle;
                        else UnifiedLibManager.ExpressionStatus = ModuleState.Active;
                    }
                    break;
            }
        }

        public void Send(byte[] data) => SenderClient.Send(data, data.Length, SocketFlags.None);
    }
}