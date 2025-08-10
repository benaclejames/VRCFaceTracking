using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.OSC.Query.mDNS;

namespace VRCFaceTracking.Core.mDNS;

public partial class MulticastDnsService : ObservableObject
{
    private static readonly IPAddress MulticastIp = IPAddress.Parse("224.0.0.251");
    private const int MulticastPost = 5353;
    private static readonly IPEndPoint MdnsEndpointIp4 = new(MulticastIp, MulticastPost);
    private readonly List<IPAddress> _localIpAddresses;
    private readonly ILogger<MulticastDnsService> _logger;

    private static readonly Dictionary<IPAddress, UdpClient> Senders = new();
    private static readonly Dictionary<UdpClient, CancellationToken> Receivers = new();
    private static readonly Dictionary<string, AdvertisedService> Services = new();

    public Action OnVrcClientDiscovered = () => { };

    [ObservableProperty] private IPEndPoint _vrchatClientEndpoint;

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

    public MulticastDnsService(ILogger<MulticastDnsService> logger)
    {
        _logger = logger;

        _localIpAddresses = GetIpv4NetInterfaces().SelectMany(GetIpv4Addresses).Where(addr => addr.AddressFamily == AddressFamily.InterNetwork).ToList();

        // Create listeners for all interfaces
        var cts = new CancellationTokenSource();
        var receiver = new UdpClient(AddressFamily.InterNetwork);
        receiver.Client.ExclusiveAddressUse = false;
        receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        receiver.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastPost));
        Receivers.Add(receiver, cts.Token);


        // For every ipv4 address discovered in the network interfaces, create a sender udp client set up grouping
        foreach (var ipAddress in _localIpAddresses)
        {
            // Add the local ip address to our multicast group
            receiver.JoinMulticastGroup(MulticastIp, ipAddress);

            var sender = new UdpClient(ipAddress.AddressFamily);
            sender.Client.ExclusiveAddressUse = false;
            sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sender.Client.Bind(new IPEndPoint(ipAddress, MulticastPost));    // Bind to the local ip address
            sender.JoinMulticastGroup(MulticastIp, ipAddress);                           // Join the multicast group
            sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            Receivers.Add(sender, cts.Token);
            Senders.Add(ipAddress, sender);
        }

        foreach (var sender in Receivers)
        {
            Listen(sender.Key, sender.Value);
        }
    }

    private async void ResolveDnsQueries(DnsPacket packet, IPEndPoint remoteEndpoint)
    {
        // Me when having to wait for vrchat to resolve itself
        // https://www.youtube.com/watch?v=wLg04uu2j2o
        if (packet.OPCODE != 0 || VrchatClientEndpoint == null)
        {
            return;
        }

        foreach (var question in packet.questions)
        {
            // Ensure the question has three labels. First for service name, second for protocol, third for domain
            // Ensure the question is for local domain
            // Ensure the question is for the _osc._udp service
            if (question.Labels.Count != 3
                || question.Labels[2] != "local"
                || !Services.TryGetValue($"{question.Labels[0]}.{question.Labels[1]}", out var service))
            {
                continue;
            }

            //foreach (var service in storedServices)
            {
                var qualifiedServiceName = new List<string>
                {
                    service.ServiceName,
                    question.Labels[0],
                    question.Labels[1],
                    question.Labels[2]
                };

                var serviceName = new List<string>
                {
                    service.ServiceName,
                    question.Labels[0].Trim('_'),
                    question.Labels[1].Trim('_')
                };

                var txt = new TXTRecord { Text = new List<string> { "txtvers=1" } };
                var srv = new SRVRecord {
                    Port = (ushort)service.Port,
                    Target = serviceName
                };
                var aRecord = new ARecord { Address = service.Address };
                var ptrRecord = new PTRRecord
                {
                    DomainLabels = qualifiedServiceName
                };

                var additionalRecords = new List<DnsResource>
                {
                    new (txt, qualifiedServiceName),
                    new (srv, qualifiedServiceName),
                    new (aRecord, serviceName)
                };

                var answers = new List<DnsResource> { new (ptrRecord, question.Labels) };

                var response = new DnsPacket
                {
                    CONFLICT = true,
                    ID = 0,
                    OPCODE = 0,
                    QUERYRESPONSE = true,
                    RESPONSECODE = 0,
                    TENTATIVE = false,
                    TRUNCATION = false,
                    questions = Array.Empty<DnsQuestion>(),
                    answers = answers.ToArray(),
                    authorities = Array.Empty<DnsResource>(),
                    additionals = additionalRecords.ToArray()
                };

                var bytes = response.Serialize();

                if (remoteEndpoint.Port == MulticastPost)
                {
                    foreach (var sender in Senders)
                    {
                        await sender.Value.SendAsync(bytes, bytes.Length, MdnsEndpointIp4);
                    }

                    continue;
                }

                //var unicastClientIp4 = new UdpClient(AddressFamily.InterNetwork);
                //await unicastClientIp4.SendAsync(bytes, bytes.Length, remoteEndpoint);
            }
        }
    }

    public async void SendQuery(string labels)
    {
        var dnsPacket = new DnsPacket();
        var dnsQuestion = new DnsQuestion(labels.Split('.').ToList(), 255, 1);
        dnsPacket.questions = new[] { dnsQuestion };
        dnsPacket.QUERYRESPONSE = false;
        dnsPacket.OPCODE = 0;
        dnsPacket.TRUNCATION = false;

        var bytes = dnsPacket.Serialize();
        foreach (var sender in Senders)
        {
            await sender.Value.SendAsync(bytes, bytes.Length, MdnsEndpointIp4);
        }

        //var unicastClientIp4 = new UdpClient(AddressFamily.InterNetwork);
        //await unicastClientIp4.SendAsync(bytes, bytes.Length, remoteEndpoint);
    }

    public void ResolveVrChatClient(DnsPacket packet, IPEndPoint remoteEndpoint)
    {
        if (!packet.QUERYRESPONSE || packet.answers.Length <= 0 || packet.answers[0].Type != 12)
        {
            return;
        }

        var ptrRecord = packet.answers[0].Data as PTRRecord;
        if (ptrRecord.DomainLabels.Count != 4 || !ptrRecord.DomainLabels[0].StartsWith("VRChat-Client"))
        {
            return;
        }

        if (packet.answers[0].Labels.Count != 3 || packet.answers[0].Labels[0] != "_oscjson" ||
            packet.answers[0].Labels[1] != "_tcp" || packet.answers[0].Labels[2] != "local")
        {
            return;
        }

        // Now we find the first A record in the additional records
        var aRecord = packet.additionals.FirstOrDefault(r => r.Type == 1);
        var srvRecord = packet.additionals.FirstOrDefault(r => r.Type == 33);
        if (aRecord == null || srvRecord == null)
        {
            return;
        }

        var vrChatClientIp = aRecord.Data as ARecord;
        var vrChatClientPort = srvRecord.Data as SRVRecord;

        if (vrChatClientIp?.Address == null || vrChatClientPort?.Port == null)
        {
            return;
        }

        var hostAddress = vrChatClientIp.Address;

        // If the host address is a loopback address (127.0.0.1) but isn't our local machine,
        // manually set the correspondence address to where we heard the mdns response come from
        // This is due to VRC ALWAYS using 127.0.0.1 as the A record address
        if (IPAddress.IsLoopback(hostAddress) && !_localIpAddresses.Contains(remoteEndpoint.Address))
        {
            hostAddress = remoteEndpoint.Address;
        }

        VrchatClientEndpoint = new IPEndPoint(hostAddress, vrChatClientPort.Port);
        OnVrcClientDiscovered();
    }

    private async void Listen(UdpClient client, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await client.ReceiveAsync(ct);

            if (!_localIpAddresses.Any(i => i.Equals(result.RemoteEndPoint.Address)))
            {
                // Wouldn't want to steal control of other people's faces on LAN now, would we!
                // (On second thought, that'd be pretty funny)
                continue;
            }

            try
            {
                var reader = new BigReader(result.Buffer);
                var packet = new DnsPacket(reader);

                // I'm aware this is cringe, but we do this first as it's a lot more likely vrchat beats us to the punch responding to the query
                ResolveVrChatClient(packet, result.RemoteEndPoint);
                ResolveDnsQueries(packet, result.RemoteEndPoint);
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e, scope => scope.SetExtra("bytes", result.Buffer));
            }
        }
    }

    public void Advertise(string serviceName, AdvertisedService advertisement)
    {
        _logger.LogDebug($"Advertising service {serviceName} with advertisement {advertisement}");

        Services[serviceName] = advertisement;
    }
}
