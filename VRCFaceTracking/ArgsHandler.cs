using System;
using System.Text.RegularExpressions;

namespace VRCFaceTracking
{
    public static class ArgsHandler
    {
        
        public static (int SendPort, string IP, int ReceivePort, string opMode) HandleArgs()
        {
            Logger.Msg("Loading Defaults");
            (int SendPort, string IP, int ReceivePort, string opMode) = (9000, "127.0.0.1", 9001, "auto");

            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if ((arg.StartsWith("--help")) || (arg.StartsWith("--options")))
                {
                   Logger.Msg("To set custom OSC port/IP config please use the following");
                   Logger.Msg("--osc=<OutPort>:<IP>:<InPort>");
                   Logger.Msg("To set operating mode please use the following");
                   Logger.Msg("--mode=vrc or --mode=cvr");
                }
                if (arg.StartsWith("--mode="))
                {
                  opMode = arg.Remove(0, 7);
                }
                if (arg.StartsWith("--osc="))
                {
                    Logger.Msg("Loading OSC config Args");
                    var oscConfig = arg.Remove(0, 6).Split(':');
                    if (oscConfig.Length < 3)
                    {
                        Logger.Error("Invalid OSC config: " + arg +"\nExpected format: --osc=<OutPort>:<IP>:<InPort>");
                        break;
                    }

                    
                    int parsedIntSP;
                    if (int.TryParse(oscConfig[0], out parsedIntSP))
                    {
                        SendPort = parsedIntSP;
                        Logger.Msg("Loaded custom OSC OutPort value, " + SendPort);
                    }
                    else
                    {
                        Logger.Error("Malformed OSC OutPort value" + oscConfig[0] + ", please ensure you set a number");
                    }
        

                    if (!new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$").IsMatch(oscConfig[1]))
                    {
                        Logger.Error("Invalid OSC IP: " + oscConfig[1]);
                        break;
                    }
                    else
                    {
                        IP = oscConfig[1];
                    }
                    
                    int parsedIntRP;
                    if (int.TryParse(oscConfig[2], out parsedIntRP))
                    {
                        ReceivePort = parsedIntRP;
                        Logger.Msg("Loaded custom OSC ReceivePort value, " + ReceivePort);
                    }
                    else
                    {
                        Logger.Error("Malformed OSC ReceivePort value " + oscConfig[2] + ", please ensure you set a number");
                    }
            
                }
            }

            return (SendPort, IP, ReceivePort, opMode);
        }
    }
}