using System;
using System.Net;
using System.Net.Sockets;

public class BangClient
{
    public static string playerName = string.Empty;
    public static int playerNumber = 0;
    public static string ip = string.Empty;
    public static bool isOwner = false;

    public bool isConnecting { private set; get; }
    
    private Socket client;
    private int byteSize;

    public delegate void EnQueueInMessageReceiver(byte[] message);
    public EnQueueInMessageReceiver enQueueInMessageReceiver = null;

    public BangClient ()
    {
        isConnecting = false;
        byteSize = 512;
    }

    public void Connect ()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), 9898);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.RemoteEndPoint = endPoint;
        args.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectCompleted);

        client.ConnectAsync(args);
    }

    public void SendToServer(byte[] message)
    {
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.SetBuffer(message, 0, message.Length);
        client.SendAsync(args);
    }

    private void ConnectCompleted (object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[byteSize], 0, byteSize);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveCompleted);

            SendToServer(MessageManager.MakeByteMessage(Header.ClientJoin, playerName));

            isConnecting = true;
            client.ReceiveAsync(args);
        }
    }

    private void ReceiveCompleted (object sender, SocketAsyncEventArgs e)
    {
        if (e.BytesTransferred > 0)
        {            
            enQueueInMessageReceiver(e.Buffer);

            e.SetBuffer(new byte[byteSize], 0, byteSize);
            client.ReceiveAsync(e);
        }
    }
}