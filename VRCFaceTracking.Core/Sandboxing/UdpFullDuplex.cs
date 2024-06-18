using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Sandboxing;
public class UdpFullDuplex : IDisposable
{
    public int Port
    {
        get; private set;
    }

    private object              _callbackLock;

    protected UdpClient         _receivingUdpClient;
    private IPEndPoint          _remoteIpEndPoint;
    private Queue<byte[]>       _queue;
    private ManualResetEvent    _closingEvent;
    private bool                _closing             = false;

    public UdpFullDuplex(int port, IPEndPoint remoteIpEndPoint = null)
    {
        Port = port;
        _queue = new Queue<byte[]>();
        _closingEvent = new ManualResetEvent(false);
        _callbackLock = new object();

        // try to open the port 10 times, else fail
        for ( int i = 0; i < 10; i++ )
        {
            try
            {
                _receivingUdpClient = new UdpClient(port);
                break;
            } catch ( Exception )
            {
                // Failed in ten tries, throw the exception and give up
                if ( i >= 9 )
                    throw;

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
            _closingEvent.Set();
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
            throw new Exception("UDPListener has been closed.");

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

    public void SendData(in byte[] message, in IPEndPoint remoteEndpoint)
    {
        _receivingUdpClient.Send(message, message.Length, remoteEndpoint);
    }

    public void SendData(in IpcPacket packet, in IPEndPoint remoteEndpoint)
    {
        SendData(packet.GetBytes(), remoteEndpoint);
    }

    public virtual void OnBytesReceived(in byte[] data, in IPEndPoint endpoint)
    {
        // @NOTE: Here for a class to extend and read data and handle it as needed
    }
}
