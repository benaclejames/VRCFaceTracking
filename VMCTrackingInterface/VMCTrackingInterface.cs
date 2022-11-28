using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRCFaceTracking;
using VRCFaceTracking.OSC;
using System.Globalization;

namespace VMCTrackingInterface
{
    public class VMCTrackingInterface : ExtTrackingModule
    {
        private static Socket VMCReciever = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        public override Action GetUpdateThreadFunc()
        {
            return () =>
            {
                Update();
            };
        }

        public override (bool eyeSuccess, bool expressionSuccess) Initialize()
        {
            Logger.Msg("VMC OSC reciever requested.");
            bool success = BindPort(31795);
            Logger.Msg("VMC OSC reciever initialized successfully.");
            return (success, success);
        }

        public override void Teardown()
        {

        }

        public bool BindPort( int inPort )
        {
            bool receiverSuccess = false;
            while (!receiverSuccess)
            {
                try
                {
                    Logger.Msg("VMC OSC: Searching on port " + inPort + ".");

                    VMCReciever.Bind(new IPEndPoint(IPAddress.Any, inPort));
                    VMCReciever.Listen(100);

                    Logger.Msg("VMC OSC: bound to OSC port " + inPort + " successfully.");
                    receiverSuccess = true;
                }
                catch (SocketException)
                {
                    Logger.Warning("VMC OSC: Bind to port " + inPort + " failed. This is usually because the OSC data from the respective VMC Protocol sender is not sending data or is not sending to the right port.");
                }
                catch (Exception e)
                {
                    Logger.Error("VMC OSC: Bind failed for some reason.");
                    Logger.Error(e.ToString());
                }
            }
            return receiverSuccess;
        }

        private void Update()
        {

            byte[] buffer = new byte[8192];

            try
            {
                VMCReciever.Receive(buffer, buffer.Length, SocketFlags.None);
                Logger.Msg("VMC OSC: Message buffered.");
            }
            catch (SocketException e)
            {
                Logger.Error(e.ToString());
                return;
            }
            // var newMsg = new OscMessage(buffer, true);
            foreach (byte b in buffer)
                Logger.Msg("VMC OSC: Byte recieved: " + b);
        }
    }
}
