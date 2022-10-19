using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MeaMod.DNS.Multicast;

namespace VRCFaceTracking.OSC.Query
{
    public class QueryRegistrar : IDisposable
    {
        private static readonly List<UdpClient> UdpClient = new List<UdpClient>();
        private static readonly List<ServiceProfile> ServiceProfiles = new List<ServiceProfile>();
        private static IPEndPoint _sender = new IPEndPoint(IPAddress.Any, 0);
        private readonly ServiceDiscovery _serviceDiscovery;
        private readonly MulticastService _multicastService;
        
        // Register with ZeroConf to advertise the service
        public QueryRegistrar()
        {
            _multicastService = new MulticastService();
            _multicastService.UseIpv6 = false;
            _multicastService.IgnoreDuplicateMessages = true;
            _serviceDiscovery = new ServiceDiscovery(_multicastService);
            _multicastService.Start();
        }

        public Thread RegisterOscListener(string instanceName, ushort port, Action<byte[]> callback)
        {
            var newClient = new UdpClient(port);
            UdpClient.Add(newClient);
            var listenerThread = new Thread(() =>
            {
                while (!MainStandalone.MasterCancellationTokenSource.IsCancellationRequested)
                {
                    var data = newClient.Receive(ref _sender);
                    callback.Invoke(data);
                }
            });

            var profile = new ServiceProfile(instanceName, "_osc._udp", port, new[] { IPAddress.Loopback });
            ServiceProfiles.Add(profile);
            _serviceDiscovery.Advertise(profile);
            return listenerThread;
        }
        
        public void Dispose()
        {
            _serviceDiscovery.Dispose();
            
            foreach (var client in UdpClient)
                client.Close();
        }
    }
}