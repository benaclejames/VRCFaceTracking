using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class QueryRegistrar
{
    private static readonly IPAddress MulticastIp = IPAddress.Parse("224.0.0.251");
    private static readonly IPEndPoint MdnsEndpointIp4 = new(MulticastIp, 5353);

    private static readonly Dictionary<IPAddress, UdpClient> Senders = new();
    private static readonly List<UdpClient> Receivers = new();

    public static Action OnVRCClientDiscovered = () => { };

    private static IPEndPoint _vrchatClientEndpoint;
    public static IPEndPoint VrchatClientEndpoint
    {
        get => _vrchatClientEndpoint;
        set
        {
            _vrchatClientEndpoint = value;
            OnVRCClientDiscovered.Invoke();
        }
    }
        
    private static List<NetworkInterface> GetIpv4NetInterfaces() => NetworkInterface.GetAllNetworkInterfaces()
        .Where(net =>
            net.OperationalStatus == OperationalStatus.Up &&
            net.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .ToList();

    // Get all ipv4 addresses from a specific network interface
    private static IEnumerable<IPAddress> GetIpv4Addresses(NetworkInterface net) => net.GetIPProperties()
        .UnicastAddresses
        .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
        .Select(addr => addr.Address);

    private static readonly Dictionary<string, (string serviceName, int port, string ipAddress)> services = new();
        
    public QueryRegistrar()
    {
        // Create listeners for all interfaces
        UdpClient receiver = new UdpClient(AddressFamily.InterNetwork);
        receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        receiver.Client.Bind(new IPEndPoint(IPAddress.Any, 5353));
        Receivers.Add(receiver);

        // For each ip address, create a sender udpclient to respond to multicast requests
        var interfaces = GetIpv4NetInterfaces();
        var ipAddresses = interfaces.SelectMany(GetIpv4Addresses)
            .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork);
            
        // For every ipv4 address discovered in the network interfaces, create a sender udpclient set up grouping
        foreach (IPAddress ipAddress in ipAddresses)
        {
            UdpClient sender = new UdpClient(ipAddress.AddressFamily);
                
            // Add the local ip address to our multicast group
            receiver.JoinMulticastGroup(MulticastIp, ipAddress);
                
            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sender.Client.Bind(new IPEndPoint(ipAddress, 5353));    // Bind to the local ip address
            sender.JoinMulticastGroup(MulticastIp);                           // Join the multicast group
            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
                
            Receivers.Add(sender);
        }

        var addresses = interfaces
            .SelectMany(GetIpv4Addresses)
            .Where(a => a.AddressFamily == AddressFamily.InterNetwork);

        foreach (var address in addresses)
        {
            if (Senders.Keys.Contains(address))
            {
                continue;
            }

            var localEndpoint = new IPEndPoint(address, 5353);
            var sender = new UdpClient(address.AddressFamily);
            try
            {
                //receiver.JoinMulticastGroup(MulticastIP, address);
                sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                sender.Client.Bind(localEndpoint);
                sender.JoinMulticastGroup(MulticastIp);
                sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

                Receivers.Add(sender);

                Senders.Add(address, sender);
            }
            catch (Exception e)
            {
                sender.Dispose();
            }
        }

        foreach (var sender in Receivers)
        {
            Listen(sender);
        }
    }

    private async void ResolveDnsQueries(DNSPacket packet, IPEndPoint remoteEndpoint)
    {
        if (packet.OPCODE != 0) 
            return;

        foreach (var question in packet.questions)
        {
            // Ensure the question has three labels. First for service name, second for protocol, third for domain
            if (question.Labels.Count != 3)
                continue;
                        
            // Ensure the question is for local domain
            if (question.Labels[2] != "local")
                continue;
                        
            // Ensure the question is for the _osc._udp service
            if (!services.TryGetValue($"{question.Labels[0]}.{question.Labels[1]}", out var service))
                continue;

            //foreach (var service in storedServices)
            {
                var qualifiedServiceName = new List<string>
                {
                    service.serviceName,
                    question.Labels[0],
                    question.Labels[1],
                    question.Labels[2]
                };

                var serviceName = new List<string>
                {
                    service.serviceName,
                    question.Labels[0].Trim('_'),
                    question.Labels[1].Trim('_')
                };
                            
                var txt = new TXTRecord { Text = new List<string> { "txtvers=1" } };
                var srv = new SRVRecord { 
                    Port = (ushort)service.port, 
                    Target = serviceName
                };
                var aRecord = new ARecord { Address = IPAddress.Parse(service.ipAddress) };
                var ptrRecord = new PTRRecord
                {
                    DomainLabels = qualifiedServiceName
                };
                            
                var additionalRecords = new List<DNSResource>
                {
                    new DNSResource(txt, qualifiedServiceName),
                    new DNSResource(srv, qualifiedServiceName),
                    new DNSResource(aRecord, serviceName)
                };

                var answers = new List<DNSResource> { new DNSResource(ptrRecord, question.Labels) };

                var response = new DNSPacket
                {
                    CONFLICT = true,
                    ID = 0,
                    OPCODE = 0,
                    QUERYRESPONSE = true,
                    RESPONSECODE = 0,
                    TENTATIVE = false,
                    TRUNCATION = false,
                    questions = Array.Empty<DNSQuestion>(),
                    answers = answers.ToArray(),
                    authorities = Array.Empty<DNSResource>(),
                    additionals = additionalRecords.ToArray()
                };

                var bytes = response.Serialize();

                if (remoteEndpoint.Port == 5353)
                {
                    foreach (var sender in Senders)
                    {
                        await sender.Value.SendAsync(bytes, bytes.Length, MdnsEndpointIp4);
                    }

                    continue;
                }

                UdpClient unicastClientIp4 = new UdpClient(AddressFamily.InterNetwork);
                await unicastClientIp4.SendAsync(bytes, bytes.Length, remoteEndpoint);
            }
        }
    }

    private async void ResolveVRChatClient(DNSPacket packet, IPEndPoint remoteEndpoint)
    {
        if (!packet.QUERYRESPONSE || packet.answers[0].Type != 12)
            return;

        var ptrRecord = packet.answers[0].Data as PTRRecord;
        if (ptrRecord.DomainLabels.Count != 4 || !ptrRecord.DomainLabels[0].StartsWith("VRChat-Client"))
            return;
            
        if (packet.answers[0].Labels.Count != 3 || packet.answers[0].Labels[0] != "_oscjson" ||
            packet.answers[0].Labels[1] != "_tcp" || packet.answers[0].Labels[2] != "local")
            return;
            
        // Now we find the first arecord in the additional records
        var aRecord = packet.additionals.FirstOrDefault(r => r.Type == 1);
        var srvRecord = packet.additionals.FirstOrDefault(r => r.Type == 33);
        if (aRecord == null || srvRecord == null)
            return;
            
        var vrchatClientIp = aRecord.Data as ARecord;
        var vrchatClientPort = srvRecord.Data as SRVRecord;
            
        VrchatClientEndpoint = new IPEndPoint(vrchatClientIp.Address, vrchatClientPort.Port);
    }
        
    private void Listen(UdpClient client)
    {
        new Thread(async () =>
        {
            while (true)
            {
                var result = await client.ReceiveAsync();
                var reader = new BigReader(result.Buffer);
                var packet = new DNSPacket(reader);

                // I'm aware this is cringe, but we do this first as it's a lot more likely vrchat beats us to the punch responding to the query
                ResolveVRChatClient(packet, result.RemoteEndPoint);
                ResolveDnsQueries(packet, result.RemoteEndPoint);
            }
        }).Start();
    }

    public void Advertise(string instanceName, string serviceName, int port, IPAddress address)
    {
        if (services.TryGetValue(serviceName, out var service))
        {
            service.serviceName = serviceName;
            service.ipAddress = address.ToString();
            service.port = port;
        }
        else
        {
            services.Add(serviceName, (instanceName, port, address.ToString()));
        }
    }
}