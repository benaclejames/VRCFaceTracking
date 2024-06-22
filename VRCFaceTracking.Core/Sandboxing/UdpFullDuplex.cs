using System.Net.Sockets;
using System.Net;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Sandboxing;
public class UdpFullDuplex : IDisposable
{
    public int Port
    {
        get; protected set;
    }

    private object              _callbackLock;

    protected UdpClient         _receivingUdpClient;
    private IPEndPoint          _remoteIpEndPoint;
    private Queue<byte[]>       _queue;
    private ManualResetEvent    _closingEvent;
    private bool                _closing             = false;
    protected bool              _isConnected         = false;
    protected SimpleEventBus    _eventBus;

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

        // setup first async event
        AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
        _receivingUdpClient.BeginReceive(callBack, null);
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        Monitor.Enter(_callbackLock);
        Byte[] bytes = null;

        try
        {
            bytes = _receivingUdpClient.EndReceive(result, ref _remoteIpEndPoint);
        } catch ( ObjectDisposedException )
        {
            // Ignore if disposed. This happens when closing the listener
        } catch ( SocketException )
        {
            // This happens when a module terminates / crashes / is shut down
        }

        // Process bytes
        if ( bytes != null && bytes.Length > 0 )
        {
            OnBytesReceived(in bytes, in _remoteIpEndPoint);
        }

        if ( _closing )
        {
            _closingEvent.Set();
        }
        else
        {
            // Setup next async event
            AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
            _receivingUdpClient.BeginReceive(callBack, null);
        }
        Monitor.Exit(_callbackLock);
    }

    public void Close()
    {
        lock ( _callbackLock )
        {
            _closingEvent.Reset();
            _closing = true;
            _receivingUdpClient.Close();
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
            SendData(packet.GetBytes(), remoteEndpoint);
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
            SendData(packet.GetBytes(), new IPEndPoint(IPAddress.Loopback, remotePort));
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
