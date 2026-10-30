using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using FastOSC;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Core.Services;

/**
 * OscSendService is responsible for encoding osc messages and sending them over OSC
 */
public class OscSendService
{
    private readonly ILogger<OscSendService> _logger;
    private readonly IOscTarget _oscTarget;

    private Socket _sendSocket;
    private readonly byte[] _sendBuffer = new byte[4096];

    private const int BundleHeaderLength = 16; // "#bundle\0" + time tag
    private const ulong ImmediateTimeTag = 1;

    private CancellationTokenSource _cts;
    public Action<int> OnMessagesDispatched = _ => { };

    public OscSendService(
        ILogger<OscSendService> logger,
        IOscTarget oscTarget
    )
    {
        _logger = logger;
        _cts = new CancellationTokenSource();

        _oscTarget = oscTarget;

        _oscTarget.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not nameof(IOscTarget.OutPort))
            {
                return;
            }

            if (_oscTarget.OutPort == default)
            {
                return;
            }

            if (string.IsNullOrEmpty(_oscTarget.DestinationAddress) || !IPAddress.TryParse(_oscTarget.DestinationAddress, out var ipAddress))
            {
                _oscTarget.DestinationAddress = "127.0.0.1";
                IPAddress.TryParse(_oscTarget.DestinationAddress, out ipAddress);
            }

            UpdateTarget(new IPEndPoint(ipAddress!, _oscTarget.OutPort));
        };
    }

    private void UpdateTarget(IPEndPoint endpoint)
    {
        _cts.Cancel();
        _sendSocket?.Close();
        _oscTarget.IsConnected = false;

        _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _sendSocket.Connect(endpoint);
            _oscTarget.IsConnected = true;
        }
        catch (SocketException ex)
        {
            _logger.LogWarning($"Failed to bind to sender endpoint: {endpoint}. {ex.Message}");
        }
        finally
        {
            _cts = new CancellationTokenSource();
        }
    }

    public async Task Send(OscMessage message, CancellationToken ct)
    {
        var packet = message.ToPacket();
        var length = OSCEncoder.GetEncodedLength(packet);
        if (length > _sendBuffer.Length)
        {
            _logger.LogError("OSC message too large to send! Skipping this batch of messages.");
            return;
        }

        OSCEncoder.Encode(packet, _sendBuffer);
        await _sendSocket.SendAsync(_sendBuffer.AsMemory(0, length));
        OnMessagesDispatched(1);
    }

    public async Task Send(IReadOnlyList<OscMessage> messages, CancellationToken ct)
    {
        // Pack messages into as few bundles as possible, each fitting in a single send buffer
        var batch = new List<IOSCPacket>();
        var batchLength = BundleHeaderLength;
        var sent = 0;

        foreach (var message in messages)
        {
            var packet = message.ToPacket();
            var elementLength = OSCEncoder.GetEncodedLength(packet) + 4; // +4 for bundle element length
            if (BundleHeaderLength + elementLength > _sendBuffer.Length)
            {
                _logger.LogError($"OSC message {packet.Address} too large to send! Skipping it.");
                continue;
            }

            if (batchLength + elementLength > _sendBuffer.Length)
            {
                sent += await SendBundle(batch);
                batch.Clear();
                batchLength = BundleHeaderLength;
            }

            batch.Add(packet);
            batchLength += elementLength;
        }

        if (batch.Count > 0)
        {
            sent += await SendBundle(batch);
        }

        OnMessagesDispatched(sent);
    }

    private async Task<int> SendBundle(List<IOSCPacket> packets)
    {
        var bundle = new OSCBundle(new OSCTimeTag(ImmediateTimeTag), packets.ToArray());
        var length = OSCEncoder.GetEncodedLength(bundle);
        OSCEncoder.Encode(bundle, _sendBuffer);
        await _sendSocket.SendAsync(_sendBuffer.AsMemory(0, length));
        return packets.Count;
    }
}
