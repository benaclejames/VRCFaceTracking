using System;
using System.Text.RegularExpressions;

namespace VRCFaceTracking
{
    public static class ArgsHandler
    {
        public static (int SendPort, string IP, int RecievePort) HandleArgs()
        {
            (int SendPort, string IP, int RecievePort) = (9000, "127.0.0.1", 9001);
            
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("--osc="))
                {
                    var oscConfig = arg.Remove(0, 6).Split(':');
                    if (oscConfig.Length < 3)
                    {
                        Console.WriteLine("Invalid OSC config: " + arg +"\nExpected format: --osc=<OutPort>:<IP>:<InPort>");
                        break;
                    }

                    if (!int.TryParse(oscConfig[0], out SendPort))
                    {
                        Console.WriteLine("Invalid OSC OutPort: " + oscConfig[0]);
                        break;
                    }
                    
                    if (!new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$").IsMatch(oscConfig[1]))
                    {
                        Console.WriteLine("Invalid OSC IP: " + oscConfig[1]);
                        break;
                    } 
                    IP = oscConfig[1];
                    
                    if (!int.TryParse(oscConfig[2], out RecievePort))
                    {
                        Console.WriteLine("Invalid OSC InPort: " + oscConfig[2]);
                        break;
                    }
                }
            }

            return (SendPort, IP, RecievePort);
        }
    }
}