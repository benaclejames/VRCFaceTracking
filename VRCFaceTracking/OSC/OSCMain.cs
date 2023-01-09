using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using VRCFaceTracking.OSC.Query;

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

    public class OscMain : IDisposable
    {
        private const int FallbackPort = 6969;
        
        private static readonly Socket SenderClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly Socket ReceiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private QueryRegistrar _queryRegistrar;
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
                
                _receiveThread = new Thread(SocketRecvLoop);
            }
            catch (Exception)
            {
                if (!receiverSuccess)
                {
                    _queryRegistrar = new QueryRegistrar();
                    _receiveThread = _queryRegistrar.RegisterOscListener("VRCFT", FallbackPort, ParseRaw);

                    Logger.Msg($"Receiver failed to bind. Using falling back to OSCQuery service discovery on port {FallbackPort}.");
                }
            }
            
            _receiveThread.Start();
            
            var resp = GetAddressValue("/avatar/change");
            if (resp != null && !string.IsNullOrEmpty(resp.VALUE))
                ConfigParser.ParseNewAvatar(resp.VALUE);
            
            return (senderSuccess, receiverSuccess);
        }
        
        private ConfigParser.QueryResponse GetAddressValue(string value)
        {
            // GET request on http://address:inport/avatar/change
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://{MainStandalone.Ip}:{MainStandalone.InPort}{value}");
            request.Method = "GET";
            try
            {
                var resp = (HttpWebResponse)request.GetResponse();

                // Ensure we have OK response
                if (resp.StatusCode != HttpStatusCode.OK)
                    return null;

                // Parse response json
                var stream = resp.GetResponseStream();
                var reader = new System.IO.StreamReader(stream);
                var json = reader.ReadToEnd();

                // Parse json to the QueryResponse class
                return JsonSerializer.Deserialize<ConfigParser.QueryResponse>(json);
            }
            catch (WebException e)
            {
                // If the web request cannot connect to the remote server, remind the user to make sure VRChat is running
                if (e.Status == WebExceptionStatus.ConnectFailure)
                    Logger.Warning("Could not connect to VRChat. Make sure VRChat is running and the IP address is correct.");
            }
            
            return null;
        }

        private void SocketRecvLoop()
        {
            while (!MainStandalone.MasterCancellationTokenSource.IsCancellationRequested)
            {
                byte[] buffer = new byte[2048];
                try
                {
                    ReceiverClient.Receive(buffer, buffer.Length, SocketFlags.None);
                }
                catch (SocketException)
                {
                    // Ignore as this is most likely a timeout exception
                    continue;
                }
                
                ParseRaw(buffer);
            }
        }

        private static void ParseRaw(byte[] rawBytes)
        {
            try {
                var newMsg = new OscMessage(rawBytes);
                if (newMsg.Address == "/avatar/change")
                    ConfigParser.ParseNewAvatar((string) newMsg.Value);
            }
            catch (Exception e)
            {
                // vrchat sending cringe packets smh
            }
        }

        public void Send(byte[] data)
        {
            if (SenderClient.Connected)
                SenderClient.Send(data, data.Length, SocketFlags.None);
        }

        public void Dispose()
        {
            _receiveThread.Abort();
            
            SenderClient.Dispose();
            ReceiverClient.Dispose();
            _queryRegistrar?.Dispose();
        }
    }
}