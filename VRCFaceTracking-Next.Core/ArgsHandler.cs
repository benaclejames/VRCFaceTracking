using System;
using System.Text.RegularExpressions;

namespace VRCFaceTracking
{
    public static class ArgsHandler
    {
        public static (int SendPort, string IP, int RecievePort, bool EnableEye, bool EnableExpression) HandleArgs()
        {
            (int SendPort, string IP, int RecievePort) = (9000, "127.0.0.1", 9001);
            (bool EnableEye, bool EnableExpression) = (true, true);
            
            

            return (SendPort, IP, RecievePort, EnableEye, EnableExpression);
        }
    }
}