using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;
using VRCFaceTracking;

namespace SocketsTrackingInterface
{
    public class TCPTrackingInterface : ExtTrackingModule
    {
        public int PORT = 13191;

        private const int expressionsSize = 63;
        private byte[] rawExpressions = new byte[expressionsSize * 4 + (8 * 2 * 4)];
        private float[] expressions = new float[expressionsSize + (8 * 2)];

        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize()
        {
            string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "questProIP.txt");
            if (!File.Exists(configPath))
            {
                Logger.Msg("Failed to find config JSON! Please maker sure it is present in the same directory as the DLL.");
                return (false, false);
            }

            string text = File.ReadAllText(configPath).Trim();

            Logger.Msg("ALXR handshake successful! Data will be broadcast to VRCFaceTracking.");
            return (true, true);
        }

        public override void Teardown()
        {
            throw new NotImplementedException();
        }

        public override Action GetUpdateThreadFunc()
        {
            throw new NotImplementedException();
        }
    }
}
