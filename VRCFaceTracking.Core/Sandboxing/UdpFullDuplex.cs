using System.Net.Sockets;
using System.Net;
using VRCFaceTracking.Core.Sandboxing.IPC;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace VRCFaceTracking.Core.Sandboxing;

public delegate void OnReceiveShouldBeQueued();

public class UdpFullDuplex : IDisposable
{
    const int SIO_UDP_CONNRESET = -1744830452;
    const int ETHERNET_FRAME_SIZE = 1500;
    const int TIMEOUT_MILLISECONDS = 10000;

    public int Port
    {
        get; protected set;
    }

    private object              _callbackLock;

    protected UdpClient         _receivingUdpClient;
    private IPEndPoint          _remoteIpEndPoint;
    private Queue<byte[]>       _queue;
    private ManualResetEvent    _closingEvent;
    private bool                _closing                = false;
    protected bool              _isConnected            = false;
    protected SimpleEventBus    _eventBus;
    private bool                _isCallbackRegistered   = false;
    private Thread              _receiveThread;
    private int                 _maximumTransferUnit = ETHERNET_FRAME_SIZE;
    private CancellationTokenSource _cts = new();
    public OnReceiveShouldBeQueued OnReceiveShouldBeQueued;
    public int MTU => _maximumTransferUnit;

    public UdpFullDuplex(int port, int[] reservedPorts = null, IPEndPoint remoteIpEndPoint = null)
    {
        Port = port;
        _queue = new Queue<byte[]>();
        _closingEvent = new ManualResetEvent(false);
        _callbackLock = new object();
        _eventBus = new SimpleEventBus();

        // try to open the port 10 times, else fail
        for ( int i = 0; i < 10; i++ )
        {
            try
            {
                _receivingUdpClient = new UdpClient(port);
                // Disable crash from ICMP messages from modules which crashed. Windows only
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _receivingUdpClient.Client.IOControl(( IOControlCode )SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
                }
                _receivingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Blacklist any reserved ports
                if ( reservedPorts != null )
                {
                    for ( int j = 0; j < reservedPorts.Length; j++ )
                    {
                        if ( ( ( IPEndPoint )_receivingUdpClient.Client.LocalEndPoint ).Port == reservedPorts[j] )
                        {
                            _receivingUdpClient.Close();
                            continue;
                        }
                    }
                }

                break;
            }
            catch ( Exception )
            {
                // Failed in ten tries, throw the exception and give up
                if ( i >= 9 )
                {
                    throw;
                }

                Thread.Sleep(5);
            }
        }

        // Receive from any IP on any port
        if ( remoteIpEndPoint == null )
        {
            _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }
        else
        {
            _remoteIpEndPoint = remoteIpEndPoint;
        }

        _receivingUdpClient.Client.ReceiveTimeout   = 10;
        _receivingUdpClient.Client.SendTimeout      = 10;
        _maximumTransferUnit = Math.Min(_receivingUdpClient.Client.ReceiveBufferSize, _receivingUdpClient.Client.SendBufferSize);
        
        // setup first async event
        // AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
        // _receivingUdpClient.BeginReceive(callBack, null);
        _receiveThread = new Thread(new ThreadStart(Listen))
        {
            IsBackground = true
        };
        _receiveThread.Start();
    }

    private void Listen()
    {
        while ( !_cts.IsCancellationRequested )
        {
            try
            {
                using CancellationTokenSource internalCancellationTokenSource = new();
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(internalCancellationTokenSource.Token, _cts.Token);

                internalCancellationTokenSource.CancelAfter(TIMEOUT_MILLISECONDS);
                var task = _receivingUdpClient.ReceiveAsync(_cts.Token);
                var index = Task.WaitAny(new [] { task.AsTask() }, TIMEOUT_MILLISECONDS, _cts.Token);
                if ( index < 0 )
                    continue;

                var result = task.Result;

                if ( result.Buffer != null && result.Buffer.Length > 0 )
                {
                    OnBytesReceived(result.Buffer, result.RemoteEndPoint);
                }
            }
            catch ( OperationCanceledException )
            {
                // Cancelled, exit loop
                break;
            }
            catch ( ObjectDisposedException )
            {
                // Ignore if disposed. This happens when closing the listener
            }
            catch ( SocketException e )
            {
                // This happens when a module terminates / crashes / is shut down
            }
        }
    }

    private void ReceiveThread()
    {

        EndPoint remoteEndpoint = (EndPoint) _remoteIpEndPoint;
        while ( !_cts.IsCancellationRequested )
        {
            byte[] receiveWindow = new byte[4096];
            int res = 0;

            Monitor.Enter(_callbackLock);

            try
            {
                res = _receivingUdpClient.Client.ReceiveFrom(receiveWindow, ref remoteEndpoint);
            } catch ( ObjectDisposedException )
            {
                // Ignore if disposed. This happens when closing the listener
            } catch ( SocketException )
            {
                // This happens when a module terminates / crashes / is shut down

            }

            // Process bytes
            if ( receiveWindow != null && receiveWindow.Length > 0 )
            {
                OnBytesReceived(in receiveWindow, in _remoteIpEndPoint);
            }

            if ( _closing )
            {
                _closingEvent.Set();
            }
            Monitor.Exit(_callbackLock);
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        Monitor.Enter(_callbackLock);
        Byte[] bytes = null;

        try
        {
            bytes = _receivingUdpClient.EndReceive(result, ref _remoteIpEndPoint);
        }
        catch ( ObjectDisposedException )
        {
            // Ignore if disposed. This happens when closing the listener
        }
        catch ( SocketException )
        {
            // This happens when a module terminates / crashes / is shut down
        }

        _isCallbackRegistered = false;

        // Process bytes
        if ( bytes != null && bytes.Length > 0 && _closing  == false )
        {
            OnBytesReceived(in bytes, in _remoteIpEndPoint);
        }

        if ( _closing )
        {
            _closingEvent.Set();
        }
        else
        {
            if ( OnReceiveShouldBeQueued != null )
            {
                OnReceiveShouldBeQueued();
            }
        }
        Monitor.Exit(_callbackLock);
    }

    public void ReceivePackets()
    {
        // Setup next async event
        AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
        _receivingUdpClient.BeginReceive(callBack, null);
        _isCallbackRegistered = true;
    }

    public void Close()
    {
        _cts.Cancel();
        lock ( _callbackLock )
        {
            _closingEvent.Reset();
            _closing = true;
            _receivingUdpClient.Close();
            _closingEvent.Set();
            _receiveThread.Join();
        }
        _closingEvent.WaitOne();
    }

    public void Dispose()
    {
        this.Close();
    }

    private byte[] ReceiveBytes()
    {
        if ( _closing )
        {
            throw new Exception("UDPListener has been closed.");
        }

        lock ( _queue )
        {
            if ( _queue.Count() > 0 )
            {
                byte[] bytes = _queue.Dequeue();
                return bytes;
            }
            else
                return null;
        }
    }

    private void SendData(in byte[] message, in IPEndPoint remoteEndpoint)
    {
        _receivingUdpClient.Send(message, message.Length, remoteEndpoint);
    }
    private void SendData(in byte[] message, in int remotePort)
    {
        _receivingUdpClient.Send(message, message.Length, new IPEndPoint(IPAddress.Loopback, remotePort));
    }

    public void SendData(in IpcPacket packet, in IPEndPoint remoteEndpoint)
    {
        if ( _isConnected || packet.GetPacketType() == IpcPacket.PacketType.Handshake )
        {
            byte[] packetData = packet.GetBytes();
            if ( packetData.Length > _maximumTransferUnit )
            {
                // @TODO: Split packet into chunks
                byte[][] packetChunkBytes = PartialPacket.SplitPacketIntoChunks(packetData, _maximumTransferUnit);
                foreach ( var packetChunk in packetChunkBytes )
                {
                    SendData(packetChunk, remoteEndpoint);
                }
            }
            else
            {
                SendData(packetData, remoteEndpoint);
            }
        }
        else
        {
            _eventBus.Push(packet);
        }
    }
    
    public void SendData(in IpcPacket packet, in int remotePort)
    {
        if ( _isConnected || packet.GetPacketType() == IpcPacket.PacketType.Handshake )
        {
            byte[] packetData = packet.GetBytes();
            if ( packetData.Length > _maximumTransferUnit )
            {
                // @TODO: Split packet into chunks
                byte[][] packetChunkBytes = PartialPacket.SplitPacketIntoChunks(packetData, _maximumTransferUnit);
                foreach ( var packetChunk in packetChunkBytes )
                {
                    SendData(packetChunk, new IPEndPoint(IPAddress.Loopback, remotePort));
                }
            }
            else
            {
                SendData(packetData, new IPEndPoint(IPAddress.Loopback, remotePort));
            }
        }
        else
        {
            _eventBus.Push(packet);
        }
    }

    public virtual void OnBytesReceived(in byte[] data, in IPEndPoint endpoint)
    {
        // @NOTE: Here for a class to extend and read data and handle it as needed
    }
}
